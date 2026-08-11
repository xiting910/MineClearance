using MineClearance.Core.Models.Records;
using System;

namespace MineClearance.UI.Models;

/// <summary>
/// 统计行构建器, 用于累积游戏结果并生成统计行
/// </summary>
public struct StatsRowBuilder()
{
    /// <summary>
    /// 空统计行文本, 用于无数据时显示
    /// </summary>
    private const string EmptyStatsText = "--";

    /// <summary>
    /// 累计游戏局数
    /// </summary>
    private int _games;

    /// <summary>
    /// 累计胜利局数
    /// </summary>
    private int _wins;

    /// <summary>
    /// 累计胜利局用时刻度数
    /// </summary>
    private long _winTicks;

    /// <summary>
    /// 累计最短胜利局用时刻度数
    /// </summary>
    private long _minWinTicks = long.MaxValue;

    /// <summary>
    /// 累计失败局完成度总和, 用于计算平均完成度
    /// </summary>
    private double _lossCompletion;

    /// <summary>
    /// 添加一个游戏结果到累计统计中
    /// </summary>
    /// <param name="result">游戏结果</param>
    public void Add(GameResult result)
    {
        _games++;
        if (result.IsWin)
        {
            _wins++;
            _winTicks += result.Duration.Ticks;
            _minWinTicks = Math.Min(_minWinTicks, result.Duration.Ticks);
        }
        else
        {
            _lossCompletion += result.Completion!.Value;
        }
    }

    /// <summary>
    /// 将累计统计转换为统计行
    /// </summary>
    /// <param name="text">统计行显示的难度文本</param>
    /// <returns>统计行</returns>
    public readonly StatsRow ToRow(string text)
    {
        var losses = _games - _wins;
        return new(
            DifficultyText: text,
            Games: _games,
            Wins: _wins,
            WinRateText: _games == 0 ? EmptyStatsText : $"{_wins * Constants.PercentBase / _games:0.##}%",
            WinRate: _games == 0 ? -1 : _wins * Constants.PercentBase / _games,
            AvgWinDurationText: _wins == 0
                ? EmptyStatsText
                : FormatTimeSpan(TimeSpan.FromTicks(_winTicks / _wins)),
            AvgWinDuration: _wins == 0 ? null : TimeSpan.FromTicks(_winTicks / _wins),
            MinWinDurationText: _wins == 0 ? EmptyStatsText : FormatTimeSpan(TimeSpan.FromTicks(_minWinTicks)),
            MinWinDuration: _wins == 0 ? null : TimeSpan.FromTicks(_minWinTicks),
            AvgCompletionText: losses == 0
                ? EmptyStatsText
                : $"{_lossCompletion / losses * Constants.PercentBase:0.##}%",
            AvgCompletion: losses == 0 ? -1 : _lossCompletion / losses
        );
    }

    /// <summary>
    /// 格式化统计用时为 MM:SS
    /// </summary>
    /// <param name="time">用时</param>
    /// <returns>格式化后的文本</returns>
    private static string FormatTimeSpan(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }
}
