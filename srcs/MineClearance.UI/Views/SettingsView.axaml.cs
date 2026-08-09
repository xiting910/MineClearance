using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 设置抽屉内容视图, 提供主题/Toast 时长/日志级别配置与关于信息
/// </summary>
public sealed partial class SettingsView : UserControl
{
    /// <summary>
    /// 创建设置视图
    /// </summary>
    public SettingsView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击关闭按钮时请求收起设置抽屉
    /// </summary>
    /// <param name="sender">按钮</param>
    /// <param name="e">路由事件参数</param>
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.RequestClose();
        }
    }

    /// <summary>
    /// 点击 GitHub 链接时打开仓库地址
    /// </summary>
    /// <param name="sender">链接文本</param>
    /// <param name="e">指针事件参数</param>
    private void OnGitHubLinkPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.OpenGitHub();
        }
    }
}
