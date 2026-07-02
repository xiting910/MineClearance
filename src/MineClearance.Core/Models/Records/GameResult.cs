using System;

namespace MineClearance.Core.Models.Records;

/// <summary>
/// 游戏结果类, 用于表示一局扫雷游戏的结果
/// </summary>
/// <param name="Seed">游戏使用的随机种子</param>
/// <param name="Difficulty">游戏难度</param>
/// <param name="StartTime">游戏开始时间</param>
/// <param name="Duration">游戏持续时间</param>
/// <param name="IsWin">
/// <see langword="true"/> 表示玩家获胜, <see langword="false"/> 表示玩家失败
/// </param>
/// <param name="Completion">
/// 完成度, 只在 <paramref name="IsWin"/> 为 <see langword="false"/> 时有效, 范围为 0.0 到 1.0
/// </param>
/// <param name="BoardHeight">
/// 棋盘高度, 只在 <paramref name="Difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时有效
/// </param>
/// <param name="BoardWidth">
/// 棋盘宽度, 只在 <paramref name="Difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时有效
/// </param>
/// <param name="MineCount">
/// 地雷数量, 只在 <paramref name="Difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时有效
/// </param>
/// <remarks>
/// 主构造函数并没有对参数进行验证, 为了避免无效的对象影响业务逻辑,
/// 在创建 <see cref="GameResult"/> 实例后应调用 <see cref="IsValid"/> 方法进行验证
/// </remarks>
public sealed record GameResult(
    int Seed,
    Enums.GameDifficulty Difficulty,
    DateTime StartTime,
    TimeSpan Duration,
    bool IsWin,
    double? Completion,
    int? BoardHeight,
    int? BoardWidth,
    int? MineCount)
{
    /// <summary>
    /// 初始化 <see cref="GameResult"/> 的新实例, 用于非自定义难度的获胜游戏结果
    /// </summary>
    /// <param name="seed">游戏使用的随机种子</param>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="startTime">游戏开始时间</param>
    /// <param name="duration">游戏持续时间</param>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时抛出
    /// </exception>
    public GameResult(
        int seed,
        Enums.GameDifficulty difficulty,
        DateTime startTime,
        TimeSpan duration)
        : this(seed, difficulty, startTime, duration, true, null, null, null, null)
    {
        if (difficulty is Enums.GameDifficulty.Custom)
        {
            throw new ArgumentException("Custom difficulty requires board dimensions and mine count.", nameof(difficulty));
        }
    }

    /// <summary>
    /// 初始化 <see cref="GameResult"/> 的新实例, 用于非自定义难度的失败游戏结果
    /// </summary>
    /// <param name="seed">游戏使用的随机种子</param>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="startTime">游戏开始时间</param>
    /// <param name="duration">游戏持续时间</param>
    /// <param name="completion">完成度, 范围为 0.0 到 1.0</param>
    /// <exception cref="ArgumentException">
    /// 当 <paramref name="difficulty"/> 为 <see cref="Enums.GameDifficulty.Custom"/> 时抛出
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="completion"/> 为负数或大于 <see cref="Constants.MaxCompletion"/> 时抛出
    /// </exception>
    public GameResult(
        int seed,
        Enums.GameDifficulty difficulty,
        DateTime startTime,
        TimeSpan duration,
        double completion)
        : this(seed, difficulty, startTime, duration, false, completion, null, null, null)
    {
        if (difficulty is Enums.GameDifficulty.Custom)
        {
            throw new ArgumentException("Custom difficulty requires board dimensions and mine count.", nameof(difficulty));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(completion, nameof(completion));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(completion, Constants.MaxCompletion, nameof(completion));
    }

    /// <summary>
    /// 初始化 <see cref="GameResult"/> 的新实例, 用于自定义难度的获胜游戏结果
    /// </summary>
    /// <param name="seed">游戏使用的随机种子</param>
    /// <param name="startTime">游戏开始时间</param>
    /// <param name="duration">游戏持续时间</param>
    /// <param name="boardHeight">棋盘高度</param>
    /// <param name="boardWidth">棋盘宽度</param>
    /// <param name="mineCount">地雷数量</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="boardHeight"/> 或 <paramref name="boardWidth"/> 为负数或零,
    /// 或 <paramref name="mineCount"/> 为负数或零,
    /// 或 <paramref name="mineCount"/> 大于等于
    /// <paramref name="boardHeight"/> * <paramref name="boardWidth"/> 时抛出
    /// </exception>
    public GameResult(
        int seed,
        DateTime startTime,
        TimeSpan duration,
        int boardHeight,
        int boardWidth,
        int mineCount)
        : this(seed, Enums.GameDifficulty.Custom, startTime, duration, true, null, boardHeight, boardWidth, mineCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardHeight, nameof(boardHeight));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardWidth, nameof(boardWidth));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mineCount, nameof(mineCount));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mineCount, boardHeight * boardWidth, nameof(mineCount));
    }

    /// <summary>
    /// 初始化 <see cref="GameResult"/> 的新实例, 用于自定义难度的失败游戏结果
    /// </summary>
    /// <param name="seed">游戏使用的随机种子</param>
    /// <param name="startTime">游戏开始时间</param>
    /// <param name="duration">游戏持续时间</param>
    /// <param name="completion">完成度, 范围为 0.0 到 1.0</param>
    /// <param name="boardHeight">棋盘高度</param>
    /// <param name="boardWidth">棋盘宽度</param>
    /// <param name="mineCount">地雷数量</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="completion"/> 为负数或大于 <see cref="Constants.MaxCompletion"/> 时抛出,
    /// 或 <paramref name="boardHeight"/> 或 <paramref name="boardWidth"/> 为负数或零,
    /// 或 <paramref name="mineCount"/> 为负数或零,
    /// 或 <paramref name="mineCount"/> 大于等于
    /// <paramref name="boardHeight"/> * <paramref name="boardWidth"/> 时抛出
    /// </exception>
    public GameResult(
        int seed,
        DateTime startTime,
        TimeSpan duration,
        double completion,
        int boardHeight,
        int boardWidth,
        int mineCount)
        : this(seed, Enums.GameDifficulty.Custom, startTime, duration, false, completion, boardHeight, boardWidth, mineCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completion, nameof(completion));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(completion, Constants.MaxCompletion, nameof(completion));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardHeight, nameof(boardHeight));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardWidth, nameof(boardWidth));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mineCount, nameof(mineCount));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mineCount, boardHeight * boardWidth, nameof(mineCount));
    }

    /// <summary>
    /// 验证当前 <see cref="GameResult"/> 实例的有效性
    /// </summary>
    /// <returns><see langword="true"/> 表示当前实例有效, <see langword="false"/> 表示当前实例无效</returns>
    public bool IsValid()
    {
        // 判断难度是否为自定义难度
        if (Difficulty is Enums.GameDifficulty.Custom)
        {
            // 自定义难度下, 棋盘高度、宽度和地雷数量必须都不为 null
            if (BoardHeight is null || BoardWidth is null || MineCount is null)
            {
                return false;
            }

            // 棋盘高度、宽度和地雷数量都必须是正数
            if (BoardHeight <= 0 || BoardWidth <= 0 || MineCount <= 0)
            {
                return false;
            }

            // 地雷数量必须小于棋盘总格子数
            if (MineCount >= BoardHeight * BoardWidth)
            {
                return false;
            }
        }
        else
        {
            // 非自定义难度下, 棋盘高度、宽度和地雷数量必须都为 null
            if (BoardHeight is not null || BoardWidth is not null || MineCount is not null)
            {
                return false;
            }
        }

        // 判断是否获胜
        if (IsWin)
        {
            // 获胜时, 完成度必须为 null
            if (Completion is not null)
            {
                return false;
            }
        }
        else
        {
            // 失败时, 完成度必须不为 null 且在 0.0 到 Constants.MaxCompletion 之间
            if (Completion is null or < 0.0 or > Constants.MaxCompletion)
            {
                return false;
            }
        }

        // 所有验证通过, 返回 true
        return true;
    }

    /// <summary>
    /// 重写 <see cref="object.ToString"/> 方法, 返回当前实例的字符串表示
    /// </summary>
    /// <returns>游戏结果的字符串表示</returns>
    public override string ToString()
    {
        // 结果信息
        var result = IsWin ? "胜利" : $"失败, 完成度: {Completion * 100:0.##}%";

        // 自定义难度信息
        var custom = Difficulty is Enums.GameDifficulty.Custom ? $", 大小: {BoardHeight}x{BoardWidth}, 地雷数: {MineCount}" : "";

        // 拼接所有信息并返回
        return $"[{StartTime}] 难度: {Difficulty}, 种子: {Seed}, 用时: {Duration}, 结果: {result}{custom}";
    }
}
