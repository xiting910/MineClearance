using Avalonia;
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
    /// 未知异常的类型
    /// </summary>
    /// <param name="message">异常消息</param>
    private sealed class UnknownException(string? message) : Exception(message);

    /// <summary>
    /// 应用程序入口点
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new UnknownException(e.ExceptionObject.ToString());
            UnhandledExceptionHelper.HandleException(e.IsTerminating, ex);
        };

        if (BootstrapUpdateHelper.IsBootstrapUpdateRequested(args, out var dir, out var version))
        {
            return BootstrapUpdateHelper.ExecuteBootstrapUpdate(dir, version);
        }

        var pipeName = $"{AppMetadata.Get(AppMetadata.ProductKey)}_{AppMetadata.Get(AppMetadata.AuthorKey)}";
        if (!SingleInstanceServer.TryCreate(pipeName, out var server))
        {
            SingleInstanceServer.SendActivateRequest(pipeName);
            return 0;
        }

        using var service = new ServiceCollection()
            .AddSingleton(server)
            .AddSingleton<IConfiguration>(Initialize())
            .AddLogging(builder => builder.AddFileLogger())
            .AddCore()
            .AddInfrastructure()
            .AddSingleton<UIOptions>()
            .AddSingleton<ShellViewModel>()
            .AddSingleton<ToastViewModel>()
            .AddSingleton<UpdateViewModel>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<GameViewModel>()
            .AddSingleton<HistoryViewModel>()
            .AddTransient<SettingsViewModel>()
            .BuildServiceProvider();

        App.Services = service;
        var exitCode = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);

        server.Dispose();
        return exitCode;
    }

    /// <summary>
    /// 初始化应用程序: 创建必要的目录, 完成日志文件的轮转并创建应用程序配置对象
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
            var logsDir = root.CreateSubdirectory(Infrastructure.Constants.LogDirectoryName);

            // 创建设置目录
            var settingsDir = root.CreateSubdirectory(Infrastructure.Constants.SettingsDirectoryName);

            // 创建数据目录
            _ = root.CreateSubdirectory(Infrastructure.Constants.DataDirectoryName);

            // 遍历所有的设置文件, 将所有的设置文件加载到配置构建器中
            foreach (var file in settingsDir.EnumerateFiles($"*{Infrastructure.Constants.JsonFileSuffix}"))
            {
                _ = configBuilder.AddJsonFile(file.FullName);
            }

            // 获取最新日志文件的 FileInfo 对象
            var latestLogFileInfo = new FileInfo(Infrastructure.Constants.LatestLogFilePath);

            // 提前获取最新日志文件名, 因为在接下来执行 MoveTo 方法后, latestLogFileInfo.Name 将不再是最新日志文件名
            var latestLogFileName = latestLogFileInfo.Name;

            // 判断最新日志是否存在并且不为空
            if (latestLogFileInfo.Exists && latestLogFileInfo.Length > 0)
            {
                // 轮转日志文件: 将最新日志文件移动到以当前时间命名的文件中
                latestLogFileInfo.MoveTo(Path.Combine(
                    latestLogFileInfo.DirectoryName!,
                    $"{DateTime.Now:yyyy-MM-dd_HHmmss}{Infrastructure.Constants.LogFileSuffix}"
                ));
            }

            // 获取所有旧的日志文件, 按时间降序排序, 并跳过最新的 N 个文件
            var oldFiles = logsDir
                .EnumerateFiles($"*{Infrastructure.Constants.LogFileSuffix}", SearchOption.TopDirectoryOnly)
                .Where(path => !Infrastructure.Constants.PathComparer.Equals(path.Name, latestLogFileName))
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
