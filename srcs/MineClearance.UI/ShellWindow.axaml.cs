using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core.Interfaces;
using MineClearance.Infrastructure;
using MineClearance.UI.ViewModels;
using MineClearance.UI.Views;
using System;
using System.ComponentModel;

namespace MineClearance.UI;

/// <summary>
/// 桌面端壳窗口, 承载 <see cref="ShellView"/>
/// </summary>
public sealed partial class ShellWindow : Window
{
    /// <summary>
    /// 是否正在执行保存后的关闭, 用于放行第二次关闭请求
    /// </summary>
    private bool _isSaving;

    /// <summary>
    /// 创建桌面端壳窗口
    /// </summary>
    public ShellWindow()
    {
        InitializeComponent();
        Width = Constants.MainViewMinWidth;
        Height = Constants.MainViewMinHeight;
    }

    /// <inheritdoc/>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // UI 启动完毕后启动更新流程: 消费上次更新信息并后台检查更新
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.StartUpdateRoutine();
        }
    }

    /// <inheritdoc/>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // 订阅壳视图模型事件, 并按当前视图更新最小窗口尺寸
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.ExitRequested += Close;
            viewModel.PropertyChanged += OnShellPropertyChanged;
            viewModel.Game.PropertyChanged += OnGamePropertyChanged;

            ApplyMinSize(Constants.MainViewMinWidth, Constants.MainViewMinHeight);
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // 窗口最小化时暂停游戏
        if (DataContext is ShellViewModel viewModel
            && change.Property == WindowStateProperty
            && change.NewValue is WindowState.Minimized)
        {
            viewModel.Game.PauseIfPerformable();
        }
    }

    /// <summary>
    /// 关闭前自动保存进行中的游戏, 并在必要时执行引导更新
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">关闭事件参数</param>
    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // 仅在第一次关闭时检查游戏进度, 避免保存完成后再次触发关闭事件导致死循环
        if (!_isSaving)
        {
            // 获取游戏管理器
            var manager = App.Services.GetRequiredService<IGameManager>();

            // 检查当前游戏是否有进度
            if (manager.Game is { HasProgress: true })
            {
                // 有进度时取消本次关闭, 等待保存完成后再关闭
                e.Cancel = true;

                // 保存当前游戏
                _ = await manager.SaveAndExitAsync();

                // 标记正在执行保存后的关闭, 放行第二次关闭请求
                _isSaving = true;

                // 真正关闭窗口
                Close();

                // 返回以避免继续执行后续代码
                return;
            }
        }

        // 真正执行关闭时, 执行引导更新
        App.Services.GetRequiredService<IUpdateService>().PerformBootstrapUpdateIfNecessary();
    }

    /// <summary>
    /// 窗口失去焦点时暂停游戏
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">事件参数</param>
    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.Game.PauseIfPerformable();
        }
    }

    /// <summary>
    /// 窗口尺寸变化时同步钳制各抽屉宽度, 防止抽屉超出窗口范围
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">尺寸变化事件参数</param>
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // 壳视图宽度变化时钳制各抽屉宽度, 防止抽屉超出窗口范围
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.ClampDrawerWidths(e.NewSize.Width);
        }
    }

    /// <summary>
    /// 按下键盘时处理 Esc 键和游戏视图热键
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">键盘事件参数</param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // 已处理的事件或非壳视图模型直接忽略
        if (e.Handled || DataContext is not ShellViewModel viewModel) { return; }

        // Esc 键: 转发给壳视图模型处理, 由壳视图模型决定隐藏哪个抽屉或呼出设置抽屉
        if (e.Key is Key.Escape)
        {
            viewModel.HandleEscapeKey();
            e.Handled = true;
            return;
        }

        // 等待开始时按下热键显示所有格子索引, 由游戏视图模型处理
        if (viewModel.IsGameViewVisible
            && viewModel.Game.IsWaitingStarted
            && viewModel.Game.ShowIndexHotKey is not Key.None
            && e.Key == viewModel.Game.ShowIndexHotKey)
        {
            viewModel.Game.IsShowingIndexes = true;
            e.Handled = true;
        }
    }

    /// <summary>
    /// 松开热键时隐藏所有格子索引
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">键盘事件参数</param>
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Handled) { return; }

        if (DataContext is ShellViewModel { IsGameViewVisible: true } shellViewModel
            && shellViewModel.Game.ShowIndexHotKey is not Key.None
            && e.Key == shellViewModel.Game.ShowIndexHotKey)
        {
            shellViewModel.Game.IsShowingIndexes = false;
        }
    }

    /// <summary>
    /// 壳视图模型属性变化时按当前视图更新最小窗口尺寸: 主视图/历史视图用常量, 游戏视图按棋盘动态计算
    /// </summary>
    /// <param name="sender">壳视图模型</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel) { return; }

        switch (e.PropertyName)
        {
            case nameof(ShellViewModel.IsMainViewVisible) when viewModel.IsMainViewVisible:
                ApplyMinSize(Constants.MainViewMinWidth, Constants.MainViewMinHeight);
                break;

            case nameof(ShellViewModel.IsHistoryViewVisible) when viewModel.IsHistoryViewVisible:
                ApplyMinSize(Constants.HistoryViewMinWidth, Constants.HistoryViewMinHeight);
                break;

            case nameof(ShellViewModel.IsGameViewVisible) when viewModel.IsGameViewVisible:
                UpdateMinSizeIfInGameView();
                break;
        }
    }

    /// <summary>
    /// 游戏视图模型属性变化时按当前棋盘尺寸更新最小窗口宽高
    /// </summary>
    /// <param name="sender">游戏视图模型</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnGamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GameViewModel.BoardPixelWidth)
            or nameof(GameViewModel.BoardPixelHeight))
        {
            UpdateMinSizeIfInGameView();
        }
    }

    /// <summary>
    /// 游戏视图可见时按当前棋盘尺寸更新最小窗口宽高
    /// </summary>
    private void UpdateMinSizeIfInGameView()
    {
        if (DataContext is not ShellViewModel { IsGameViewVisible: true } viewModel) { return; }

        var minWidth = viewModel.Game.BoardPixelWidth + Constants.GameViewMinWidthExtra;
        var minHeight = viewModel.Game.BoardPixelHeight + Constants.GameViewMinHeightExtra;

        ApplyMinSize(minWidth, minHeight);
    }

    /// <summary>
    /// 应用固定的最小窗口宽高, 并钳制上限防止最小尺寸超过当前屏幕工作区
    /// </summary>
    /// <param name="minWidth">最小宽度</param>
    /// <param name="minHeight">最小高度</param>
    private void ApplyMinSize(double minWidth, double minHeight)
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
            return;
        }

        MinWidth = Math.Min(minWidth, screen.WorkingArea.Width / screen.Scaling);
        MinHeight = Math.Min(minHeight, screen.WorkingArea.Height / screen.Scaling);
    }
}
