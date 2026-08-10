using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json.Nodes;

namespace MineClearance.UI.Models;

/// <summary>
/// UI 配置实现类, 属性变化时自动保存到文件
/// </summary>
/// <param name="_configuration">应用程序配置对象</param>
#pragma warning disable CA1707
public sealed class UIOptions(IConfiguration _configuration)
#pragma warning restore CA1707
{
    /// <summary>
    /// 主题模式
    /// </summary>
    public ThemeMode Theme
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    } = GetThemeFromConfiguration(_configuration);

    /// <summary>
    /// Toast 提示显示时间 (秒)
    /// </summary>
    public double ToastDurationSeconds
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    } = GetToastDurationFromConfiguration(_configuration);

    /// <summary>
    /// Toast 同时显示的最大条数
    /// </summary>
    public int MaxToastCount
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    } = GetMaxToastCountFromConfiguration(_configuration);

    /// <summary>
    /// 是否显示下载悬浮球
    /// </summary>
    public bool ShowDownloadBall
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    } = GetShowDownloadBallFromConfiguration(_configuration);

    /// <summary>
    /// 是否显示首次启动提示, 仅在首次启动时显示, 显示后自动设置为 false
    /// </summary>
    public bool ShowFirstLaunchTip
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    } = GetShowFirstLaunchTipFromConfiguration(_configuration);

    /// <summary>
    /// 从应用程序配置对象中获取主题模式
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    /// <returns>主题模式</returns>
    private static ThemeMode GetThemeFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));
        return Enum.TryParse(section[nameof(Theme)], out ThemeMode theme) ? theme : ThemeMode.System;
    }

    /// <summary>
    /// 从应用程序配置对象中获取 Toast 提示显示时间
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    /// <returns>Toast 提示显示时间</returns>
    private static double GetToastDurationFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));
        return double.TryParse(section[nameof(ToastDurationSeconds)], out var seconds)
            ? Math.Clamp(seconds, Constants.MinToastDurationSeconds, Constants.MaxToastDurationSeconds)
            : Constants.DefaultToastDurationSeconds;
    }

    /// <summary>
    /// 从应用程序配置对象中获取 Toast 同时显示的最大条数
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    /// <returns>Toast 同时显示的最大条数</returns>
    private static int GetMaxToastCountFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));
        return int.TryParse(section[nameof(MaxToastCount)], out var count)
            ? Math.Clamp(count, Constants.MinMaxToastCount, Constants.MaxMaxToastCount)
            : Constants.DefaultMaxToastCount;
    }

    /// <summary>
    /// 从应用程序配置对象中获取是否显示下载悬浮球
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    /// <returns>是否显示下载悬浮球</returns>
    private static bool GetShowDownloadBallFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));
        return !bool.TryParse(section[nameof(ShowDownloadBall)], out var show) || show;
    }

    /// <summary>
    /// 从应用程序配置对象中获取是否显示首次启动提示
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    /// <returns>是否显示首次启动提示</returns>
    private static bool GetShowFirstLaunchTipFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));
        return !bool.TryParse(section[nameof(ShowFirstLaunchTip)], out var show) || show;
    }

    /// <summary>
    /// 将当前配置保存到文件
    /// </summary>
    private void SaveToFile()
    {
        try
        {
            File.WriteAllText(Constants.UIOptionsSettingsFilePath, new JsonObject
            {
                [nameof(UIOptions)] = new JsonObject
                {
                    [nameof(Theme)] = Theme.ToString(),
                    [nameof(ToastDurationSeconds)] = ToastDurationSeconds,
                    [nameof(MaxToastCount)] = MaxToastCount,
                    [nameof(ShowDownloadBall)] = ShowDownloadBall,
                    [nameof(ShowFirstLaunchTip)] = ShowFirstLaunchTip
                }
            }.ToJsonString(Infrastructure.Constants.JsonSerializerOptions));
        }
        catch { /* 忽略写入文件时的异常 */ }
    }
}
