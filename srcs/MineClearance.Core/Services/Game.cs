using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏核心实现类, 负责管理游戏状态、处理玩家操作
/// </summary>
internal sealed partial class Game : IGame
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public IGameBoardDictionary Board
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public IGameTimer Timer
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public GameStatus Status
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Status)));
                LogGameStatusChanged(value);
            }
        }
    }

    /// <inheritdoc/>
    public GameDifficulty Difficulty
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public GameConfig Config
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public int Seed
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public double Completion
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Completion)));
            }
        }
    }

    /// <inheritdoc/>
    public GameResult? Result
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Result)));
            }
        }
    }

    /// <inheritdoc/>
    public bool IsPerformable
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Status is GameStatus.WaitingStarted or GameStatus.InProgress;
        }
    }

    /// <inheritdoc/>
    public bool HasProgress
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Timer.FirstStartTime is not null && Status is GameStatus.InProgress or GameStatus.Paused;
        }
    }

    /// <summary>
    /// 以开始新游戏的方式初始化游戏实例
    /// </summary>
    /// <param name="serviceScope">服务作用域</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="mineField">内部地雷场</param>
    /// <param name="board">游戏棋盘字典</param>
    /// <param name="timer">游戏计时器</param>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="config">游戏配置</param>
    /// <param name="seed">随机种子</param>
    public Game(
        IServiceScope serviceScope,
        ILogger<Game> logger,
        IMineField mineField,
        IGameBoardDictionary board,
        IGameTimer timer,
        GameDifficulty difficulty,
        GameConfig config,
        int seed)
    {
        Debug.Assert(difficulty is GameDifficulty.Custom || GameConfig.FromDifficulty(difficulty) == config,
            $"{nameof(config)} must match the specified difficulty."
        );

        _serviceScope = serviceScope;
        _logger = logger;
        _mineField = mineField;
        Board = board;
        Timer = timer;
        Status = GameStatus.WaitingStarted;
        Difficulty = difficulty;
        Config = config;
        Seed = seed;
        LogGameCreated(difficulty, config, seed);
    }

    /// <summary>
    /// 以从保存的游戏状态恢复的方式初始化游戏实例
    /// </summary>
    /// <param name="serviceScope">服务作用域</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="mineField">内部地雷场</param>
    /// <param name="board">游戏棋盘字典</param>
    /// <param name="timer">游戏计时器</param>
    /// <param name="config">游戏配置</param>
    /// <param name="saveData">游戏存档数据</param>
    public Game(
        IServiceScope serviceScope,
        ILogger<Game> logger,
        IMineField mineField,
        IGameBoardDictionary board,
        IGameTimer timer,
        GameConfig config,
        GameSaveData saveData)
    {
        _serviceScope = serviceScope;
        _logger = logger;
        _mineField = mineField;
        Board = board;
        Timer = timer;
        Difficulty = saveData.Difficulty;
        Config = config;
        Seed = saveData.Seed;

        // 应用存档中的地雷场位图, 并获取表示每个位置周围地雷数量的数组
        _mineField.Apply(Config, saveData.MineField);

        // 将游戏状态设置为暂停, 等待玩家取消暂停后继续游戏
        Status = GameStatus.Paused;

        // 遍历存档中的格子状态, 并将其应用到游戏棋盘字典中
        foreach (var (position, cellType) in saveData.CellStates)
        {
            // 将存档中的格子状态应用到游戏棋盘字典中
            Board[position].Type = cellType;
        }

        // 更新游戏完成度
        var isCompleted = UpdateCompletion();
        Debug.Assert(!isCompleted, "Game should not be completed when loading from save data.");

        // 初始化计时器, 以便在取消暂停后继续计时
        Timer.Initial(saveData.StartTime, saveData.Duration);

        // 记录游戏被创建的日志信息
        LogGameCreated(Difficulty, Config, Seed);
    }

    /// <inheritdoc/>
    public void Pause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Debug.Assert(IsPerformable, "Game must be in progress or waiting to start to pause.");
        Timer.Pause();
        Status = GameStatus.Paused;
    }

    /// <inheritdoc/>
    public void CancelPause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Debug.Assert(Status is GameStatus.Paused, "Game must be paused to cancel pause.");
        if (Timer.FirstStartTime is null)
        {
            Status = GameStatus.WaitingStarted;
        }
        else
        {
            Status = GameStatus.InProgress;
            Timer.Start();
        }
    }

    /// <inheritdoc/>
    public void OpenCell(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 断言当前游戏处于可以进行操作的状态, 因为在游戏暂停或结束后不允许再打开格子
        Debug.Assert(IsPerformable, "Game must be in progress or waiting to start to open a cell.");

        // 如果游戏尚未开始, 需要先生成地雷场
        if (Status is GameStatus.WaitingStarted)
        {
            // 生成地雷场
            _mineField.Generate(Config, position, Seed);

            // 将游戏状态设置为进行中
            Status = GameStatus.InProgress;

            // 启动计时器
            Timer.Start();
        }

        // 打开格子
        FloodOpen(position);

        // 检查游戏是否已完成, 如果已完成则更新游戏状态为胜利
        CheckGameCompletion();
    }

    /// <inheritdoc/>
    public void FlagCell(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 断言当前游戏处于进行中的状态, 因为只有在游戏进行中才能标记格子
        Debug.Assert(Status is GameStatus.InProgress, "Game must be in progress to flag a cell.");

        // 将指定位置的格子插旗
        Board[position].Type = CellType.Flagged;

        // 检查所有数字格子的警告状态是否需要更新
        CheckAndUpdateWarningStates();
    }

    /// <inheritdoc/>
    public void QuestionCell(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 断言当前游戏处于进行中的状态, 因为只有在游戏进行中才能标记格子
        Debug.Assert(Status is GameStatus.InProgress, "Game must be in progress to question a cell.");

        // 获取当前位置的格子
        var cell = Board[position];

        // 记录是否需要更新所有数字格子的警告状态
        var needUpdateWarningStates = cell.Type is CellType.Flagged;

        // 将当前位置的格子标记为问号
        cell.Type = CellType.Question;

        // 如果之前该格子是旗子, 则需要检查所有数字格子的警告状态是否需要更新
        if (needUpdateWarningStates)
        {
            CheckAndUpdateWarningStates();
        }
    }

    /// <inheritdoc/>
    public void UnmarkCell(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 断言当前游戏处于进行中的状态, 因为只有在游戏进行中才能标记格子
        Debug.Assert(Status is GameStatus.InProgress, "Game must be in progress to unmark a cell.");

        // 获取当前位置的格子
        var cell = Board[position];

        // 记录是否需要更新所有数字格子的警告状态
        var needUpdateWarningStates = cell.Type is CellType.Flagged;

        // 将当前位置的格子取消标记
        cell.Type = CellType.Unopened;

        // 如果之前该格子是旗子, 则需要检查所有数字格子的警告状态是否需要更新
        if (needUpdateWarningStates)
        {
            CheckAndUpdateWarningStates();
        }
    }

    /// <inheritdoc/>
    public void OpenAdjacentCells(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 断言当前游戏处于进行中的状态, 因为只有在游戏进行中才能点击数字格子打开相邻格子
        Debug.Assert(Status is GameStatus.InProgress, "Game must be in progress to open adjacent cells.");

        // 获取当前位置的格子
        var cell = Board[position];

        // 如果指定位置的格子不是数字格子, 则无法打开相邻格子
        if (cell.Type is not CellType.Number) { return; }

        // 获取该位置周围的位置集合
        var adjacentPositions = position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth);

        // 如果指定位置周围的旗子数量等于该数字格子的数字
        if (_mineField.GetAdjacentMineCount(position) ==
            adjacentPositions.Count(pos => Board[pos].Type is CellType.Flagged))
        {
            // 遍历该位置周围的所有相邻位置, 并尝试打开相邻格子
            foreach (var adjacentPosition in adjacentPositions)
            {
                FloodOpen(adjacentPosition);
            }

            // 检查游戏是否已完成, 如果已完成则更新游戏状态为胜利
            CheckGameCompletion();
        }
    }

    /// <inheritdoc/>
    public void FlagAdjacentCells(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 断言当前游戏处于进行中的状态, 因为只有在游戏进行中才能点击数字格子标记相邻格子
        Debug.Assert(Status is GameStatus.InProgress, "Game must be in progress to flag adjacent cells.");

        // 获取当前位置的格子
        var cell = Board[position];

        // 如果指定位置的格子不是数字格子, 则无法标记相邻格子
        if (cell.Type is not CellType.Number) { return; }

        // 保存该位置周围所有未打开的相邻格子位置, 用于后续标记为旗子
        List<Position> nonRevealedAdjacentPositions = [];

        // 遍历该位置周围的所有相邻位置
        foreach (var adjacentPosition in
            position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth))
        {
            // 如果该相邻位置的格子是未打开的格子、问号格子或旗子格子, 则将其加入未打开的相邻格子列表
            if (Board[adjacentPosition].Type is CellType.Unopened or CellType.Question or CellType.Flagged)
            {
                nonRevealedAdjacentPositions.Add(adjacentPosition);
            }
        }

        // 如果指定位置周围的旗子数量等于该数字格子的数字, 则将所有未打开的相邻格子标记为旗子
        if (_mineField.GetAdjacentMineCount(position) == nonRevealedAdjacentPositions.Count)
        {
            // 遍历所有未打开的相邻格子位置, 并将其标记为旗子
            foreach (var adjacentPosition in nonRevealedAdjacentPositions)
            {
                Board[adjacentPosition].Type = CellType.Flagged;
            }

            // 检查所有数字格子的警告状态是否需要更新
            CheckAndUpdateWarningStates();
        }
    }

    /// <inheritdoc/>
    public int GetAdjacentMineCount(Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 返回指定位置周围的地雷数量
        return _mineField.GetAdjacentMineCount(position);
    }

    /// <inheritdoc/>
    public GameSaveData? GetSaveData()
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 如果游戏尚未开始或已结束, 则无法获取游戏存档数据
        if (Status is GameStatus.WaitingStarted or GameStatus.Won or GameStatus.Lost) { return null; }

        // 此时计时器的 FirstStartTime 属性不应为 null, 因为游戏已经开始过
        Debug.Assert(
            Timer.FirstStartTime is not null,
            $"{nameof(Timer.FirstStartTime)} should not be null when getting save data for an ongoing game."
        );

        // 获取游戏开始时间
        var startTime = Timer.FirstStartTime.Value;

        // 获取地雷分布的位图表示
        var mineField = _mineField.GetMineMap();

        // 获取所有非未打开格子的状态
        var cellStates = Board.GetCellStates();

        // 返回游戏存档数据
        return Difficulty is GameDifficulty.Custom
            ? GameSaveData.CreateCustom(
                Seed, startTime, Timer.Elapsed, mineField,
                cellStates, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
            : GameSaveData.Create(Seed, Difficulty, startTime, Timer.Elapsed, mineField, cellStates);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 如果当前实例已被释放, 则不需要再次释放
        if (_disposed) { return; }

        // 释放服务作用域, 以便释放所有依赖注入的资源
        _serviceScope.Dispose();

        // 标记当前实例已被释放
        _disposed = true;

        // 通知 GC 不再调用终结器, 因为已经手动释放了资源
        GC.SuppressFinalize(this);
    }
}
