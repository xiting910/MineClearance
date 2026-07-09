using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MineClearance.Core.Models.Records;

/// <summary>
/// 游戏存档数据类, 用于保存游戏的状态, 以便在游戏中断后恢复游戏
/// </summary>
/// <param name="Seed">随机种子</param>
/// <param name="Difficulty">游戏难度</param>
/// <param name="StartTime">游戏开始时间</param>
/// <param name="Duration">游戏已进行的时间</param>
/// <param name="MineField">
/// 地雷分布, 使用 <see cref="BitArray"/> 表示, 其中 <see langword="true"/> 表示该位置有地雷,
/// <see langword="false"/> 表示该位置没有地雷
/// </param>
/// <param name="CellStates">
/// 单元格状态, 使用 <see cref="IReadOnlyDictionary{Position, CellType}"/> 表示,
/// 只记录不为 <see cref="Enums.CellType.Unopened"/> 的单元格状态, 其中键为单元格位置, 值为单元格状态
/// </param>
/// <param name="BoardHeight">游戏板高度</param>
/// <param name="BoardWidth">游戏板宽度</param>
/// <param name="MineCount">地雷数量</param>
/// <remarks>
/// 主构造函数并没有对参数进行验证, 为了避免无效的对象影响业务逻辑,
/// 在创建 <see cref="GameSaveData"/> 实例后应调用 <see cref="IsValid"/> 方法进行验证
/// </remarks>
public sealed record GameSaveData(
    int Seed,
    Enums.GameDifficulty Difficulty,
    DateTime StartTime,
    TimeSpan Duration,
    BitArray MineField,
    IReadOnlyDictionary<Position, Enums.CellType> CellStates,
    int? BoardHeight,
    int? BoardWidth,
    int? MineCount)
{
    /// <summary>
    /// 创建非自定义难度的游戏存档数据
    /// </summary>
    /// <param name="seed">随机种子</param>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="startTime">游戏开始时间</param>
    /// <param name="duration">游戏已进行的时间</param>
    /// <param name="mineField">地雷分布</param>
    /// <param name="cellStates">单元格状态</param>
    /// <returns>游戏存档数据</returns>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时抛出
    /// </exception>
    public static GameSaveData Create(int seed, Enums.GameDifficulty difficulty, DateTime startTime, TimeSpan duration, BitArray mineField, IReadOnlyDictionary<Position, Enums.CellType> cellStates)
    {
        return difficulty is Enums.GameDifficulty.Custom
            ? throw new ArgumentException(Constants.CustomDifficultyMissingInfoMessage, nameof(difficulty))
            : new(seed, difficulty, startTime, duration, mineField, cellStates, null, null, null);
    }

    /// <summary>
    /// 创建自定义难度的游戏存档数据
    /// </summary>
    /// <param name="seed">随机种子</param>
    /// <param name="startTime">游戏开始时间</param>
    /// <param name="duration">游戏已进行的时间</param>
    /// <param name="mineField">地雷分布</param>
    /// <param name="cellStates">单元格状态</param>
    /// <param name="boardHeight">游戏板高度</param>
    /// <param name="boardWidth">游戏板宽度</param>
    /// <param name="mineCount">地雷数量</param>
    /// <returns>自定义难度游戏存档数据</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="boardHeight"/> 或 <paramref name="boardWidth"/> 为负数或零,
    /// 或 <paramref name="mineCount"/> 为负数或零,
    /// 或 <paramref name="mineCount"/> 大于等于
    /// <paramref name="boardHeight"/> * <paramref name="boardWidth"/> 时抛出
    /// </exception>
    public static GameSaveData CreateCustom(int seed, DateTime startTime, TimeSpan duration, BitArray mineField, IReadOnlyDictionary<Position, Enums.CellType> cellStates, int boardHeight, int boardWidth, int mineCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardHeight, nameof(boardHeight));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardWidth, nameof(boardWidth));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mineCount, nameof(mineCount));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mineCount, boardHeight * boardWidth, nameof(mineCount));

        return new(seed, Enums.GameDifficulty.Custom, startTime, duration, mineField, cellStates, boardHeight, boardWidth, mineCount);
    }

    /// <summary>
    /// 验证当前 <see cref="GameSaveData"/> 实例是否有效
    /// </summary>
    /// <returns><see langword="true"/> 如果当前实例有效, 否则 <see langword="false"/></returns>
    public bool IsValid()
    {
        // 获取棋盘高度和宽度
        int boardHeight, boardWidth;

        // 判断游戏难度是否为自定义难度
        if (Difficulty is Enums.GameDifficulty.Custom)
        {
            // 自定义难度时, 棋盘高度、宽度和地雷数量必须不为 null且大于零
            if (BoardHeight is null or <= 0 || BoardWidth is null or <= 0 || MineCount is null or <= 0)
            {
                return false;
            }

            // 获取棋盘高度和宽度
            boardHeight = BoardHeight.Value;
            boardWidth = BoardWidth.Value;

            // 地雷数量必须小于棋盘总格子数
            if (MineCount.Value >= boardHeight * boardWidth)
            {
                return false;
            }
        }
        else
        {
            // 非自定义难度时, 棋盘高度、宽度和地雷数量必须为 null
            if (BoardHeight is not null || BoardWidth is not null || MineCount is not null)
            {
                return false;
            }

            // 获取棋盘高度和宽度
            (boardHeight, boardWidth, _) = GameConfig.FromDifficulty(Difficulty);
        }

        // 验证地雷分布的长度是否与棋盘大小匹配
        if (MineField.Length != boardHeight * boardWidth)
        {
            return false;
        }

        // 验证单元格状态的键是否都在棋盘范围内
        if (!CellStates.Keys.All(pos => pos.IsInBounds(boardHeight, boardWidth)))
        {
            return false;
        }

        // 全部验证通过, 当前实例有效
        return true;
    }

    // 该类不必重写 object.ToString() 方法, 因为存档数据不需要以字符串形式输出
}
