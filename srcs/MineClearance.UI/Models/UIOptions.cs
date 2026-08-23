using Avalonia.Input;
using Avalonia.Media;
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
    /// 是否使用自定义背景图片
    /// </summary>
    public bool UseCustomBackgroundImage
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
    /// 要使用的背景图片文件名
    /// </summary>
    public string BackgroundImageFileName
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
    /// 背景图片拉伸方式
    /// </summary>
    public Stretch BackgroundImageStretch
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
    /// 背景图片透明度
    /// </summary>
    public double BackgroundImageOpacity
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
        var sec = configuration.GetSection(nameof(UIOptions));

        ShowFirstLaunchTip = !bool.TryParse(sec[nameof(ShowFirstLaunchTip)], out var showTip) || showTip;
        Theme = Enum.TryParse(sec[nameof(Theme)], out ThemeMode theme) ? theme : ThemeMode.System;
        UseCustomBackgroundImage = bool.TryParse(sec[nameof(UseCustomBackgroundImage)], out var u) && u;
        var bgFileName = sec[nameof(BackgroundImageFileName)];
        BackgroundImageFileName = bgFileName is not null ? bgFileName : UseCustomBackgroundImage
            ? string.Empty : Constants.DefaultBackgroundImageFileName;
        BackgroundImageStretch = Enum.TryParse(sec[nameof(BackgroundImageStretch)], out Stretch stretch)
            ? stretch : Stretch.UniformToFill;
        BackgroundImageOpacity = double.TryParse(sec[nameof(BackgroundImageOpacity)], out var opacity)
            ? Math.Clamp(opacity, 0, Constants.MaxRatio) : Constants.MaxRatio;
        ToastDurationSeconds = double.TryParse(sec[nameof(ToastDurationSeconds)], out var seconds)
            ? Math.Clamp(seconds, Constants.MinToastDurationSeconds, Constants.MaxToastDurationSeconds)
            : Constants.DefaultToastDurationSeconds;
        MaxToastCount = int.TryParse(sec[nameof(MaxToastCount)], out var count)
            ? Math.Clamp(count, Constants.MinMaxToastCount, Constants.MaxMaxToastCount)
            : Constants.DefaultMaxToastCount;
        ShowDownloadBall = !bool.TryParse(sec[nameof(ShowDownloadBall)], out var show) || show;
        CopyIndexOnFirstClick = bool.TryParse(sec[nameof(CopyIndexOnFirstClick)], out var copy) && copy;
        ShowIndexHotKey = Enum.TryParse(sec[nameof(ShowIndexHotKey)], out Key hotKey)
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
                    [nameof(ShowFirstLaunchTip)] = ShowFirstLaunchTip,
                    [nameof(Theme)] = Theme.ToString(),
                    [nameof(UseCustomBackgroundImage)] = UseCustomBackgroundImage,
                    [nameof(BackgroundImageFileName)] = BackgroundImageFileName,
                    [nameof(BackgroundImageStretch)] = BackgroundImageStretch.ToString(),
                    [nameof(BackgroundImageOpacity)] = BackgroundImageOpacity,
                    [nameof(ToastDurationSeconds)] = ToastDurationSeconds,
                    [nameof(MaxToastCount)] = MaxToastCount,
                    [nameof(ShowDownloadBall)] = ShowDownloadBall,
                    [nameof(CopyIndexOnFirstClick)] = CopyIndexOnFirstClick,
                    [nameof(ShowIndexHotKey)] = ShowIndexHotKey.ToString()
                }
            }.ToJsonString(Infrastructure.Constants.JsonSerializerOptions));
        }
        catch { /* 忽略写入文件时的异常 */ }
    }
}
