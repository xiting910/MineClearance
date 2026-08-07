using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core;
using System;

namespace MineClearance.UI;

/// <summary>
/// 程序入口类
/// </summary>
file static class Program
{
    /// <summary>
    /// 应用程序入口点
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        App.Services = new ServiceCollection()
            .AddCore()
            .BuildServiceProvider();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
