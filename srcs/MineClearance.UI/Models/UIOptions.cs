using Avalonia.Input;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json.Nodes;

namespace MineClearance.UI.Models;

/// <summary>
/// UI 配置实现类, 属性变化时自动保存到文件
/// </summary>
public sealed class UIOptions
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
    }

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
    }

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
    }

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
    }

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
    }

    /// <summary>
    /// 在首次点击格子打开时是否复制格子索引
    /// </summary>
    public bool CopyIndexOnFirstClick
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
    }

    /// <summary>
    /// 在等待游戏开始时显示格子索引的热键
    /// </summary>
    public Key ShowIndexHotKey
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
    }

    /// <summary>
    /// 构造函数, 从应用程序配置对象中获取 UI 配置
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    public UIOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));

        Theme = Enum.TryParse(section[nameof(Theme)], out ThemeMode theme) ? theme : ThemeMode.System;
        ToastDurationSeconds = double.TryParse(section[nameof(ToastDurationSeconds)], out var seconds)
            ? Math.Clamp(seconds, Constants.MinToastDurationSeconds, Constants.MaxToastDurationSeconds)
            : Constants.DefaultToastDurationSeconds;
        MaxToastCount = int.TryParse(section[nameof(MaxToastCount)], out var count)
            ? Math.Clamp(count, Constants.MinMaxToastCount, Constants.MaxMaxToastCount)
            : Constants.DefaultMaxToastCount;
        ShowDownloadBall = !bool.TryParse(section[nameof(ShowDownloadBall)], out var show) || show;
        ShowFirstLaunchTip = !bool.TryParse(section[nameof(ShowFirstLaunchTip)], out var showTip) || showTip;
        CopyIndexOnFirstClick = bool.TryParse(section[nameof(CopyIndexOnFirstClick)], out var copy) && copy;
        ShowIndexHotKey = Enum.TryParse(section[nameof(ShowIndexHotKey)], out Key hotKey)
            && hotKey.IsValidHotKey() ? hotKey : Key.None;
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
                    [nameof(ShowFirstLaunchTip)] = ShowFirstLaunchTip,
                    [nameof(CopyIndexOnFirstClick)] = CopyIndexOnFirstClick,
                    [nameof(ShowIndexHotKey)] = ShowIndexHotKey.ToString()
                }
            }.ToJsonString(Infrastructure.Constants.JsonSerializerOptions));
        }
        catch { /* 忽略写入文件时的异常 */ }
    }
}
