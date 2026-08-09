using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.ViewModels;
using System;

namespace MineClearance.UI.Views;

/// <summary>
/// 下载详情抽屉视图, 显示下载进度/速度/异常信息, 允许取消下载与拖动右边界调整宽度
/// </summary>
public sealed partial class DownloadDrawerView : UserControl
{
    /// <summary>
    /// 是否正在拖动右边界调整宽度
    /// </summary>
    private bool _isResizing;

    /// <summary>
    /// 开始拖动时的指针横坐标, 用于计算拖动增量
    /// </summary>
    private double _resizeStartX;

    /// <summary>
    /// 开始拖动时的抽屉宽度
    /// </summary>
    private double _resizeStartWidth;

    /// <summary>
    /// 创建下载详情抽屉视图
    /// </summary>
    public DownloadDrawerView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 按下右边界拖动手柄时记录起点并捕获指针
    /// </summary>
    /// <param name="sender">拖动手柄</param>
    /// <param name="e">指针事件参数</param>
    private void OnResizeHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not UpdateViewModel viewModel) { return; }

        _isResizing = true;
        _resizeStartX = e.GetPosition(this).X;
        _resizeStartWidth = viewModel.DrawerWidth;
        e.Pointer.Capture((IInputElement)sender!);
        e.Handled = true;
    }

    /// <summary>
    /// 拖动过程中按指针移动增量更新抽屉宽度
    /// </summary>
    /// <param name="sender">拖动手柄</param>
    /// <param name="e">指针事件参数</param>
    private void OnResizeHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || DataContext is not UpdateViewModel viewModel) { return; }

        // 拖动宽度不超过当前窗口大小
        var maxWidth = TopLevel.GetTopLevel(this)?.ClientSize.Width ?? _resizeStartWidth;
        var deltaX = e.GetPosition(this).X - _resizeStartX;
        viewModel.DrawerWidth = Math.Clamp(_resizeStartWidth + deltaX, 0, maxWidth);
    }

    /// <summary>
    /// 释放拖动手柄时结束拖动并释放指针捕获
    /// </summary>
    /// <param name="sender">拖动手柄</param>
    /// <param name="e">指针事件参数</param>
    private void OnResizeHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isResizing) { return; }

        _isResizing = false;
        e.Pointer.Capture(null);
    }
}
