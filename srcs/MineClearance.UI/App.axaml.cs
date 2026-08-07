using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace MineClearance.UI;

/// <summary>
/// 应用程序类
/// </summary>
public sealed partial class App : Application
{
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

        // 如果应用程序生命周期是经典桌面样式, 则设置主窗口
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }
        else
        {
            throw new NotSupportedException("Not supported application lifetime.");
        }
    }
}
