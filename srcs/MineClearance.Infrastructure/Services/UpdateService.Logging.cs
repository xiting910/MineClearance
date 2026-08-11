using Microsoft.Extensions.Logging;
using System;

namespace MineClearance.Infrastructure.Services;

// 更新服务实现类的日志记录部分
internal partial class UpdateService
{
    /// <summary>
    /// 记录检查更新的日志
    /// </summary>
    /// <param name="author">作者</param>
    /// <param name="repository">仓库</param>
    /// <param name="version">版本号</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "CheckingForUpdates",
        Level = LogLevel.Information,
        Message = "Checking for updates for {Author}/{Repository} from version {Version}."
    )]
    private partial void LogCheckingForUpdates(string author, string repository, string version);

    /// <summary>
    /// 记录已是最新版本的日志
    /// </summary>
    /// <param name="version">当前版本号</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "UpToDate",
        Level = LogLevel.Information,
        Message = "Already up to date on version {Version}."
    )]
    private partial void LogUpToDate(string version);

    /// <summary>
    /// 记录更新包已存在的日志
    /// </summary>
    /// <param name="filePath">更新包文件路径</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "UpdatePackageAlreadyComplete",
        Level = LogLevel.Information,
        Message = "Update package already complete at {FilePath}."
    )]
    private partial void LogUpdatePackageAlreadyComplete(string filePath);

    /// <summary>
    /// 记录发现新版本的日志
    /// </summary>
    /// <param name="latestVersion">最新版本号</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "FoundUpdate",
        Level = LogLevel.Information,
        Message = "Found new version {LatestVersion}."
    )]
    private partial void LogFoundUpdate(string latestVersion);

    /// <summary>
    /// 记录检查更新失败的日志
    /// </summary>
    /// <param name="exception">引发异常</param>
    [LoggerMessage(
        EventId = 5,
        EventName = "CheckingFailed",
        Level = LogLevel.Warning,
        Message = "Checking for updates failed"
    )]
    private partial void LogCheckingFailed(Exception exception);

    /// <summary>
    /// 记录开始下载的日志
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="filePath">保存文件路径</param>
    [LoggerMessage(
        EventId = 6,
        EventName = "DownloadStarted",
        Level = LogLevel.Information,
        Message = "Downloading update from {Url} to {FilePath}."
    )]
    private partial void LogDownloadStarted(string url, string filePath);

    /// <summary>
    /// 记录下载完成的日志
    /// </summary>
    /// <param name="latestVersion">最新版本号</param>
    [LoggerMessage(
        EventId = 7,
        EventName = "DownloadCompleted",
        Level = LogLevel.Information,
        Message = "Download completed for version {LatestVersion}."
    )]
    private partial void LogDownloadCompleted(string latestVersion);

    /// <summary>
    /// 记录下载取消的日志
    /// </summary>
    [LoggerMessage(
        EventId = 8,
        EventName = "DownloadCancelled",
        Level = LogLevel.Information,
        Message = "Download cancelled"
    )]
    private partial void LogDownloadCancelled();

    /// <summary>
    /// 记录下载失败的日志
    /// </summary>
    /// <param name="exception">引发异常</param>
    [LoggerMessage(
        EventId = 9,
        EventName = "DownloadFailed",
        Level = LogLevel.Warning,
        Message = "Downloading update failed"
    )]
    private partial void LogDownloadFailed(Exception exception);

    /// <summary>
    /// 记录从断点续传的日志
    /// </summary>
    /// <param name="filePath">断点文件路径</param>
    [LoggerMessage(
        EventId = 10,
        EventName = "ResumingDownload",
        Level = LogLevel.Information,
        Message = "Resuming download from breakpoint file {FilePath}."
    )]
    private partial void LogResumingDownload(string filePath);

    /// <summary>
    /// 记录执行引导更新失败的日志
    /// </summary>
    /// <param name="exception">引发异常</param>
    [LoggerMessage(
        EventId = 11,
        EventName = "PerformingBootstrapUpdateFailed",
        Level = LogLevel.Error,
        Message = "Performing bootstrap update failed"
    )]
    private partial void LogPerformingBootstrapUpdateFailed(Exception exception);
}
