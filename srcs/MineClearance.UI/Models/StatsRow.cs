using MineClearance.Core.Enums;
using System;

namespace MineClearance.UI.Models;

/// <summary>
/// 统计行, 展示一组难度范围下的汇总统计, 数值字段用于排序
/// </summary>
/// <param name="Difficulty">难度范围, 全部难度范围时为 <see langword="null"/></param>
/// <param name="DifficultyText">难度范围文本</param>
/// <param name="Games">游戏次数</param>
/// <param name="Wins">胜利次数</param>
/// <param name="WinRateText">胜率文本, 无数据时为 --</param>
/// <param name="WinRate">胜率数值 (百分比), 无数据时为 -1</param>
/// <param name="AvgWinDurationText">平均胜利用时文本, 无胜局时为 --</param>
/// <param name="AvgWinDuration">平均胜利用时, 无胜局时为 <see langword="null"/></param>
/// <param name="MinWinDurationText">最短胜利用时文本, 无胜局时为 --</param>
/// <param name="MinWinDuration">最短胜利用时, 无胜局时为 <see langword="null"/></param>
/// <param name="AvgCompletionText">平均完成度文本, 无失败局时为 --</param>
/// <param name="AvgCompletion">平均完成度数值 (0-1), 无失败局时为 -1</param>
public sealed record StatsRow(
    GameDifficulty? Difficulty,
    string DifficultyText,
    int Games,
    int Wins,
    string WinRateText,
    double WinRate,
    string AvgWinDurationText,
    TimeSpan? AvgWinDuration,
    string MinWinDurationText,
    TimeSpan? MinWinDuration,
    string AvgCompletionText,
    double AvgCompletion
);
