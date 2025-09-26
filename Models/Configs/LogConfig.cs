using MineClearance.Models.Enums;
using System.Text.Json.Serialization;

namespace MineClearance.Models.Configs;

/// <summary>
/// 日志配置类
/// </summary>
internal sealed class LogConfig
{
    /// <summary>
    /// 当前最低记录日志级别的整数值的默认值
    /// </summary>
    private const int DefaultMinLogLevelValue = (int)LogLevel.Warning;

    /// <summary>
    /// 程序退出后等待日志写入的最长时间的默认值
    /// </summary>
    private const int DefaultMaxLogWaitTime = 5000;

    /// <summary>
    /// 日志文件最多保留时间的默认值(单位: 天)
    /// </summary>
    private const int DefaultMaxLogFileRetentionDays = 7;

    /// <summary>
    /// 当前最低记录日志级别的整数值字段
    /// </summary>
    [JsonInclude]
    private volatile int _minLogLevelValue = DefaultMinLogLevelValue;

    /// <summary>
    /// 程序退出后等待日志写入的最长时间的字段
    /// </summary>
    [JsonInclude]
    private volatile int _maxLogWaitTime = DefaultMaxLogWaitTime;

    /// <summary>
    /// 日志文件最多保留时间的字段
    /// </summary>
    [JsonInclude]
    private volatile int _maxLogFileRetentionDays = DefaultMaxLogFileRetentionDays;

    /// <summary>
    /// 获取或设置当前最低记录日志级别
    /// </summary>
    [JsonIgnore]
    public LogLevel MinLogLevel
    {
        get => (LogLevel)_minLogLevelValue;
        set
        {
            var newValue = (int)value;
            if (_minLogLevelValue != newValue)
            {
                _minLogLevelValue = newValue;
                Changed?.Invoke();
            }
        }
    }

    /// <summary>
    /// 程序退出后等待日志写入的最长时间(单位: 毫秒)
    /// </summary>
    [JsonIgnore]
    public int MaxLogWaitTime
    {
        get => _maxLogWaitTime;
        set
        {
            if (value != _maxLogWaitTime)
            {
                _maxLogWaitTime = value;
                Changed?.Invoke();
            }
        }
    }

    /// <summary>
    /// 日志文件最多保留时间(单位: 天)
    /// </summary>
    [JsonIgnore]
    public int MaxLogFileRetentionDays
    {
        get => _maxLogFileRetentionDays;
        set
        {
            if (value != _maxLogFileRetentionDays)
            {
                _maxLogFileRetentionDays = value;
                Changed?.Invoke();
            }
        }
    }

    /// <summary>
    /// 当前日志配置发生变化事件
    /// </summary>
    public event Action? Changed;
}