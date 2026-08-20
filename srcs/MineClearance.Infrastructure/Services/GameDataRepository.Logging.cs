using Microsoft.Extensions.Logging;
using System;

namespace MineClearance.Infrastructure.Services;

// GameDataRepository 类的日志记录功能实现
internal partial class GameDataRepository
{
    /// <summary>
    /// 记录加载存档数据时的异常
    /// </summary>
    /// <param name="ex">异常对象</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "LoadSaveDataException",
        Level = LogLevel.Warning,
        Message = "Load save data exception"
    )]
    private partial void LogLoadSaveDataException(Exception ex);

    /// <summary>
    /// 记录加载游戏结果记录时的异常
    /// </summary>
    /// <param name="ex">异常对象</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "LoadGameResultsException",
        Level = LogLevel.Warning,
        Message = "Load game results exception"
    )]
    private partial void LogLoadGameResultsException(Exception ex);

    /// <summary>
    /// 记录保存游戏存档数据时的异常
    /// </summary>
    /// <param name="ex">异常对象</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "SaveGameSaveDataException",
        Level = LogLevel.Warning,
        Message = "Save game save data exception"
    )]
    private partial void LogSaveGameSaveDataException(Exception ex);

    /// <summary>
    /// 记录保存游戏结果记录时的异常
    /// </summary>
    /// <param name="ex">异常对象</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "SaveGameResultsException",
        Level = LogLevel.Warning,
        Message = "Save game results exception"
    )]
    private partial void LogSaveGameResultsException(Exception ex);

    /// <summary>
    /// 记录游戏存档成功保存的日志信息
    /// </summary>
    [LoggerMessage(
        EventId = 5,
        EventName = "GameSaveDataSaved",
        Level = LogLevel.Information,
        Message = "Game save data saved, isDeleted: {IsDeleted}"
    )]
    private partial void LogGameSaveDataSaved(bool isDeleted);

    /// <summary>
    /// 记录游戏结果成功保存的日志信息
    /// </summary>
    [LoggerMessage(
        EventId = 6,
        EventName = "GameResultsSaved",
        Level = LogLevel.Information,
        Message = "Game results saved"
    )]
    private partial void LogGameResultsSaved();
}
