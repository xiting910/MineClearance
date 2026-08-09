using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using System;
using System.Threading;

namespace MineClearance.UI;

/// <summary>
/// 应用程序类
/// </summary>
public sealed partial class App : Application
{
    /// <summary>
    /// 程序退出的取消令牌源
    /// </summary>
    public static CancellationTokenSource ExitCts { get; } = new();

    /// <summary>
    /// 服务容器, 由平台入口在启动时注入
    /// </summary>
    /// <exception cref="InvalidOperationException">服务容器未初始化</exception>
    public static IServiceProvider Services
    {
        get => field ?? throw new InvalidOperationException($"{nameof(Services)} is not initialized.");
        set;
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // 按配置的主题模式应用主题
        Current?.RequestedThemeVariant = Services.GetRequiredService<UIOptions>().Theme switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        // 如果应用程序生命周期是经典桌面样式, 则设置主窗口并注册退出事件处理程序
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new ShellWindow
            {
                DataContext = Services.GetRequiredService<ShellViewModel>()
            };
            desktop.Exit += (_, _) => ExitCts.Cancel();
        }
        else
        {
            throw new NotSupportedException("Not supported application lifetime.");
        }
    }
}
