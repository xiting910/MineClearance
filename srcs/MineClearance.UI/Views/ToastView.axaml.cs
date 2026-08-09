using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 右下角短暂提示视图, 以列表展示多条 Toast 提示
/// </summary>
public sealed partial class ToastView : UserControl
{
    /// <summary>
    /// 创建短暂提示视图
    /// </summary>
    public ToastView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 鼠标进入提示条目时暂停该条目的倒计时
    /// </summary>
    /// <param name="sender">提示条目</param>
    /// <param name="e">指针事件参数</param>
    private void OnToastPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border { DataContext: ToastItem item })
        {
            item.Pause();
        }
    }

    /// <summary>
    /// 鼠标离开提示条目时恢复该条目的倒计时
    /// </summary>
    /// <param name="sender">提示条目</param>
    /// <param name="e">指针事件参数</param>
    private void OnToastPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border { DataContext: ToastItem item })
        {
            item.Resume();
        }
    }

    /// <summary>
    /// 点击提示条目时立即关闭该条目并执行其点击回调
    /// </summary>
    /// <param name="sender">提示条目</param>
    /// <param name="e">点击事件参数</param>
    private void OnToastTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: ToastItem item } &&
            DataContext is ToastViewModel viewModel)
        {
            viewModel.InvokeClick(item);
        }
    }
}
