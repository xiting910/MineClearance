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
    /// 界面状态轮询刷新间隔 (毫秒), 用于游戏计时显示
    /// </summary>
    public const int UiRefreshIntervalMilliseconds = 250;

    /// <summary>
    /// 棋盘格子大小 (像素), 固定正方形
    /// </summary>
    public const double CellSize = 25;

    /// <summary>
    /// 默认的 Toast 提示显示时间 (秒)
    /// </summary>
    public const double DefaultToastDurationSeconds = 5;

    /// <summary>
    /// Toast 提示的最短显示时间 (秒)
    /// </summary>
    public const double MinToastDurationSeconds = 0;

    /// <summary>
    /// Toast 提示的最长显示时间 (秒)
    /// </summary>
    public const double MaxToastDurationSeconds = 20;

    /// <summary>
    /// UI 配置设置文件路径
    /// </summary>
    public static readonly string UIOptionsSettingsFilePath = Path.Combine(
        Infrastructure.Constants.AppDataRootDirectory,
        Infrastructure.Constants.SettingsDirectory,
        $"UISettings{Infrastructure.Constants.SettingFileSuffix}"
    );
}
