using MineClearance.Core;
using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using System;

namespace MineClearance.UI.Models;

/// <summary>
/// 游戏结果行, 包装单条游戏结果, 提供显示文本与棋盘尺寸
/// </summary>
/// <param name="result">游戏结果</param>
public sealed class GameResultRow(GameResult result)
{
    /// <summary>
    /// 游戏结果
    /// </summary>
    public GameResult Result { get; } = result;

    /// <summary>
    /// 棋盘配置, 内置难度取预设值, 自定义难度取结果中的实际值
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">当游戏结果的难度值不在枚举范围内时抛出</exception>
    public GameConfig Config { get; } = result.Difficulty switch
    {
        GameDifficulty.Beginner => Core.Constants.BeginnerConfig,
        GameDifficulty.Intermediate => Core.Constants.IntermediateConfig,
        GameDifficulty.Expert => Core.Constants.ExpertConfig,
        GameDifficulty.Master => Core.Constants.MasterConfig,
        GameDifficulty.Custom => new(
            result.BoardHeight!.Value,
            result.BoardWidth!.Value,
            result.MineCount!.Value
        ),
        _ => throw new ArgumentOutOfRangeException(
            nameof(result.Difficulty),
            result.Difficulty,
            $"Unknown difficulty: {result.Difficulty}"
        )
    };

    /// <summary>
    /// 开始时间文本
    /// </summary>
    public string StartTimeText => Result.StartTime.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// 难度文本
    /// </summary>
    public string DifficultyText => Result.Difficulty.GetDescription();

    /// <summary>
    /// 结果文本 (胜利/失败)
    /// </summary>
    public string ResultText => Result.IsWin ? "胜利" : "失败";

    /// <summary>
    /// 完成度排序值, 胜利时为 1.0, 失败时为实际完成度
    /// </summary>
    public double CompletionForSort => Result.IsWin
        ? Core.Constants.MaxCompletion
        : (Result.Completion ?? Core.Constants.MaxCompletion);

    /// <summary>
    /// 完成度文本, 胜利显示 100%, 失败显示实际完成度
    /// </summary>
    public string CompletionText => (CompletionForSort * Core.Constants.PercentBase)
        .ToString(Core.Constants.FloatFormat) +
        Core.Constants.PercentSign;

    /// <summary>
    /// 用时文本 (MM:SS.xx)
    /// </summary>
    public string DurationText => $"{(int)Result.Duration.TotalMinutes:00}:{Result.Duration.Seconds:00}.{Result.Duration.Milliseconds / 10:00}";

    /// <summary>
    /// 棋盘高度文本
    /// </summary>
    public string HeightText => Config.BoardHeight.ToString();

    /// <summary>
    /// 棋盘宽度文本
    /// </summary>
    public string WidthText => Config.BoardWidth.ToString();

    /// <summary>
    /// 地雷数量文本
    /// </summary>
    public string MineCountText => Config.MineCount.ToString();
}
