using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 右下角短暂提示视图, 绑定视图模型的 Toast 反馈属性
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
    /// 鼠标进入提示区域时暂停倒计时并切换为手型光标
    /// </summary>
    /// <param name="sender">提示视图</param>
    /// <param name="e">指针事件参数</param>
    private void OnToastPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is ToastViewModel viewModel)
        {
            viewModel.Pause();
            Cursor = new(StandardCursorType.Hand);
        }
    }

    /// <summary>
    /// 鼠标离开提示区域时恢复倒计时并还原光标
    /// </summary>
    /// <param name="sender">提示视图</param>
    /// <param name="e">指针事件参数</param>
    private void OnToastPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is ToastViewModel viewModel)
        {
            viewModel.Resume();
            Cursor = Cursor.Default;
        }
    }

    /// <summary>
    /// 点击提示时关闭提示并执行点击回调
    /// </summary>
    /// <param name="sender">提示视图</param>
    /// <param name="e">点击事件参数</param>
    private void OnToastTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is ToastViewModel viewModel)
        {
            viewModel.InvokeClick();
        }
    }
}
