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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
                    [nameof(ToastDurationSeconds)] = ToastDurationSeconds
                }
            }.ToJsonString(Infrastructure.Constants.JsonSerializerOptions));
        }
        catch { /* 忽略写入文件时的异常 */ }
    }
}
