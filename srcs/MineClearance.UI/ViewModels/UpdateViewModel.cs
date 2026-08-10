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
    /// 刷新悬浮球可见性并处理配置关闭悬浮球时的抽屉兜底, 由设置抽屉切换开关时调用, 下载中兜底弹出抽屉一次
    /// </summary>
    public void RefreshBallVisibility()
    {
        IsBallVisible = _updateService.State is UpdateState.Downloading && _uiOptions.ShowDownloadBall;

        // 悬浮球被禁用时, 如果正在下载中自动弹出抽屉, 保证下载过程有可见反馈
        if (!_uiOptions.ShowDownloadBall && _updateService.State is UpdateState.Downloading && !IsDrawerOpen)
        {
            IsDrawerOpen = true;
        }
    }

    /// <summary>
    /// 打开更新日志文件夹, 由上次更新失败的提示点击调用
    /// </summary>
    public void OpenUpdateLogFolder()
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
        var state = _updateService.State;

        // 正在检查中: 手动触发时提示
        if (state is UpdateState.Checking)
        {
            if (manual) { _toast.Show("已经有更新检查在后台进行"); }
            return;
        }

        // 正在下载中: 手动触发时提示
        if (state is UpdateState.Downloading)
        {
            if (manual) { _toast.Show("正在下载更新, 请稍候再检查"); }
            return;
        }

        // 下载失败: 手动触发时提示, 点击可重新打开抽屉查看异常并重试
        if (state is UpdateState.DownloadFailed)
        {
            if (manual)
            {
                _toast.Show("上次下载失败, 点击查看详情", () => IsDrawerOpen = true);
            }
            return;
        }

        // 下载完成: 手动触发时提示关闭应用后自动更新
        if (state is UpdateState.DownloadCompleted)
        {
            if (manual) { _toast.Show("更新包已下载, 关闭应用后将自动更新"); }
            return;
        }

        // 发起检查请求
        await _updateService.CheckNewestAsync(
            AppMetadata.Get(AppMetadata.AuthorKey),
            AppMetadata.Get(AppMetadata.ProductKey),
            AppMetadata.Get(AppMetadata.VersionKey),
            App.ExitCts.Token
        );

        // 按检查结果反馈
        switch (_updateService.State)
        {
            // 已是最新: 仅手动检查时提示
            case UpdateState.UpToDate:
                if (manual) { _toast.Show("已是最新版本"); }
                break;

            // 发现新版本: 提示并允许点击开始下载
            case UpdateState.NeedUpdate:
                _toast.Show(
                    $"发现新版本 v{_updateService.LatestVersion}, 点击下载更新",
                    () => _ = _updateService.DownloadAsync(App.ExitCts.Token)
                );
                break;

            // 检查失败: 提示异常信息
            case UpdateState.Idle when _updateService.Exception is { } exception:
                _toast.Show($"检查更新失败: {exception.Message}");
                break;
        }
    }

    /// <summary>
    /// 按服务当前状态刷新界面: 处理状态转换反馈并更新悬浮球与抽屉内容
    /// </summary>
    private void RefreshFromService()
    {
        var state = _updateService.State;
        var previous = _previousState;
        _previousState = state;

        // 下载失败: 直接按状态处理, 不依赖前一状态 (瞬间失败时 Downloading 可能被合并刷新跳过)
        if (state is UpdateState.DownloadFailed)
        {
            // 自动弹出抽屉显示异常, 点击 Toast 可随时找回抽屉
            _toast.Show($"下载失败: {_updateService.Exception?.Message}", () => IsDrawerOpen = true);
            IsDrawerOpen = true;
        }
        else if (state is UpdateState.DownloadCompleted)
        {
            // 下载完成: 关闭抽屉并提示关闭应用后自动更新
            _toast.Show($"更新包下载完成 (v{_updateService.LatestVersion}), 关闭应用后将自动更新");
            IsDrawerOpen = false;
        }
        else if (previous is UpdateState.Downloading && state is UpdateState.NeedUpdate)
        {
            // 用户取消: 关闭抽屉并提示
            _toast.Show("下载已取消");
            IsDrawerOpen = false;
        }
        else if (previous is not UpdateState.Downloading && state is UpdateState.Downloading)
        {
            // 下载开始: 悬浮球被禁用时自动弹出抽屉一次, 进度更新不重复弹出
            if (!_uiOptions.ShowDownloadBall && !IsDrawerOpen)
            {
                IsDrawerOpen = true;
            }
        }

        // 刷新悬浮球可见性, 仅下载中且配置允许时显示
        IsBallVisible = _updateService.State is UpdateState.Downloading && _uiOptions.ShowDownloadBall;

        // 刷新抽屉内容
        if (state is UpdateState.Downloading)
        {
            BallFillHeight = (_updateService.ProgressPercentage ?? 0) / Constants.PercentBase * Constants.DownloadBallSize;
        }
        DrawerVersionText = $"v{_updateService.LatestVersion}";
        DrawerProgress = (_updateService.ProgressPercentage ?? 0) / Constants.PercentBase;
        DownloadedText = _updateService.TotalBytes is { } total
            ? _updateService.DownloadedBytes is { } downloaded
                ? $"{FormatBytesValue(downloaded)} / {FormatBytesValue(total)}"
                : $"未知 / {FormatBytesValue(total)}"
            : string.Empty;
        SpeedText = _updateService.SpeedBytesPerSecond is { } speed
            ? $"{FormatBytesValue(speed)}/s"
            : string.Empty;
        IsFailed = state is UpdateState.DownloadFailed;
        ExceptionText = _updateService.Exception?.Message ?? string.Empty;
        IsCancelVisible = state is UpdateState.Downloading;
        StateText = state switch
        {
            UpdateState.Downloading => "正在下载",
            UpdateState.DownloadFailed => "下载失败",
            _ => string.Empty
        };
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

        // 服务事件来自后台线程, 统一编组到 UI 线程刷新
        Dispatcher.UIThread.Post(() =>
        {
            _refreshPending = false;
            RefreshFromService();
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
