using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MineClearance.Infrastructure;
using MineClearance.Infrastructure.Models;
using MineClearance.UI.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 更新视图模型, 协调更新服务与界面: 启动更新流程/检查更新/下载悬浮球与下载详情抽屉/全局提示
/// </summary>
public sealed partial class UpdateViewModel : ObservableObject
{
    /// <summary>
    /// 字节单位换算基数
    /// </summary>
    private const long BytesBase = 1024;

    /// <summary>
    /// 字节的单位数组
    /// </summary>
    private static readonly string[] BytesUnits = ["B", "KB", "MB", "GB"];

    /// <summary>
    /// 首次启动应用时的提示文本
    /// </summary>
    private static readonly string FirstLaunchTipText = $"欢迎使用 {nameof(MineClearance)}!\n" +
        $"本程序由 {AppMetadata.Get(AppMetadata.AuthorKey)} 开发\n" +
        "任意界面按下 Esc 键可打开设置抽屉\n" +
        "支持通过设置种子来生成固定雷区, 但需要确保难度和首次点击位置一致\n" +
        "应用启动时会自动检查更新, 发现新版本后可通过提示下载\n" +
        $"应用的全部数据保存在 {Infrastructure.Constants.AppDataRootDirectory}";

    /// <summary>
    /// 更新服务, 其属性变化事件驱动界面刷新
    /// </summary>
    private readonly IUpdateService _updateService;

    /// <summary>
    /// 全局短暂提示视图模型, 用于更新流程的各类反馈
    /// </summary>
    private readonly ToastViewModel _toast;

    /// <summary>
    /// UI 配置, 用于读取是否显示下载悬浮球
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 上一次观察到的服务状态, 用于识别状态转换
    /// </summary>
    private UpdateState _previousState;

    /// <summary>
    /// 是否有一次刷新排队中, 用于合并高频进度事件的多次刷新
    /// </summary>
    private volatile bool _refreshPending;

    /// <summary>
    /// 抽屉关闭动画的版本号, 防止过期的延迟隐藏任务误关重新打开的抽屉
    /// </summary>
    private int _closeDrawerVersion;

    /// <summary>
    /// 下载悬浮球是否可见, 仅下载中且配置允许时显示
    /// </summary>
    [ObservableProperty]
    public partial bool IsBallVisible { get; set; }

    /// <summary>
    /// 悬浮球填充高度 (像素), 驱动实心球体从底部向上填充
    /// </summary>
    [ObservableProperty]
    public partial double BallFillHeight { get; set; }

    /// <summary>
    /// 下载抽屉逻辑状态, 由状态机控制, 变化时驱动抽屉滑入滑出
    /// </summary>
    [ObservableProperty]
    public partial bool IsDrawerOpen { get; set; }

    /// <summary>
    /// 下载抽屉是否实际可见, 关闭动画结束后才置为 false
    /// </summary>
    [ObservableProperty]
    public partial bool IsDrawerVisible { get; set; }

    /// <summary>
    /// 下载抽屉透明度, 驱动抽屉淡入淡出
    /// </summary>
    [ObservableProperty]
    public partial double DrawerOpacity { get; set; }

    /// <summary>
    /// 下载抽屉水平偏移, 关闭时滑出到屏幕左侧
    /// </summary>
    [ObservableProperty]
    public partial double DrawerSlideOffset { get; set; } = -Constants.DrawerWidth;

    /// <summary>
    /// 下载抽屉当前宽度, 可由用户拖动右边界调整, 不保存
    /// </summary>
    [ObservableProperty]
    public partial double DrawerWidth { get; set; } = Constants.DrawerWidth;

    /// <summary>
    /// 抽屉中显示的版本文本
    /// </summary>
    [ObservableProperty]
    public partial string DrawerVersionText { get; set; } = string.Empty;

    /// <summary>
    /// 抽屉中显示的下载进度比例 (0-1)
    /// </summary>
    [ObservableProperty]
    public partial double DrawerProgress { get; set; }

    /// <summary>
    /// 抽屉中显示的已下载/总大小文本
    /// </summary>
    [ObservableProperty]
    public partial string DownloadedText { get; set; } = string.Empty;

    /// <summary>
    /// 抽屉中显示的下载速度文本
    /// </summary>
    [ObservableProperty]
    public partial string SpeedText { get; set; } = string.Empty;

    /// <summary>
    /// 抽屉中显示的当前状态文本
    /// </summary>
    [ObservableProperty]
    public partial string StateText { get; set; } = string.Empty;

    /// <summary>
    /// 是否处于下载失败状态, 控制异常信息区的显示
    /// </summary>
    [ObservableProperty]
    public partial bool IsFailed { get; set; }

    /// <summary>
    /// 下载失败时的异常信息文本
    /// </summary>
    [ObservableProperty]
    public partial string ExceptionText { get; set; } = string.Empty;

    /// <summary>
    /// 取消按钮是否可见, 仅下载中显示
    /// </summary>
    [ObservableProperty]
    public partial bool IsCancelVisible { get; set; }

    /// <summary>
    /// 创建更新视图模型
    /// </summary>
    /// <param name="updateService">更新服务</param>
    /// <param name="toast">全局短暂提示视图模型</param>
    /// <param name="uiOptions">UI 配置</param>
    public UpdateViewModel(
        IUpdateService updateService,
        ToastViewModel toast,
        UIOptions uiOptions)
    {
        _updateService = updateService;
        _toast = toast;
        _uiOptions = uiOptions;

        // 订阅服务属性变化, 进度与状态事件均从后台线程触发, 刷新时统一编组到 UI 线程
        _updateService.PropertyChanged += OnUpdateServicePropertyChanged;
    }

    /// <summary>
    /// 下载抽屉逻辑状态变化时驱动滑入滑出动画
    /// </summary>
    /// <param name="value">新的逻辑状态</param>
    partial void OnIsDrawerOpenChanged(bool value)
    {
        if (value)
        {
            // 打开: 立即显示并滑入淡入
            IsDrawerVisible = true;
            DrawerOpacity = Constants.MaxRatio;
            DrawerSlideOffset = 0;
        }
        else
        {
            // 关闭: 滑出淡出, 动画结束后隐藏, 滑出距离与当前宽度一致
            DrawerOpacity = 0;
            DrawerSlideOffset = -DrawerWidth;
            _ = HideDrawerAfterAnimationAsync();
        }
    }

    /// <summary>
    /// 取消当前下载, 取消后状态机自动关闭抽屉并提示
    /// </summary>
    [RelayCommand]
    private void CancelDownload()
    {
        _updateService.CancelDownload();
    }

    /// <summary>
    /// 启动更新流程: 消费上次更新的结果并后台检查更新, 由窗口首次打开时调用
    /// </summary>
    public void StartUpdateRoutine()
    {
        _ = RunStartupAsync();
    }

    /// <summary>
    /// 关闭下载抽屉, 由 Esc 键与遮布点击调用, 滑出动画结束后隐藏
    /// </summary>
    public void CloseDrawer()
    {
        IsDrawerOpen = false;
    }

    /// <summary>
    /// 呼出或关闭下载抽屉, 由悬浮球点击调用
    /// </summary>
    public void ToggleDrawer()
    {
        IsDrawerOpen = !IsDrawerOpen;
    }

    /// <summary>
    /// 刷新悬浮球可见性
    /// </summary>
    public void RefreshBallVisibility()
    {
        var isDownloading = _updateService.State is UpdateState.Downloading;
        IsBallVisible = _uiOptions.ShowDownloadBall && isDownloading;

        // 悬浮球被禁用时, 如果正在下载中自动弹出抽屉, 保证下载过程有可见反馈
        if (!_uiOptions.ShowDownloadBall && isDownloading && !IsDrawerOpen)
        {
            IsDrawerOpen = true;
        }
    }

    /// <summary>
    /// 清除更新缓存, 由设置界面按钮触发, 失败时通过 Toast 提示
    /// </summary>
    public void ClearUpdateCache()
    {
        try
        {
            if (Directory.Exists(Infrastructure.Constants.UpdateDataDirectory))
            {
                Directory.Delete(Infrastructure.Constants.UpdateDataDirectory, recursive: true);
            }
            _toast.Show("更新缓存已清除");
        }
        catch (Exception ex)
        {
            _toast.Show($"清除更新缓存失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查更新, 手动检查时已是最新也提示, 启动检查时静默
    /// </summary>
    /// <param name="manual">是否由用户手动触发</param>
    public async Task CheckForUpdatesAsync(bool manual)
    {
        // 如果已有检查或下载在进行中, 则不发起新请求, 手动检查时提示
        switch (_updateService.State)
        {
            case UpdateState.Checking:
                if (manual) { _toast.Show("已有更新检查在后台进行"); }
                return;

            case UpdateState.Downloading:
                if (manual) { _toast.Show("正在后台下载更新, 请稍候再检查"); }
                return;
        }

        // 发起检查请求
        await _updateService.CheckNewestAsync(
            AppMetadata.Get(AppMetadata.AuthorKey),
            AppMetadata.Get(AppMetadata.ProductKey),
            AppMetadata.Get(AppMetadata.VersionKey),
            App.ExitCts.Token
        );

        // 请求完成后处理状态转换反馈
        HandleStateTransition(_updateService.State, manual);
    }

    /// <summary>
    /// 打开更新日志文件夹, 由上次更新失败的提示点击调用
    /// </summary>
    private void OpenUpdateLogFolder()
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = Infrastructure.Constants.UpdateDataDirectory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _toast.Show($"打开更新日志文件夹失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理更新服务的状态转换
    /// </summary>
    /// <param name="state">当前状态</param>
    /// <param name="isManualCheck">是否由用户手动触发检查更新</param>
    private void HandleStateTransition(UpdateState state, bool isManualCheck)
    {
        var previous = _previousState;
        if (state == _previousState) { return; }
        _previousState = state;

        // 处理状态转换反馈: 按当前状态匹配, 对依赖前一状态的分支用 when 过滤
        switch (state)
        {
            case UpdateState.Idle or UpdateState.Checking:
                break;

            case UpdateState.UpToDate when isManualCheck:
                _toast.Show($"当前版本 v{AppMetadata.Get(AppMetadata.VersionKey)} 已是最新版本");
                break;

            case UpdateState.NeedUpdate when previous is UpdateState.Checking:
                _toast.Show(
                    $"发现新版本 v{_updateService.LatestVersion}, 点击下载更新",
                    () => _ = _updateService.DownloadAsync(App.ExitCts.Token)
                );
                break;

            case UpdateState.NeedUpdate when previous is UpdateState.Downloading:
                IsCancelVisible = false;
                IsFailed = false;
                StateText = "下载已取消";
                IsBallVisible = false;
                IsDrawerOpen = false;
                _toast.Show("下载已取消");
                break;

            case UpdateState.CheckFailed when isManualCheck:
                _toast.Show($"检查更新失败: {_updateService.Exception.Message}");
                break;

            case UpdateState.Downloading:
                DrawerVersionText = $"v{_updateService.LatestVersion}";
                ExceptionText = string.Empty;
                IsCancelVisible = true;
                IsFailed = false;
                StateText = "正在下载";
                RefreshFromDownloadProgress(state);
                if (_uiOptions.ShowDownloadBall) { IsBallVisible = true; }
                else { IsDrawerOpen = true; }
                break;

            case UpdateState.DownloadCompleted:
                IsCancelVisible = false;
                IsFailed = false;
                StateText = "下载完成";
                IsBallVisible = false;
                IsDrawerOpen = false;
                _toast.Show($"更新包 (v{_updateService.LatestVersion})已下载完成, 关闭应用后将自动更新");
                break;

            case UpdateState.DownloadFailed:
                IsCancelVisible = false;
                IsFailed = true;
                StateText = "下载失败";
                ExceptionText = _updateService.Exception.ToString();
                if (_uiOptions.ShowDownloadBall)
                {
                    IsBallVisible = false;
                    _toast.Show(
                        $"下载更新失败: {_updateService.Exception.Message}, 点击查看错误详情",
                        () => IsDrawerOpen = true
                    );
                }
                else
                {
                    IsDrawerOpen = true;
                    _toast.Show("下载更新失败");
                }
                break;
        }
    }

    /// <summary>
    /// 下载进度变化时刷新界面, 由服务属性变化事件触发, 高频事件合并刷新
    /// </summary>
    /// <param name="state">当前状态</param>
    private void RefreshFromDownloadProgress(UpdateState state)
    {
        var percent = _updateService.ProgressPercentage / Infrastructure.Constants.PercentBase;
        IsBallVisible = _uiOptions.ShowDownloadBall && state is UpdateState.Downloading;
        BallFillHeight = percent * Constants.DownloadBallSize;
        DrawerProgress = percent;
        DownloadedText = $"{FormatBytesValue(_updateService.DownloadedBytes)} / {FormatBytesValue(_updateService.TotalBytes)}";
        SpeedText = $"{FormatBytesValue(_updateService.SpeedBytesPerSecond)}/s";
    }

    /// <summary>
    /// 启动更新流程: 读取上次更新信息并提示, 然后后台检查更新
    /// </summary>
    private async Task RunStartupAsync()
    {
        // 首次启动提示: 介绍自动更新功能, 展示后自动关闭该配置
        if (_uiOptions.ShowFirstLaunchTip)
        {
            _toast.Show(FirstLaunchTipText);
            _uiOptions.ShowFirstLaunchTip = false;
        }

        // 读取上次更新信息并清理, 仅在有结果时提示, 失败时可点击打开日志目录查看
        var info = _updateService.GetLastUpdateInfoAndCleanUp();

        // 上次更新有结果时提示, 失败时可点击打开日志目录查看
        if (info is not null)
        {
            if (info.IsSuccess)
            {
                _toast.Show($"更新成功: v{info.OriginalVersion} -> v{info.NewVersion}");
            }
            else
            {
                _toast.Show("上次更新失败, 点击查看更新日志", OpenUpdateLogFolder);
            }
        }

        // 后台检查更新, 启动检查不提示已是最新
        await CheckForUpdatesAsync(manual: false);
    }

    /// <summary>
    /// 抽屉滑出动画结束后隐藏抽屉, 期间重新打开时跳过
    /// </summary>
    private async Task HideDrawerAfterAnimationAsync()
    {
        var version = ++_closeDrawerVersion;
        await Task.Delay(Constants.DrawerAnimationDurationMilliseconds);

        // 版本不匹配或抽屉已重新打开时不隐藏
        if (version != _closeDrawerVersion || IsDrawerOpen) { return; }
        IsDrawerVisible = false;
    }

    /// <summary>
    /// 服务属性变化时合并刷新请求, 高频进度事件每帧只刷新一次
    /// </summary>
    /// <param name="sender">更新服务</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnUpdateServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_refreshPending) { return; }
        _refreshPending = true;

        // 捕获当前状态, 避免刷新时状态已变化导致的错误
        var state = _updateService.State;

        // 服务事件来自后台线程, 统一编组到 UI 线程刷新
        Dispatcher.UIThread.Post(() =>
        {
            _refreshPending = false;
            if (e.PropertyName is nameof(IUpdateService.State))
            {
                // 从检查中退出的状态变化应该由 CheckForUpdatesAsync 处理以正确区分手动检查与自动检查
                if (_previousState is not UpdateState.Checking)
                {
                    HandleStateTransition(state, isManualCheck: false);
                }
            }
            else
            {
                RefreshFromDownloadProgress(state);
            }
        });
    }

    /// <summary>
    /// 格式化字节数为带单位的文本
    /// </summary>
    /// <param name="bytes">字节数</param>
    /// <returns>格式化的字节文本</returns>
    private static string FormatBytesValue(double bytes)
    {
        var unitIndex = 0;
        while (bytes >= BytesBase && unitIndex < BytesUnits.Length - 1)
        {
            bytes /= BytesBase;
            unitIndex++;
        }
        return $"{bytes:F2} {BytesUnits[unitIndex]}";
    }
}
