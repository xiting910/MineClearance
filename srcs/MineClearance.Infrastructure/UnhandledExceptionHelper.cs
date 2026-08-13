using System;
using System.IO;
using System.Threading;

namespace MineClearance.Infrastructure;

/// <summary>
/// 未处理异常帮助类
/// </summary>
public static class UnhandledExceptionHelper
{
    /// <summary>
    /// 锁对象
    /// </summary>
    private static readonly Lock _lock = new();

    /// <summary>
    /// 处理未处理异常
    /// </summary>
    /// <param name="isTerminating">是否即将终止</param>
    /// <param name="ex">未处理异常</param>
    public static void HandleException(bool isTerminating, Exception ex)
    {
        var text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
            (isTerminating ? "[Terminating] " : "[Non-Terminating] ") +
            ex.ToString() + Environment.NewLine;

        try
        {
            lock (_lock)
            {
                // 写入未处理异常日志文件
                File.AppendAllText(Constants.UnhandledExceptionLogFilePath, text);
            }
        }
        catch { /* 忽略写日志时发生的异常 */ }
    }
}
