using MineClearance.Core.Enums;
using System;
using System.Diagnostics;

namespace MineClearance.Core.Models.Records;

/// <summary>
/// 游戏配置类, 用于表示一局扫雷游戏的配置
/// </summary>
/// <param name="BoardHeight">棋盘高度</param>
/// <param name="BoardWidth">棋盘宽度</param>
/// <param name="MineCount">地雷数量</param>
public readonly record struct GameConfig(int BoardHeight, int BoardWidth, int MineCount)
{
    /// <summary>
    /// 获取当前配置下要打开的格子总数
    /// </summary>
    /// <returns>要打开的格子总数</returns>
    public int TotalCellsToOpen => (BoardHeight * BoardWidth) - MineCount;

    /// <summary>
    /// 通过 <see cref="GameDifficulty"/> 获取内置的 <see cref="GameConfig"/> 实例
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <returns>对应难度的 <see cref="GameConfig"/> 实例</returns>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="difficulty"/> 为 <see cref="GameDifficulty.Custom"/> 时抛出
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="difficulty"/> 为无效值时抛出
    /// </exception>
    public static GameConfig FromDifficulty(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Beginner => Constants.BeginnerConfig,
            GameDifficulty.Intermediate => Constants.IntermediateConfig,
            GameDifficulty.Expert => Constants.ExpertConfig,
            GameDifficulty.Master => Constants.MasterConfig,
            GameDifficulty.Custom => throw new ArgumentException("Custom difficulty requires custom configuration.", nameof(difficulty)),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
        };
    }

    /// <summary>
    /// 从 <see cref="GameResult"/> 获取 <see cref="GameConfig"/> 实例
    /// </summary>
    /// <param name="result">游戏结果</param>
    /// <returns>对应的 <see cref="GameConfig"/> 实例</returns>
    public static GameConfig FromGameResult(GameResult result)
    {
        Debug.Assert(result.IsValid(), $"{nameof(result)} must be valid to extract.");

        return result.Difficulty is GameDifficulty.Custom
            ? new(result.BoardHeight!.Value, result.BoardWidth!.Value, result.MineCount!.Value)
            : FromDifficulty(result.Difficulty);
    }

    /// <summary>
    /// 从 <see cref="GameSaveData"/> 获取 <see cref="GameConfig"/> 实例
    /// </summary>
    /// <param name="data">游戏存档数据</param>
    /// <returns>对应的 <see cref="GameConfig"/> 实例</returns>
    public static GameConfig FromGameSaveData(GameSaveData data)
    {
        Debug.Assert(data.IsValid(), $"{nameof(data)} must be valid to extract.");

        return data.Difficulty is GameDifficulty.Custom
            ? new(data.BoardHeight!.Value, data.BoardWidth!.Value, data.MineCount!.Value)
            : FromDifficulty(data.Difficulty);
    }

    /// <summary>
    /// 重写 <see cref="object.ToString"/> 方法, 返回游戏配置的字符串表示
    /// </summary>
    /// <returns>游戏配置的字符串表示</returns>
    public override string ToString()
    {
        return $"{BoardHeight}x{BoardWidth}, {MineCount} mines";
    }
}
