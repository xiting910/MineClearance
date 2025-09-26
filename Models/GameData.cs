namespace MineClearance.Models;

/// <summary>
/// 带有版本号和最后更新时间的数据类, 用于游戏结果的存储和管理.
/// </summary>
/// <remarks><para>
/// 旧版本只有游戏结果列表, 且自定义难度的<see cref="GameResult.Difficulty"/>为3, 称为版本0.
/// </para><para>
/// 版本1开始引入了<see cref="GameData"/>类, 包含了最后更新时间和版本号, 同时增加地狱难度, 地狱难度的<see cref="GameResult.Difficulty"/>为3, 自定义难度的<see cref="GameResult.Difficulty"/>变为4.
/// </para><para>
/// 版本2开始, 非自定义难度的<see cref="GameResult.BoardWidth"/>、<see cref="GameResult.BoardHeight"/>和<see cref="GameResult.MineCount"/>都不再保存(均为null).
/// </para><para>
/// 版本3开始, 胜利时不再保存<see cref="GameResult.Completion"/>(为null).
/// </para><para>
/// 版本4开始, 游戏结果保存按时间倒序排列.
/// </para></remarks>
internal sealed record GameData
{
    /// <summary>
    /// 数据版本号
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdate { get; init; }

    /// <summary>
    /// 游戏结果列表
    /// </summary>
    public required List<GameResult> GameResults { get; init; }
}