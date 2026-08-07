using Avalonia.Controls;

namespace MineClearance.UI.Views;

/// <summary>
/// 桌面端壳窗口, 承载 <see cref="ShellView"/>
/// </summary>
public sealed partial class ShellWindow : Window
{
    /// <summary>
    /// 创建桌面端壳窗口
    /// </summary>
    public ShellWindow()
    {
        InitializeComponent();
    }
}
