using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.Infrastructure;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        // 获取 Toast 提示视图模型, 以便在未处理异常时显示提示
        var toastViewModel = Services.GetRequiredService<ToastViewModel>();

        // 处理未处理的 UI 线程异常, 显示 Toast 提示并写入日志文件
        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            var ex = e.Exception;
            UnhandledExceptionHelper.HandleException(false, ex);
            toastViewModel.Show(
                $"发生未处理的 UI 线程异常: {ex.Message}, 阅读 " +
                Infrastructure.Constants.UnhandledExceptionLogFilePath +
                " 以查看详细信息"
            );
            e.Handled = true;
        };

        // 处理未处理的任务异常, 显示 Toast 提示并写入日志文件
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            UnhandledExceptionHelper.HandleException(false, e.Exception);
            toastViewModel.Show(
                $"发生未处理的任务异常: {e.Exception.Message}, 阅读 " +
                Infrastructure.Constants.UnhandledExceptionLogFilePath +
                " 以查看详细信息"
            );
            e.SetObserved();
        };

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
            _ = Services.GetRequiredService<SingleInstanceServer>().WaitForActivationRequestsAsync(() =>
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (desktop.MainWindow is { } window)
                {
                    if (window.WindowState is WindowState.Minimized)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    window.Show();
                    window.Activate();
                }
            }), ExitCts.Token);
        }
        else
        {
            throw new NotSupportedException("Not supported application lifetime.");
        }
    }
}
