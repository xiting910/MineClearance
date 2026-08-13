using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MineClearance.Infrastructure.Services;

/// <summary>
/// 文件日志记录器提供程序实现类
/// </summary>
/// <param name="_options">文件日志记录器选项</param>
internal sealed class FileLoggerProvider(FileLoggerOptions _options) : ILoggerProvider
{
    /// <summary>
    /// 文件日志记录器锁对象
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// 文件日志记录器的写入器
    /// </summary>
    private readonly StreamWriter _writer = new(Constants.LatestLogFilePath, true, Encoding.UTF8)
    {
        AutoFlush = true
    };

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_options, _writer, categoryName, _lock);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _writer.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 文件日志记录器, Provider 的私有嵌套类
    /// </summary>
    /// <param name="_options">文件日志记录器选项</param>
    /// <param name="_writer">文件日志记录器的写入器</param>
    /// <param name="_categoryName">日志类别名称</param>
    /// <param name="_lock">文件日志记录器锁对象</param>
    private sealed class FileLogger(
        FileLoggerOptions _options,
        StreamWriter _writer,
        string _categoryName,
        Lock _lock
    ) : ILogger
    {
        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            // 不支持日志作用域
            return null;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            // 日志级别为 None 时永不启用, 否则级别高于或等于配置级别时启用
            return logLevel != LogLevel.None && logLevel >= _options.Level;
        }

        /// <inheritdoc/>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // 如果当前级别未启用, 则不记录日志
            if (!IsEnabled(logLevel)) { return; }

            // 拼接日志行: 时间戳, 级别缩写, 类别名称, 事件ID, 事件名, 日志内容, 异常信息
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] "
                + $"[{logLevel}] [{_categoryName}] ({eventId.Id} {eventId.Name}) "
                + $"{formatter(state, exception)}";

            try
            {
                lock (_lock)
                {
                    // 写入日志行
                    _writer.WriteLine(line);

                    // 如果有异常, 则写入异常信息
                    if (exception is not null)
                    {
                        _writer.WriteLine(exception);
                    }
                }
            }
            catch { /* 忽略写日志时发生的异常 */ }
        }
    }
}
