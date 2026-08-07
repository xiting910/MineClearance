using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 设置视图, 提供主题/Toast 时长/日志级别配置与关于信息
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
