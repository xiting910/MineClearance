using MineClearance.Infrastructure.Models;
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
    /// 要使用的路径比较器
    /// </summary>
    public static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    /// <summary>
    /// 程序数据根目录
    /// </summary>
    public static readonly string AppDataRootDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        nameof(MineClearance)
    );

    /// <summary>
    /// 数据目录文件夹名
    /// </summary>
    public const string DataDirectoryName = "Data";

    /// <summary>
    /// 日志文件夹名
    /// </summary>
    public const string LogDirectoryName = "Logs";

    /// <summary>
    /// 设置文件夹名
    /// </summary>
    public const string SettingsDirectoryName = "Settings";

    /// <summary>
    /// 更新数据目录
    /// </summary>
    public static readonly string UpdateDataDirectory = Path.Combine(AppDataRootDirectory, "Update");

    /// <summary>
    /// 游戏存档文件路径
    /// </summary>
    public static readonly string GameSaveDataFilePath = Path.Combine(
        AppDataRootDirectory, DataDirectoryName, $"SavedGame{JsonFileSuffix}"
    );

    /// <summary>
    /// 游戏结果记录文件路径
    /// </summary>
    public static readonly string GameResultsFilePath = Path.Combine(
        AppDataRootDirectory, DataDirectoryName, $"History{JsonFileSuffix}"
    );

    /// <summary>
    /// 最新日志文件路径
    /// </summary>
    public static readonly string LatestLogFilePath = Path.Combine(
        AppDataRootDirectory, LogDirectoryName, $"Latest{LogFileSuffix}"
    );

    /// <summary>
    /// 日志设置文件路径
    /// </summary>
    public static readonly string LogSettingsFilePath = Path.Combine(
        AppDataRootDirectory, SettingsDirectoryName, $"LogSettings{JsonFileSuffix}"
    );

    /// <summary>
    /// 备份目录
    /// </summary>
    public static readonly string BackupDirectory = Path.Combine(UpdateDataDirectory, "Backup");

    /// <summary>
    /// 引导副本所在的目录
    /// </summary>
    public static readonly string BootstrapCopyDirectory = Path.Combine(UpdateDataDirectory, "BootstrapCopy");

    /// <summary>
    /// 下载的更新包的文件路径
    /// </summary>
    public static readonly string UpdatePackageFilePath = Path.Combine(
        UpdateDataDirectory, $"Update{ZipFileSuffix}"
    );

    /// <summary>
    /// 新版本的版本号文件路径
    /// </summary>
    public static readonly string NewVersionFilePath = Path.Combine(UpdateDataDirectory, "NewVersion.txt");

    /// <summary>
    /// 更新信息文件路径
    /// </summary>
    public static readonly string UpdateInfoFilePath = Path.Combine(
        UpdateDataDirectory, $"{nameof(UpdateInfo)}{JsonFileSuffix}"
    );

    /// <summary>
    /// 更新日志文件路径
    /// </summary>
    public static readonly string UpdateLogFilePath = Path.Combine(
        UpdateDataDirectory, $"Update{LogFileSuffix}"
    );

    /// <summary>
    /// 下载临时文件后缀
    /// </summary>
    public const string DownloadTempFileSuffix = ".download";

    /// <summary>
    /// 日志文件后缀
    /// </summary>
    public const string LogFileSuffix = ".log";

    /// <summary>
    /// json 文件后缀
    /// </summary>
    public const string JsonFileSuffix = ".json";

    /// <summary>
    /// zip 文件后缀
    /// </summary>
    public const string ZipFileSuffix = ".zip";

    /// <summary>
    /// 使用引导更新模式的参数
    /// </summary>
    public const string UseBootstrapUpdateModeArgument = "--use-bootstrap-update-mode";

    /// <summary>
    /// 最大日志文件数量
    /// </summary>
    public const int MaxLogFiles = 5;

    /// <summary>
    /// 百分比基数, 用于百分比和比例的转换
    /// </summary>
    public const double PercentBase = 100.0;
}
