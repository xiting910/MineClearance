using System;
using System.IO;
using System.Text.Json;

namespace MineClearance.Infrastructure;

/// <summary>
/// 常量类
/// </summary>
public static class Constants
{
    /// <summary>
    /// 要使用的 Json 序列化选项
    /// </summary>
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// 程序数据根目录
    /// </summary>
    public static readonly string AppDataRootDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        nameof(MineClearance)
    );

    /// <summary>
    /// 数据目录文件夹
    /// </summary>
    public const string DataDirectory = "Data";

    /// <summary>
    /// 日志文件夹
    /// </summary>
    public const string LogDirectory = "Logs";

    /// <summary>
    /// 设置文件夹
    /// </summary>
    public const string SettingsDirectory = "Settings";

    /// <summary>
    /// 游戏存档文件路径
    /// </summary>
    public static readonly string GameSaveDataFilePath = Path.Combine(
        AppDataRootDirectory, DataDirectory, "SavedGame.json"
    );

    /// <summary>
    /// 游戏结果记录文件路径
    /// </summary>
    public static readonly string GameResultsFilePath = Path.Combine(
        AppDataRootDirectory, DataDirectory, "History.json"
    );

    /// <summary>
    /// 最新日志文件路径
    /// </summary>
    public static readonly string LatestLogFilePath = Path.Combine(
        AppDataRootDirectory, LogDirectory, $"Latest{LogFileSuffix}"
    );

    /// <summary>
    /// 日志设置文件路径
    /// </summary>
    public static readonly string LogSettingsFilePath = Path.Combine(
        AppDataRootDirectory, SettingsDirectory, $"LogSettings{SettingFileSuffix}"
    );

    /// <summary>
    /// 设置文件后缀
    /// </summary>
    public const string SettingFileSuffix = ".json";

    /// <summary>
    /// 日志文件后缀
    /// </summary>
    public const string LogFileSuffix = ".log";

    /// <summary>
    /// 最大日志文件数量
    /// </summary>
    public const int MaxLogFiles = 5;
}
