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
    /// 记录执行引导更新失败的日志
    /// </summary>
    /// <param name="exception">引发异常</param>
    [LoggerMessage(
        EventName = "PerformingBootstrapUpdateFailed",
        Level = LogLevel.Error,
        Message = "Performing bootstrap update failed"
    )]
    private partial void LogPerformingBootstrapUpdateFailed(Exception exception);
}
