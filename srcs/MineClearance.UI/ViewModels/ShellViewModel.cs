using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.UI.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 壳视图模型, 负责主视图/游戏视图/历史记录视图之间的切换与设置抽屉
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    /// <summary>
    /// 主视图模型
    /// </summary>
    public MainViewModel Main { get; }

    /// <summary>
    /// 游戏视图模型
    /// </summary>
    public GameViewModel Game { get; }

    /// <summary>
    /// 历史记录视图模型
    /// </summary>
    public HistoryViewModel History { get; }

    /// <summary>
    /// 全局短暂提示视图模型
    /// </summary>
    public ToastViewModel Toast { get; }

    /// <summary>
    /// 更新视图模型, 负责更新流程与下载悬浮球/下载详情抽屉
    /// </summary>
    public UpdateViewModel Update { get; }

    /// <summary>
    /// 游戏视图透明度, 未显示时为 0 (透明常驻布局以便预热棋盘控件), 显示时为 1
    /// </summary>
    public double GameViewOpacity => IsGameViewVisible ? Constants.MaxRatio : 0;

    /// <summary>
    /// 历史记录视图透明度, 未显示时为 0 (透明常驻布局以便预热表格控件), 显示时为 1
    /// </summary>
    public double HistoryViewOpacity => IsHistoryViewVisible ? Constants.MaxRatio : 0;

    /// <summary>
    /// 当前可见的视图
    /// </summary>
    [ObservableProperty]
    public partial object CurrentView { get; set; }

    /// <summary>
    /// 主视图是否可见
    /// </summary>
    [ObservableProperty]
    public partial bool IsMainViewVisible { get; set; }

    /// <summary>
    /// 游戏视图是否可见
    /// </summary>
    [ObservableProperty]
    public partial bool IsGameViewVisible { get; set; }

    /// <summary>
    /// 历史记录视图是否可见
    /// </summary>
    [ObservableProperty]
    public partial bool IsHistoryViewVisible { get; set; }

    /// <summary>
    /// 设置视图模型, 打开设置抽屉时创建
    /// </summary>
    [ObservableProperty]
    public partial SettingsViewModel? Settings { get; set; }

    /// <summary>
    /// 设置抽屉是否打开
    /// </summary>
    [ObservableProperty]
    public partial bool IsSettingsOpen { get; set; }

    /// <summary>
    /// 设置抽屉是否实际可见, 关闭动画结束后才置为 false
    /// </summary>
    [ObservableProperty]
    public partial bool IsSettingsVisible { get; set; }

    /// <summary>
    /// 设置抽屉透明度, 驱动抽屉淡入淡出
    /// </summary>
    [ObservableProperty]
    public partial double SettingsOpacity { get; set; }

    /// <summary>
    /// 设置抽屉水平偏移, 关闭时滑出到屏幕左侧
    /// </summary>
    [ObservableProperty]
    public partial double SettingsSlideOffset { get; set; } = -Constants.DrawerWidth;

    /// <summary>
    /// 设置抽屉当前宽度, 可由用户拖动右边界调整, 不保存
    /// </summary>
    [ObservableProperty]
    public partial double SettingsDrawerWidth { get; set; } = Constants.DrawerWidth;

    /// <summary>
    /// 共用遮布是否可见, 任一抽屉实际可见时显示
    /// </summary>
    [ObservableProperty]
    public partial bool IsMaskVisible { get; set; }

    /// <summary>
    /// 共用遮布透明度, 驱动遮布淡入淡出
    /// </summary>
    [ObservableProperty]
    public partial double MaskOpacity { get; set; }

    /// <summary>
    /// 请求退出程序的事件, 由视图层关闭主窗口
    /// </summary>
    public event Action? ExitRequested;

    /// <summary>
    /// 因打开抽屉而暂停游戏的计数, 任一抽屉打开时累加, 全部关闭归零时恢复
    /// </summary>
    private int _gamePauseCount;

    /// <summary>
    /// 首次因抽屉暂停前游戏是否已处于暂停状态, 关闭抽屉时据此避免取消用户原有的暂停
    /// </summary>
    private bool _wasPausedBeforeDrawer;

    /// <summary>
    /// 设置抽屉关闭动画的版本号, 防止过期的延迟隐藏任务误关重新打开的抽屉
    /// </summary>
    private int _closeSettingsVersion;

    /// <summary>
    /// 创建壳视图模型
    /// </summary>
    /// <param name="main">主视图模型</param>
    /// <param name="game">游戏视图模型</param>
    /// <param name="history">历史记录视图模型</param>
    /// <param name="toast">全局短暂提示视图模型</param>
    /// <param name="update">更新视图模型</param>
    public ShellViewModel(
        MainViewModel main,
        GameViewModel game,
        HistoryViewModel history,
        ToastViewModel toast,
        UpdateViewModel update)
    {
        Main = main;
        Game = game;
        History = history;
        Toast = toast;
        Update = update;
        CurrentView = main;

        // 订阅主视图的导航请求
        main.NavigationRequested += OnNavigationRequested;

        // 转发主视图的退出请求
        main.ExitRequested += () => ExitRequested?.Invoke();

        // 订阅游戏视图返回主视图的请求
        game.MainViewRequested += ShowMainView;

        // 订阅历史记录视图返回主视图的请求
        history.MainViewRequested += ShowMainView;

        // 订阅下载抽屉的打开/关闭, 同步共用遮布状态
        update.PropertyChanged += OnUpdatePropertyChanged;
    }

    /// <summary>
    /// 当前视图变化时同步各视图的可见性, 视图常驻可视树以便复用控件
    /// </summary>
    /// <param name="value">新的当前视图</param>
    partial void OnCurrentViewChanged(object value)
    {
        IsMainViewVisible = value is MainViewModel;
        IsGameViewVisible = value is GameViewModel;
        IsHistoryViewVisible = value is HistoryViewModel;
    }

    /// <summary>
    /// 游戏视图可见性变化时同步透明度
    /// </summary>
    /// <param name="value">新的可见性</param>
    partial void OnIsGameViewVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(GameViewOpacity));
    }

    /// <summary>
    /// 历史记录视图可见性变化时同步透明度
    /// </summary>
    /// <param name="value">新的可见性</param>
    partial void OnIsHistoryViewVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(HistoryViewOpacity));
    }

    /// <summary>
    /// 切换到主视图, 并刷新存档状态
    /// </summary>
    [RelayCommand]
    private void ShowMainView()
    {
        Main.RefreshSaveDataState();
        CurrentView = Main;
    }

    /// <summary>
    /// 切换到游戏视图
    /// </summary>
    private void ShowGameView()
    {
        CurrentView = Game;
    }

    /// <summary>
    /// 切换到历史记录视图, 游戏结果数据延迟到空闲时刷新, 避免同步重建造成卡顿
    /// </summary>
    private void ShowHistoryView()
    {
        CurrentView = History;
        Dispatcher.UIThread.Post(History.Refresh, DispatcherPriority.Background);
    }

    /// <summary>
    /// 处理主视图的导航请求
    /// </summary>
    /// <param name="target">导航目标</param>
    private void OnNavigationRequested(NavigationTarget target)
    {
        switch (target)
        {
            case NavigationTarget.GameView: ShowGameView(); break;
            case NavigationTarget.HistoryView: ShowHistoryView(); break;
            case NavigationTarget.SettingsDrawer: OpenSettings(); break;
        }
    }

    /// <summary>
    /// 打开设置抽屉: 游戏视图打开时自动暂停游戏, 并记录暂停来源以便关闭时恢复, 抽屉滑入淡入
    /// </summary>
    private void OpenSettings()
    {
        PauseGameForDrawer();

        Settings = App.Services.GetRequiredService<SettingsViewModel>();
        Settings.CloseRequested += CloseSettings;

        IsSettingsOpen = true;
        IsSettingsVisible = true;
        SettingsOpacity = Constants.MaxRatio;
        SettingsSlideOffset = 0;
        RefreshMask();
    }

    /// <summary>
    /// 任一抽屉打开时暂停游戏: 计数从 0 到 1 时记录原始暂停状态并真正暂停, 之后重复打开只累加计数
    /// </summary>
    private void PauseGameForDrawer()
    {
        if (!IsGameViewVisible) { return; }
        if (_gamePauseCount++ == 0)
        {
            _wasPausedBeforeDrawer = Game.IsPaused;
            Game.PauseIfPerformable();
        }
    }

    /// <summary>
    /// 任一抽屉关闭时恢复游戏: 所有抽屉都关闭后才真正恢复, 游戏原本就暂停时保持暂停
    /// </summary>
    private void ResumeGameForDrawer()
    {
        if (_gamePauseCount > 0 && --_gamePauseCount == 0 && !_wasPausedBeforeDrawer)
        {
            Game.ResumeIfPaused();
        }
    }

    /// <summary>
    /// 刷新共用遮布: 任一抽屉打开时淡入, 全部关闭后等动画结束淡出隐藏
    /// </summary>
    private void RefreshMask()
    {
        IsMaskVisible = IsSettingsVisible || Update.IsDrawerVisible;
        MaskOpacity = IsSettingsOpen || Update.IsDrawerOpen ? Constants.MaxRatio : 0;
    }

    /// <summary>
    /// 设置抽屉滑出动画结束后隐藏抽屉, 期间重新打开时跳过
    /// </summary>
    private async Task HideSettingsAfterAnimationAsync()
    {
        var version = ++_closeSettingsVersion;
        await Task.Delay(Constants.DrawerAnimationDurationMilliseconds);

        // 版本不匹配或抽屉已重新打开时不隐藏
        if (version != _closeSettingsVersion || IsSettingsOpen) { return; }
        IsSettingsVisible = false;
        Settings?.CloseRequested -= CloseSettings;
        Settings = null;
        RefreshMask();
    }

    /// <summary>
    /// 关闭设置抽屉: 抽屉滑出淡出, 动画结束后隐藏, 并恢复因打开抽屉而暂停的游戏
    /// </summary>
    public void CloseSettings()
    {
        // 未打开时忽略, 防止动画延迟期间的重复调用
        if (!IsSettingsOpen) { return; }

        IsSettingsOpen = false;
        SettingsOpacity = 0;
        SettingsSlideOffset = -SettingsDrawerWidth;
        RefreshMask();

        ResumeGameForDrawer();

        _ = HideSettingsAfterAnimationAsync();
    }

    /// <summary>
    /// 壳视图可用宽度变化时钳制各抽屉当前宽度, 防止抽屉超出壳视图范围 (窗口变窄时压缩, 变宽时保持用户拖动设定的宽度)
    /// </summary>
    /// <param name="availableWidth">壳视图当前可用宽度</param>
    public void ClampDrawerWidths(double availableWidth)
    {
        if (SettingsDrawerWidth > availableWidth)
        {
            SettingsDrawerWidth = availableWidth;
        }
        if (Update.DrawerWidth > availableWidth)
        {
            Update.DrawerWidth = availableWidth;
        }
    }

    /// <summary>
    /// 处理 Esc 键: 下载抽屉可见时隐藏它, 否则交给设置抽屉的开关
    /// </summary>
    public void HandleEscapeKey()
    {
        if (Update.IsDrawerVisible)
        {
            Update.CloseDrawer();
        }
        else if (IsSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    /// <summary>
    /// 启动更新流程, 由窗口首次打开时调用
    /// </summary>
    public void StartUpdateRoutine()
    {
        Update.StartUpdateRoutine();
    }

    /// <summary>
    /// 更新视图模型属性变化时同步共用遮布状态, 下载抽屉打开时暂停游戏
    /// </summary>
    /// <param name="sender">更新视图模型</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnUpdatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(UpdateViewModel.IsDrawerOpen))
        {
            // 下载抽屉打开时暂停游戏, 关闭时恢复
            if (Update.IsDrawerOpen)
            {
                PauseGameForDrawer();
            }
            else
            {
                ResumeGameForDrawer();
            }
            RefreshMask();
        }
        else if (e.PropertyName is nameof(UpdateViewModel.IsDrawerVisible))
        {
            RefreshMask();
        }
    }
}
