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
    /// <see cref="Enums.GameDifficulty.Beginner"/> 对应的 <see cref="Models.Records.GameConfig"/> 实例
    /// </summary>
    public static Models.Records.GameConfig BeginnerConfig { get; } = new(9, 9, 10, null);

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Intermediate"/> 对应的 <see cref="Models.Records.GameConfig"/> 实例
    /// </summary>
    public static Models.Records.GameConfig IntermediateConfig { get; } = new(16, 16, 40, null);

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Expert"/> 对应的 <see cref="Models.Records.GameConfig"/> 实例
    /// </summary>
    public static Models.Records.GameConfig ExpertConfig { get; } = new(16, 30, 99, null);

    /// <summary>
    /// <see cref="Enums.GameDifficulty.Master"/> 对应的 <see cref="Models.Records.GameConfig"/> 实例
    /// </summary>
    public static Models.Records.GameConfig MasterConfig { get; } = new(30, 50, 309, null);
}
