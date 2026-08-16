using Microsoft.Extensions.Logging;
using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;

namespace MineClearance.Core.Services;

// Game 类的日志记录功能实现
internal partial class Game
{
    /// <summary>
    /// 记录游戏被创建的日志信息
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="config">游戏配置</param>
    /// <param name="seed">随机种子</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "GameCreated",
        Level = LogLevel.Information,
        Message = "Game created with difficulty {Difficulty}, config: {Config}, seed: {Seed}"
    )]
    private partial void LogGameCreated(GameDifficulty difficulty, GameConfig config, int seed);

    /// <summary>
    /// 记录游戏状态变更的日志信息
    /// </summary>
    /// <param name="newStatus">新的游戏状态</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "GameStatusChanged",
        Level = LogLevel.Information,
        Message = "Game status changed to {NewStatus}"
    )]
    private partial void LogGameStatusChanged(GameStatus newStatus);

    /// <summary>
    /// 记录游戏结果的日志信息
    /// </summary>
    /// <param name="result">游戏结果</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "GameResult",
        Level = LogLevel.Information,
        Message = "Game ended with result: {Result}"
    )]
    private partial void LogGameResult(GameResult result);

    /// <summary>
    /// 记录内部地雷场被更换的日志信息
    /// </summary>
    [LoggerMessage(
        EventId = 4,
        EventName = "MineFieldReplaced",
        Level = LogLevel.Information,
        Message = "Internal mine field has been replaced"
    )]
    private partial void LogMineFieldReplaced();
}
