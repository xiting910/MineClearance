using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 下载进度悬浮球视图, 点击时呼出或关闭下载详情抽屉
/// </summary>
public sealed partial class DownloadBallView : UserControl
{
    /// <summary>
    /// 创建下载进度悬浮球视图
    /// </summary>
    public DownloadBallView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击悬浮球时呼出或关闭下载详情抽屉
    /// </summary>
    /// <param name="sender">悬浮球</param>
    /// <param name="e">点击事件参数</param>
    private void OnBallTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is UpdateViewModel viewModel)
        {
            viewModel.ToggleDrawer();
        }
    }
}
