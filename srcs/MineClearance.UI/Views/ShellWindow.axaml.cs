using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.UI.ViewModels;
using System;

namespace MineClearance.UI.Views;

/// <summary>
/// 桌面端壳窗口, 承载 <see cref="ShellView"/>
/// </summary>
public sealed partial class ShellWindow : Window
{
    /// <summary>
    /// 是否正在执行保存后的关闭, 用于放行第二次关闭请求
    /// </summary>
    private bool _isSaving;

    /// <summary>
    /// 创建桌面端壳窗口
    /// </summary>
    public ShellWindow()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // 订阅主视图的退出请求, 直接关闭窗口
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.ExitRequested += OnExitRequested;
        }
    }

    /// <summary>
    /// 退出程序: 关闭本窗口, 进行中的游戏由关闭事件自动保存
    /// </summary>
    private void OnExitRequested()
    {
        Close();
    }

    /// <summary>
    /// 关闭前自动保存进行中的游戏: 取消本次关闭, 保存完成后真正关闭, 避免进程退出截断文件写入
    /// </summary>
    /// <param name="sender">窗口</param>
    /// <param name="e">关闭事件参数</param>
    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // 保存完成后再次关闭, 放行
        if (_isSaving) { return; }

        // 获取游戏管理器
        var manager = App.Services.GetRequiredService<IGameManager>();

        // 当前没有游戏进行中, 放行关闭
        if (manager.Game is not { Status: GameStatus.InProgress or GameStatus.Paused }) { return; }

        // 取消本次关闭
        e.Cancel = true;

        // 保存当前游戏
        _ = await manager.SaveAndExitAsync();

        // 设置标记, 避免再次触发保存
        _isSaving = true;

        // 真正关闭窗口
        Close();
    }
}
