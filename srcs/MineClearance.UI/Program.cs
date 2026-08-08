using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core;
using MineClearance.Infrastructure;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using System;
using System.IO;
using System.Linq;

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
        using var service = new ServiceCollection()
            .AddSingleton<IConfiguration>(Initialize())
            .AddLogging(builder => builder.AddFileLogger())
            .AddCore()
            .AddInfrastructure()
            .AddSingleton<UIOptions>()
            .AddSingleton<ToastViewModel>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<ShellViewModel>()
            .AddSingleton<GameViewModel>()
            .AddSingleton<HistoryViewModel>()
            .AddTransient<SettingsViewModel>()
            .BuildServiceProvider();

        App.Services = service;
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    /// <summary>
    /// 初始化应用程序: 创建必要的目录, 完成日志文件的轮转并创建
    /// </summary>
    /// <returns>应用程序配置对象</returns>
    private static IConfigurationRoot Initialize()
    {
        // 创建配置构建器
        var configBuilder = new ConfigurationBuilder();

        try
        {
            // 创建根目录
            var root = Directory.CreateDirectory(Infrastructure.Constants.AppDataRootDirectory);

            // 创建日志目录
            var logsDir = root.CreateSubdirectory(Infrastructure.Constants.LogDirectory);

            // 创建设置目录
            var settingsDir = root.CreateSubdirectory(Infrastructure.Constants.SettingsDirectory);

            // 创建数据目录
            _ = root.CreateSubdirectory(Infrastructure.Constants.DataDirectory);

            // 遍历所有的设置文件, 将所有的设置文件加载到配置构建器中
            foreach (var file in settingsDir.EnumerateFiles($"*{Infrastructure.Constants.SettingFileSuffix}"))
            {
                _ = configBuilder.AddJsonFile(file.FullName);
            }

            // 获取最新日志文件的 FileInfo 对象
            var latestLogFileInfo = new FileInfo(Infrastructure.Constants.LatestLogFilePath);

            // 判断最新日志是否存在并且不为空
            if (latestLogFileInfo.Exists && latestLogFileInfo.Length > 0)
            {
                // 轮转日志文件: 将最新日志文件移动到以当前时间命名的文件中
                latestLogFileInfo.MoveTo(Path.Combine(
                    latestLogFileInfo.DirectoryName!,
                    $"{DateTime.Now:yyyy-MM-dd_HHmmss}{Infrastructure.Constants.LogFileSuffix}"
                ));
            }

            // 要使用的路径比较器
            var comparer = OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            // 获取所有旧的日志文件, 按时间降序排序, 并跳过最新的 N 个文件
            var oldFiles = logsDir
                .EnumerateFiles($"*{Infrastructure.Constants.LogFileSuffix}", SearchOption.TopDirectoryOnly)
                .Where(path => !comparer.Equals(path.Name, latestLogFileInfo.Name))
                .OrderByDescending(static path => path.Name)
                .Skip(Infrastructure.Constants.MaxLogFiles - 1)
                .ToList();

            // 删除旧的日志文件
            oldFiles.ForEach(file => file.Delete());
        }
        catch { /* 忽略初始化过程中的异常, 以防止应用程序启动失败 */ }

        // 构建配置对象并返回
        return configBuilder.Build();
    }
}
