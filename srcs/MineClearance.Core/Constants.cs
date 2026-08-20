using MineClearance.Core.Models.Records;

namespace MineClearance.Core;

/// <summary>
/// 常量类, 用于存放游戏相关的常量值
/// </summary>
public static class Constants
{
    /// <summary>
    /// 允许的最大完成度
    /// </summary>
    public const double MaxCompletion = 1.0;

    /// <summary>
    /// 百分比基数, 用于百分比和比例的转换
    /// </summary>
    public const double PercentBase = 100.0;

    /// <summary>
    /// 百分号符号
    /// </summary>
    public const string PercentSign = "%";

    /// <summary>
    /// 浮点数格式化字符串, 保留两位小数
    /// </summary>
    public const string FloatFormat = "0.##";

    /// <summary>
    /// 周围地雷数量数组中表示地雷的特殊值
    /// </summary>
    public const int MineValue = -1;

    /// <summary>
    /// 允许的最大棋盘高度
    /// </summary>
    public const int MaxBoardHeight = 30;

    /// <summary>
    /// 允许的最大棋盘宽度
    /// </summary>
    public const int MaxBoardWidth = 50;

    /// <summary>
    /// 自定义难度未提供高度、宽度和地雷数量时的异常信息
    /// </summary>
    public const string CustomDifficultyMissingInfoMessage =
        "Custom difficulty requires board dimensions and mine count.";

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Beginner"/> 对应的 <see cref="GameConfig"/> 实例
    /// </summary>
    public static GameConfig BeginnerConfig { get; } = new(9, 9, 10);

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Intermediate"/> 对应的 <see cref="GameConfig"/> 实例
    /// </summary>
    public static GameConfig IntermediateConfig { get; } = new(16, 16, 40);

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Expert"/> 对应的 <see cref="GameConfig"/> 实例
    /// </summary>
    public static GameConfig ExpertConfig { get; } = new(16, 30, 99);

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Master"/> 对应的 <see cref="GameConfig"/> 实例
    /// </summary>
    public static GameConfig MasterConfig { get; } = new(30, 50, 309);
}
