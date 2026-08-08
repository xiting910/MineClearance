using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core.Interfaces;
using MineClearance.UI.ViewModels;
using System;
using System.ComponentModel;

namespace MineClearance.UI.Views;

/// <summary>
/// 桌面端壳窗口, 承载 <see cref="ShellView"/>
/// </summary>
public sealed partial class ShellWindow : Window
{
    /// <summary>
    /// 是否正在执行位置调整, 用于防止调整位置触发的位置变化事件递归
    /// </summary>
    private bool _isAdjustingPosition;

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
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.ExitRequested += OnExitRequested;
            viewModel.PropertyChanged += OnShellPropertyChanged;
            viewModel.Game.PropertyChanged += OnGamePropertyChanged;

            ApplyMinSize(Constants.MainViewMinWidth, Constants.MainViewMinHeight);
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
    /// 游戏视图模型属性变化时更新最小窗口尺寸 (游戏视图中切换难度时)
    /// </summary>
    /// <param name="sender">游戏视图模型</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnGamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GameViewModel.BoardPixelWidth) or nameof(GameViewModel.BoardPixelHeight))
        {
            UpdateMinSizeIfInGameView();
        }
    }

    /// <summary>
    /// 窗口尺寸变化时把位置钳制回工作区内, 防止最小尺寸增大导致窗口向右下扩张超出屏幕
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">尺寸变化事件参数</param>
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        AdjustPositionToWorkingArea();
    }

    /// <summary>
    /// 窗口位置变化时把位置钳制回工作区内, 使窗口在移动过程中始终完整可见 (参照旧项目 WM_MOVING 处理)
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">位置变化事件参数</param>
    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        // 调整位置触发的递归变化直接忽略
        if (_isAdjustingPosition) { return; }

        AdjustPositionToWorkingArea();
    }

    /// <summary>
    /// 关闭前自动保存进行中的游戏: 取消本次关闭, 保存完成后真正关闭, 避免进程退出截断文件写入
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">关闭事件参数</param>
    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // 保存完成后再次关闭, 放行
        if (_isSaving) { return; }

        // 获取游戏管理器
        var manager = App.Services.GetRequiredService<IGameManager>();

        // 当前没有有实际进度的游戏, 放行关闭
        if (manager.Game is not { HasProgress: true }) { return; }

        // 取消本次关闭
        e.Cancel = true;

        // 保存当前游戏
        _ = await manager.SaveAndExitAsync();

        // 设置标记, 避免再次触发保存
        _isSaving = true;

        // 真正关闭窗口
        Close();
    }

    /// <summary>
    /// 应用固定的最小窗口宽高
    /// </summary>
    /// <param name="minWidth">最小宽度</param>
    /// <param name="minHeight">最小高度</param>
    private void ApplyMinSize(double minWidth, double minHeight)
    {
        MinWidth = minWidth;
        MinHeight = minHeight;
    }

    /// <summary>
    /// 游戏视图可见时按当前棋盘尺寸更新最小窗口宽高
    /// </summary>
    private void UpdateMinSizeIfInGameView()
    {
        if (DataContext is ShellViewModel { IsGameViewVisible: true } viewModel)
        {
            MinWidth = viewModel.Game.BoardPixelWidth + Constants.GameViewMinWidthExtra;
            MinHeight = viewModel.Game.BoardPixelHeight + Constants.GameViewMinHeightExtra;
        }
    }

    /// <summary>
    /// 将窗口位置钳制到当前屏幕工作区内, 保证窗口完整可见 (窗口大于工作区时尽量贴边)
    /// </summary>
    private void AdjustPositionToWorkingArea()
    {
        if (WindowState is WindowState.Maximized) { return; }

        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) { return; }

        var workArea = screen.WorkingArea;

        var frameSize = PixelSize.FromSize(
            new(
                ClientSize.Width + WindowDecorationMargin.Left + WindowDecorationMargin.Right,
                ClientSize.Height + WindowDecorationMargin.Top + WindowDecorationMargin.Bottom
            ),
            screen.Scaling
        );

        var x = Math.Clamp(
            Position.X,
            workArea.X,
            workArea.X + workArea.Width - frameSize.Width - Constants.WindowClampRightMargin
        );
        var y = Math.Clamp(
            Position.Y,
            workArea.Y,
            workArea.Y + workArea.Height - frameSize.Height - Constants.WindowClampBottomMargin
        );

        if (x == Position.X && y == Position.Y) { return; }

        _isAdjustingPosition = true;
        try
        {
            Position = new(x, y);
        }
        finally
        {
            _isAdjustingPosition = false;
        }
    }

    /// <summary>
    /// 退出程序: 关闭本窗口, 进行中的游戏由关闭事件自动保存
    /// </summary>
    private void OnExitRequested()
    {
        Close();
    }
}
