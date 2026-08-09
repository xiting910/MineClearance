using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.UI.Models;
using System;

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
    /// 游戏视图透明度, 未显示时为 0 (透明常驻布局以便预热棋盘控件), 显示时为 1
    /// </summary>
    public double GameViewOpacity => IsGameViewVisible ? 1.0 : 0.0;

    /// <summary>
    /// 历史记录视图透明度, 未显示时为 0 (透明常驻布局以便预热表格控件), 显示时为 1
    /// </summary>
    public double HistoryViewOpacity => IsHistoryViewVisible ? 1.0 : 0.0;

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
    /// 请求退出程序的事件, 由视图层关闭主窗口
    /// </summary>
    public event Action? ExitRequested;

    /// <summary>
    /// 是否因打开设置抽屉而暂停了游戏, 关闭抽屉时据此恢复
    /// </summary>
    private bool _isGamePausedByDrawer;

    /// <summary>
    /// 创建壳视图模型
    /// </summary>
    /// <param name="main">主视图模型</param>
    /// <param name="game">游戏视图模型</param>
    /// <param name="history">历史记录视图模型</param>
    /// <param name="toast">全局短暂提示视图模型</param>
    public ShellViewModel(
        MainViewModel main,
        GameViewModel game,
        HistoryViewModel history,
        ToastViewModel toast)
    {
        Main = main;
        Game = game;
        History = history;
        Toast = toast;
        CurrentView = main;

        // 订阅主视图的导航请求
        main.NavigationRequested += OnNavigationRequested;

        // 转发主视图的退出请求
        main.ExitRequested += () => ExitRequested?.Invoke();

        // 订阅游戏视图返回主视图的请求
        game.MainViewRequested += ShowMainView;

        // 订阅历史记录视图返回主视图的请求
        history.MainViewRequested += ShowMainView;
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
    /// 打开设置抽屉: 游戏视图打开时自动暂停游戏, 并记录暂停来源以便关闭时恢复
    /// </summary>
    private void OpenSettings()
    {
        _isGamePausedByDrawer = IsGameViewVisible && Game.PauseIfPerformable();

        Settings = App.Services.GetRequiredService<SettingsViewModel>();
        Settings.CloseRequested += CloseSettings;

        IsSettingsOpen = true;
    }

    /// <summary>
    /// 关闭设置抽屉, 并恢复因打开抽屉而暂停的游戏
    /// </summary>
    public void CloseSettings()
    {
        IsSettingsOpen = false;
        Settings?.CloseRequested -= CloseSettings;
        Settings = null;

        if (_isGamePausedByDrawer)
        {
            Game.ResumeIfPaused();
            _isGamePausedByDrawer = false;
        }
    }

    /// <summary>
    /// 呼出或隐藏设置抽屉, 供 Esc 键调用
    /// </summary>
    public void ToggleSettings()
    {
        if (IsSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }
}
