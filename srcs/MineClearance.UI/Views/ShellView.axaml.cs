using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.UI.ViewModels;
using System;

namespace MineClearance.UI.Views;

/// <summary>
/// 壳视图, 承载当前视图与全局 Toast 通知, 并负责打开设置窗口
/// </summary>
public sealed partial class ShellView : UserControl
{
    /// <summary>
    /// 当前绑定的视图模型, 用于事件订阅管理
    /// </summary>
    private ShellViewModel? _viewModel;

    /// <summary>
    /// 设置窗口实例, 窗口单例
    /// </summary>
    private SettingsWindow? _settingsWindow;

    /// <summary>
    /// 创建壳视图
    /// </summary>
    public ShellView()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // 退订旧视图模型的事件
        _viewModel?.SettingsWindowRequested -= OnSettingsWindowRequested;

        // 订阅新视图模型的事件
        _viewModel = DataContext as ShellViewModel;
        _viewModel?.SettingsWindowRequested += OnSettingsWindowRequested;
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // 视图离开视觉树时退订事件, 防止内存泄漏
        _viewModel?.SettingsWindowRequested -= OnSettingsWindowRequested;
        _viewModel = null;
    }

    /// <summary>
    /// 打开设置窗口, 窗口单例, 已打开时激活现有窗口
    /// </summary>
    private void OnSettingsWindowRequested()
    {
        if (_settingsWindow is not null && _settingsWindow.IsVisible)
        {
            // 已打开则激活现有窗口
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new()
        {
            DataContext = App.Services.GetRequiredService<SettingsViewModel>()
        };
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }
}
