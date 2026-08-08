namespace MineClearance.UI.Models;

/// <summary>
/// 统计表格排序键常量, 与 <see cref="StatsRow"/> 的属性名对应, 供列头排序标识使用
/// </summary>
public static class SortKeys
{
    /// <summary>
    /// 统计: 难度, 对应 <see cref="StatsRow.DifficultyText"/>
    /// </summary>
    public const string DifficultyText = nameof(StatsRow.DifficultyText);

    /// <summary>
    /// 统计: 游戏次数, 对应 <see cref="StatsRow.Games"/>
    /// </summary>
    public const string Games = nameof(StatsRow.Games);

    /// <summary>
    /// 统计: 胜利次数, 对应 <see cref="StatsRow.Wins"/>
    /// </summary>
    public const string Wins = nameof(StatsRow.Wins);

    /// <summary>
    /// 统计: 胜率, 对应 <see cref="StatsRow.WinRate"/>
    /// </summary>
    public const string WinRate = nameof(StatsRow.WinRate);

    /// <summary>
    /// 统计: 平均胜利用时, 对应 <see cref="StatsRow.AvgWinDuration"/>
    /// </summary>
    public const string AvgWinDuration = nameof(StatsRow.AvgWinDuration);

    /// <summary>
    /// 统计: 最短胜利用时, 对应 <see cref="StatsRow.MinWinDuration"/>
    /// </summary>
    public const string MinWinDuration = nameof(StatsRow.MinWinDuration);

    /// <summary>
    /// 统计: 平均完成度, 对应 <see cref="StatsRow.AvgCompletion"/>
    /// </summary>
    public const string AvgCompletion = nameof(StatsRow.AvgCompletion);
}
