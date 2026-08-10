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

        // 录制模式下使用 Tunnel 方向注册事件, 在内部控件 (按钮/复选框) 之前拦截按键与点击
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
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

    /// <summary>
    /// 录制模式下处理按键: Esc 取消, Back/Delete 清除, 无效键提示, 有效键设置
    /// </summary>
    /// <param name="sender">设置视图</param>
    /// <param name="e">键盘事件参数</param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SettingsViewModel { IsListeningHotkey: true } viewModel) { return; }

        e.Handled = true;

        switch (e.Key)
        {
            case Key.Escape: viewModel.CancelHotkeyListening(); break;
            case Key.Back or Key.Delete: viewModel.ClearHotkey(); break;
            default:
                if (e.Key.IsValidHotKey())
                {
                    viewModel.CompleteHotkeyCapture(e.Key);
                }
                else
                {
                    viewModel.NotifyDisallowedHotKey(e.Key);
                }
                break;
        }
    }

    /// <summary>
    /// 录制模式下鼠标点击任意位置退出录制, 并拦截该次点击不落到其他控件
    /// </summary>
    /// <param name="sender">设置视图</param>
    /// <param name="e">指针按下事件参数</param>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not SettingsViewModel { IsListeningHotkey: true } viewModel) { return; }

        viewModel.CancelHotkeyListening();
        e.Handled = true;
    }
}
