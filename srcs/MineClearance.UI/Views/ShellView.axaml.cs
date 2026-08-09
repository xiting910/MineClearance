using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 壳视图, 承载当前视图/设置抽屉与全局 Toast 通知
/// </summary>
public sealed partial class ShellView : UserControl
{
    /// <summary>
    /// 创建壳视图
    /// </summary>
    public ShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击设置抽屉遮罩时收起抽屉
    /// </summary>
    /// <param name="sender">遮罩</param>
    /// <param name="e">指针事件参数</param>
    private void OnSettingsMaskPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.CloseSettings();
        }
    }
}
