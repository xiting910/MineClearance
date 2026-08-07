using System.IO;

namespace MineClearance.UI;

/// <summary>
/// 常量类
/// </summary>
public static class Constants
{
    /// <summary>
    /// 视图模型后缀
    /// </summary>
    public const string ViewModelSuffix = "ViewModel";

    /// <summary>
    /// 视图后缀
    /// </summary>
    public const string ViewSuffix = "View";

    /// <summary>
    /// 默认的 Toast 提示显示时间 (秒)
    /// </summary>
    public const double DefaultToastDurationSeconds = 3;

    /// <summary>
    /// Toast 提示的最短显示时间 (秒)
    /// </summary>
    public const double MinToastDurationSeconds = 0;

    /// <summary>
    /// Toast 提示的最长显示时间 (秒)
    /// </summary>
    public const double MaxToastDurationSeconds = 10;

    /// <summary>
    /// UI 配置设置文件路径
    /// </summary>
    public static readonly string UIOptionsSettingsFilePath = Path.Combine(
        Infrastructure.Constants.AppDataRootDirectory,
        Infrastructure.Constants.SettingsDirectory,
        $"UISettings{Infrastructure.Constants.SettingFileSuffix}"
    );
}
