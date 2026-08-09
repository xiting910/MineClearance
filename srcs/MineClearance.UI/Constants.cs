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
    /// 最大比例 (0-1 区间的最大值)
    /// </summary>
    public const double MaxRatio = 1.0;

    /// <summary>
    /// 百分比基数, 用于百分比和比例的转换
    /// </summary>
    public const double PercentBase = 100.0;

    /// <summary>
    /// 界面状态轮询刷新间隔 (毫秒), 用于游戏计时显示
    /// </summary>
    public const int UiRefreshIntervalMilliseconds = 250;

    /// <summary>
    /// Toast 提示轮询刷新间隔 (毫秒), 用于提示进度条显示
    /// </summary>
    public const int ToastRefreshIntervalMilliseconds = 50;

    /// <summary>
    /// 抽屉与遮布动画时长 (毫秒)
    /// </summary>
    public const int DrawerAnimationDurationMilliseconds = 200;

    /// <summary>
    /// 窗口钳制到工作区时的右边额外留边距
    /// </summary>
    public const int WindowClampRightMargin = 20;

    /// <summary>
    /// 窗口钳制到工作区时的底部额外留边距
    /// </summary>
    public const int WindowClampBottomMargin = 60;

    /// <summary>
    /// Toast 提示入场偏移 (像素), 新提示从下方滑入时使用的初始位移
    /// </summary>
    public const double ToastEnterOffset = 16;

    /// <summary>
    /// 抽屉默认宽度, 用户可拖动右边界调整, 不保存
    /// </summary>
    public const double DrawerWidth = 450;

    /// <summary>
    /// 棋盘格子大小 (像素), 固定正方形
    /// </summary>
    public const double CellSize = 25;

    /// <summary>
    /// 下载悬浮球直径 (像素)
    /// </summary>
    public const double DownloadBallSize = 48;

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
    /// 默认的 Toast 同时显示最大条数
    /// </summary>
    public const int DefaultMaxToastCount = 2;

    /// <summary>
    /// Toast 同时显示的最少条数
    /// </summary>
    public const int MinMaxToastCount = 1;

    /// <summary>
    /// Toast 同时显示的最多条数
    /// </summary>
    public const int MaxMaxToastCount = 5;

    /// <summary>
    /// UI 配置设置文件路径
    /// </summary>
    public static readonly string UIOptionsSettingsFilePath = Path.Combine(
        Infrastructure.Constants.AppDataRootDirectory,
        Infrastructure.Constants.SettingsDirectoryName,
        $"UISettings{Infrastructure.Constants.JsonFileSuffix}"
    );
}
