using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json.Nodes;

namespace MineClearance.Infrastructure;

/// <summary>
/// 文件日志记录器选项实现类
/// </summary>
/// <param name="_configuration">应用程序配置对象</param>
#pragma warning disable CA1707
public sealed class FileLoggerOptions(IConfiguration _configuration)
#pragma warning restore CA1707
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public LogLevel Level
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveLogLevelToFile(value);
            }
        }
    } = GetLogLevelFromConfiguration(_configuration);

    /// <summary>
    /// 从应用程序配置对象中获取日志级别
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    /// <returns>日志级别</returns>
    private static LogLevel GetLogLevelFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(FileLoggerOptions));
        return Enum.TryParse(section[nameof(Level)], out LogLevel level) ? level : LogLevel.Information;
    }

    /// <summary>
    /// 将日志级别保存到文件
    /// </summary>
    /// <param name="level">日志级别</param>
    private static void SaveLogLevelToFile(LogLevel level)
    {
        try
        {
            File.WriteAllText(Constants.LogSettingsFilePath, new JsonObject
            {
                [nameof(FileLoggerOptions)] = new JsonObject
                {
                    [nameof(Level)] = level.ToString()
                }
            }.ToJsonString(Constants.JsonSerializerOptions));
        }
        catch { /* 忽略写入文件时的异常 */}
    }
}
