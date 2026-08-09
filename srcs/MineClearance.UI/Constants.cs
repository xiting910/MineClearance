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
    /// 窗口钳制到工作区时的右边额外留边距
    /// </summary>
    public const int WindowClampRightMargin = 20;

    /// <summary>
    /// 窗口钳制到工作区时的底部额外留边距
    /// </summary>
    public const int WindowClampBottomMargin = 60;

    /// <summary>
    /// 棋盘格子大小 (像素), 固定正方形
    /// </summary>
    public const double CellSize = 25;

    /// <summary>
    /// 最小窗口宽度的额外值
    /// </summary>
    public const double GameViewMinWidthExtra = 50;

    /// <summary>
    /// 最小窗口高度的额外值
    /// </summary>
    public const double GameViewMinHeightExtra = 100;

    /// <summary>
    /// 主视图最小窗口宽度
    /// </summary>
    public const double MainViewMinWidth = 700;

    /// <summary>
    /// 主视图最小窗口高度
    /// </summary>
    public const double MainViewMinHeight = 500;

    /// <summary>
    /// 历史记录视图最小窗口宽度
    /// </summary>
    public const double HistoryViewMinWidth = 1000;

    /// <summary>
    /// 历史记录视图最小窗口高度
    /// </summary>
    public const double HistoryViewMinHeight = 600;

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
        Infrastructure.Constants.SettingsDirectoryName,
        $"UISettings{Infrastructure.Constants.JsonFileSuffix}"
    );
}
