using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MineClearance.UI.Models;
using System;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 壳视图模型, 负责主视图/游戏视图/历史记录视图之间的切换
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
    /// 当前可见的视图
    /// </summary>
    [ObservableProperty]
    public partial object CurrentView { get; set; }

    /// <summary>
    /// 请求打开设置窗口的事件, 由视图层创建窗口
    /// </summary>
    public event Action? SettingsWindowRequested;

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
    /// 切换到历史记录视图
    /// </summary>
    private void ShowHistoryView()
    {
        CurrentView = History;
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
            case NavigationTarget.SettingsWindow: SettingsWindowRequested?.Invoke(); break;
        }
    }
}
