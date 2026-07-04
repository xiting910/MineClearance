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
    /// 通过 <see cref="Enums.GameDifficulty"/> 获取内置的 <see cref="GameConfig"/> 实例
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <returns>对应难度的 <see cref="GameConfig"/> 实例</returns>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时抛出
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="difficulty"/> 为无效值时抛出
    /// </exception>
    public static GameConfig FromDifficulty(Enums.GameDifficulty difficulty)
    {
        return difficulty switch
        {
            Enums.GameDifficulty.Beginner => Constants.BeginnerConfig,
            Enums.GameDifficulty.Intermediate => Constants.IntermediateConfig,
            Enums.GameDifficulty.Expert => Constants.ExpertConfig,
            Enums.GameDifficulty.Master => Constants.MasterConfig,
            Enums.GameDifficulty.Custom => throw new ArgumentException("Custom difficulty requires custom configuration.", nameof(difficulty)),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), "Invalid game difficulty.")
        };
    }

    /// <summary>
    /// 从 <see cref="GameResult"/> 获取 <see cref="GameConfig"/> 实例
    /// </summary>
    /// <param name="result">游戏结果</param>
    /// <returns>对应的 <see cref="GameConfig"/> 实例</returns>
    public static GameConfig FromGameResult(GameResult result)
    {
        Debug.Assert(result.IsValid(), "GameResult must be valid to extract GameConfig.");

        return result.Difficulty is Enums.GameDifficulty.Custom
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
        Debug.Assert(data.IsValid(), "GameSaveData must be valid to extract GameConfig.");

        return data.Difficulty is Enums.GameDifficulty.Custom
            ? new(data.BoardHeight!.Value, data.BoardWidth!.Value, data.MineCount!.Value)
            : FromDifficulty(data.Difficulty);
    }

    /// <summary>
    /// 获取当前配置下要打开的格子总数
    /// </summary>
    /// <returns>要打开的格子总数</returns>
    public int GetTotalCellsToOpen()
    {
        return (BoardHeight * BoardWidth) - MineCount;
    }

    /// <summary>
    /// 重写 <see cref="object.ToString"/> 方法, 返回游戏配置的字符串表示
    /// </summary>
    /// <returns>游戏配置的字符串表示</returns>
    public override string ToString()
    {
        return $"GameConfig: {BoardHeight}x{BoardWidth}, {MineCount} mines";
    }
}
