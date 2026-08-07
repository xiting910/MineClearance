using Avalonia.Controls;

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
}
