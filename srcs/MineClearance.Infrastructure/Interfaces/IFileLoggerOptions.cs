using Microsoft.Extensions.Logging;

namespace MineClearance.Infrastructure.Interfaces;

/// <summary>
/// 文件日志记录器选项接口
/// </summary>
public interface IFileLoggerOptions
{
    /// <summary>
    /// 日志级别
    /// </summary>
    LogLevel Level { get; set; }
}
