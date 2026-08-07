using Avalonia.Controls;

namespace MineClearance.UI.Views;

/// <summary>
/// 设置窗口, 提供主题/Toast 时长/日志级别配置与关于信息 (阶段 4 实现)
/// </summary>
public sealed partial class SettingsWindow : Window
{
    /// <summary>
    /// 创建设置窗口
    /// </summary>
    public SettingsWindow()
    {
        InitializeComponent();
    }
}
