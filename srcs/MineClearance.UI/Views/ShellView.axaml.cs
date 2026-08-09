using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.ViewModels;
using System;

namespace MineClearance.UI.Views;

/// <summary>
/// 壳视图, 承载当前视图/设置抽屉/下载悬浮球/下载详情抽屉与全局 Toast 通知
/// </summary>
public sealed partial class ShellView : UserControl
{
    /// <summary>
    /// 是否正在拖动设置抽屉右边界调整宽度
    /// </summary>
    private bool _isResizingSettings;

    /// <summary>
    /// 开始拖动时的指针横坐标, 用于计算拖动增量
    /// </summary>
    private double _resizeStartX;

    /// <summary>
    /// 开始拖动时的设置抽屉宽度
    /// </summary>
    private double _resizeStartWidth;

    /// <summary>
    /// 创建壳视图
    /// </summary>
    public ShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击共用遮布时收起当前抽屉: 下载抽屉打开时先关闭它, 否则关闭设置抽屉
    /// </summary>
    /// <param name="sender">遮布</param>
    /// <param name="e">指针事件参数</param>
    private void OnMaskPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
        {
            if (viewModel.Update.IsDrawerVisible)
            {
                viewModel.Update.CloseDrawer();
            }
            else
            {
                viewModel.CloseSettings();
            }
        }
    }

    /// <summary>
    /// 按下设置抽屉右边界拖动手柄时记录起点并捕获指针
    /// </summary>
    /// <param name="sender">拖动手柄</param>
    /// <param name="e">指针事件参数</param>
    private void OnSettingsResizePressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ShellViewModel viewModel) { return; }

        _isResizingSettings = true;
        _resizeStartX = e.GetPosition(this).X;
        _resizeStartWidth = viewModel.SettingsDrawerWidth;
        e.Pointer.Capture((IInputElement)sender!);
        e.Handled = true;
    }

    /// <summary>
    /// 拖动过程中按指针移动增量更新设置抽屉宽度
    /// </summary>
    /// <param name="sender">拖动手柄</param>
    /// <param name="e">指针事件参数</param>
    private void OnSettingsResizeMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizingSettings || DataContext is not ShellViewModel viewModel) { return; }

        // 拖动宽度不超过当前窗口大小
        var maxWidth = TopLevel.GetTopLevel(this)?.ClientSize.Width ?? _resizeStartWidth;
        var deltaX = e.GetPosition(this).X - _resizeStartX;
        viewModel.SettingsDrawerWidth = Math.Clamp(_resizeStartWidth + deltaX, 0, maxWidth);
    }

    /// <summary>
    /// 释放设置抽屉拖动手柄时结束拖动并释放指针捕获
    /// </summary>
    /// <param name="sender">拖动手柄</param>
    /// <param name="e">指针事件参数</param>
    private void OnSettingsResizeReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isResizingSettings) { return; }

        _isResizingSettings = false;
        e.Pointer.Capture(null);
    }
}
