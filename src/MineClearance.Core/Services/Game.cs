using Microsoft.Extensions.DependencyInjection;
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
internal sealed class Game : IGame
{
    /// <summary>
    /// 当前实例是否已被释放
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// 服务作用域, 用于管理依赖注入的生命周期
    /// </summary>
    private readonly IServiceScope _serviceScope;

    /// <summary>
    /// 游戏棋盘字典工厂
    /// </summary>
    private readonly IGameBoardDictionaryFactory _boardFactory;

    /// <summary>
    /// 内部地雷场
    /// </summary>
    private readonly IMineField _mineField;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public IGameBoardDictionary? Board
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
                PropertyChanged?.Invoke(this, new(nameof(Board)));
            }
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

    /// <summary>
    /// 以开始新游戏的方式初始化游戏实例
    /// </summary>
    /// <param name="serviceScope">服务作用域</param>
    /// <param name="boardFactory">游戏棋盘字典工厂</param>
    /// <param name="mineField">内部地雷场</param>
    /// <param name="timer">游戏计时器</param>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="config">游戏配置</param>
    /// <param name="seed">随机种子</param>
    public Game(
        IServiceScope serviceScope,
        IGameBoardDictionaryFactory boardFactory,
        IMineField mineField,
        IGameTimer timer,
        GameDifficulty difficulty,
        GameConfig config,
        int seed)
    {
        Debug.Assert(difficulty is GameDifficulty.Custom || GameConfig.FromDifficulty(difficulty) == config, $"{nameof(config)} must match the specified difficulty.");
        _serviceScope = serviceScope;
        _boardFactory = boardFactory;
        _mineField = mineField;
        Timer = timer;
        Difficulty = difficulty;
        Config = config;
        Seed = seed;
    }

    /// <summary>
    /// 以从保存的游戏状态恢复的方式初始化游戏实例
    /// </summary>
    /// <param name="serviceScope">服务作用域</param>
    /// <param name="boardFactory">游戏棋盘字典工厂</param>
    /// <param name="mineField">内部地雷场</param>
    /// <param name="timer">游戏计时器</param>
    /// <param name="saveData">游戏存档数据</param>
    public Game(
        IServiceScope serviceScope,
        IGameBoardDictionaryFactory boardFactory,
        IMineField mineField,
        IGameTimer timer,
        GameSaveData saveData)
    {
        _serviceScope = serviceScope;
        _boardFactory = boardFactory;
        _mineField = mineField;
        Timer = timer;
        Difficulty = saveData.Difficulty;
        Config = GameConfig.FromGameSaveData(saveData);
        Seed = saveData.Seed;

        // 立刻生成地雷场
        var adjacentMineCounts = _mineField.Generate(Config, saveData.MineField);

        // 创建游戏棋盘字典
        Board = _boardFactory.CreateGameBoardDictionary(Config.BoardHeight, Config.BoardWidth, adjacentMineCounts);

        // 将游戏状态设置为暂停, 等待玩家取消暂停后继续游戏
        Status = GameStatus.Paused;

        // 遍历存档中的格子状态, 并将其应用到游戏棋盘字典中
        foreach (var (position, cellType) in saveData.CellStates)
        {
            // 将存档中的格子状态应用到游戏棋盘字典中
            Board[position].Type = cellType;
        }

        // 更新游戏完成度
        if (UpdateCompletion())
        {
            Debug.WriteLine("Game is already completed when loading from save data.");
        }

        // 设置计时器的初始时间为存档中的已运行时间
        Timer.SetInitialTime(saveData.Duration);
    }

    /// <inheritdoc/>
    public void Pause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        AssertGamePerformable();
        Timer.Pause();
        Status = GameStatus.Paused;
    }

    /// <inheritdoc/>
    public void CancelPause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Debug.Assert(Status is GameStatus.Paused, "Game must be paused to cancel pause.");
        if (Board is null)
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
        AssertGamePerformable();

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 需要先生成地雷场和游戏棋盘字典
        if (Board is null)
        {
            // 生成地雷场
            var adjacentMineCounts = _mineField.Generate(Config, position, Seed);

            // 创建游戏棋盘字典
            Board = _boardFactory.CreateGameBoardDictionary(Config.BoardHeight, Config.BoardWidth, adjacentMineCounts);

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

        // 断言当前游戏处于可以进行操作的状态, 因为在游戏暂停或结束后不允许再标记格子
        AssertGamePerformable();

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法标记格子
        if (Board is null) { return; }

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

        // 断言当前游戏处于可以进行操作的状态, 因为在游戏暂停或结束后不允许再标记格子
        AssertGamePerformable();

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法标记格子
        if (Board is null) { return; }

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

        // 断言当前游戏处于可以进行操作的状态, 因为在游戏暂停或结束后不允许再取消标记格子
        AssertGamePerformable();

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法取消标记格子
        if (Board is null) { return; }

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

        // 断言当前游戏处于可以进行操作的状态, 因为在游戏暂停或结束后不允许再打开格子
        AssertGamePerformable();

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法打开相邻格子
        if (Board is null) { return; }

        // 获取当前位置的格子
        var cell = Board[position];

        // 如果指定位置的格子不是数字格子, 则无法打开相邻格子
        if (cell.Type is not CellType.Number) { return; }

        // 获取该位置周围的位置集合
        var adjacentPositions = position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth);

        // 如果指定位置周围的旗子数量等于该数字格子的数字
        if (cell.AdjacentMineCount == adjacentPositions.Count(adjacentPosition => Board[adjacentPosition].Type is CellType.Flagged))
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

        // 断言当前游戏处于可以进行操作的状态, 因为在游戏暂停或结束后不允许再标记格子
        AssertGamePerformable();

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法标记相邻格子
        if (Board is null) { return; }

        // 获取当前位置的格子
        var cell = Board[position];

        // 如果指定位置的格子不是数字格子, 则无法标记相邻格子
        if (cell.Type is not CellType.Number) { return; }

        // 保存该位置周围所有未打开的相邻格子位置, 用于后续标记为旗子
        List<Position> nonRevealedAdjacentPositions = [];

        // 遍历该位置周围的所有相邻位置
        foreach (var adjacentPosition in position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth))
        {
            // 如果该相邻位置的格子是未打开的格子、问号格子或旗子格子, 则将其加入未打开的相邻格子列表
            if (Board[adjacentPosition].Type is CellType.Unopened or CellType.Question or CellType.Flagged)
            {
                nonRevealedAdjacentPositions.Add(adjacentPosition);
            }
        }

        // 如果指定位置周围的旗子数量等于该数字格子的数字, 则将所有未打开的相邻格子标记为旗子
        if (cell.AdjacentMineCount == nonRevealedAdjacentPositions.Count)
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
    public GameSaveData? GetSaveData()
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法获取存档数据
        if (Board is null) { return null; }

        // 如果当前游戏已结束, 则不用获取存档数据, 因为游戏已结束, 无法继续进行
        if (Status is GameStatus.Won or GameStatus.Lost) { return null; }

        // 此时计时器的 FirstStartTime 属性不应为 null, 因为游戏已经开始过
        Debug.Assert(Timer.FirstStartTime is not null, $"{nameof(Timer.FirstStartTime)} should not be null when getting save data for an ongoing game.");

        var startTime = Timer.FirstStartTime.Value;

        // 获取地雷分布的位图表示
        var mineField = _mineField.GetMineMap();

        // 获取所有非未打开格子的状态
        var cellStates = Board.GetCellStates();

        // 返回游戏存档数据
        return Difficulty is GameDifficulty.Custom
            ? GameSaveData.CreateCustom(Seed, startTime, Timer.Elapsed, mineField, cellStates, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
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
    }

    /// <summary>
    /// 断言当前游戏处于可以进行操作的状态
    /// </summary>
    private void AssertGamePerformable()
    {
        const string errorMessage = "Game must be in progress or waiting to start to perform this operation.";
        Debug.Assert(Status is GameStatus.WaitingStarted or GameStatus.InProgress, errorMessage);
    }

    /// <summary>
    /// 更新当前游戏的完成度, 并返回是否已完成
    /// </summary>
    /// <returns><see langword="true"/> 如果游戏已完成, 否则为 <see langword="false"/></returns>
    private bool UpdateCompletion()
    {
        // 如果游戏棋盘字典为空, 则完成度为 0.0
        if (Board is null)
        {
            Completion = 0.0;
            return false;
        }

        // 获取已经打开的格子数量
        var openedCount = Board.OpenedCount;

        // 获取要打开的格子总数
        var totalCellsToOpen = Config.TotalCellsToOpen;

        // 计算完成度百分比
        Completion = Constants.MaxCompletion * openedCount / totalCellsToOpen;

        // 返回游戏是否已完成
        return openedCount == totalCellsToOpen;
    }

    /// <summary>
    /// 泛洪打开指定位置的格子, 如果该位置周围没有地雷, 则递归打开所有相邻的格子
    /// </summary>
    /// <param name="position">要打开的格子位置</param>
    private void FloodOpen(Position position)
    {
        // 调用该方法时, 游戏棋盘字典不应为 null, 因为该方法只在游戏进行中调用
        Debug.Assert(Board is not null, $"{nameof(Board)} should not be null when calling FloodOpen.");

        // 如果游戏已经结束, 则不需要继续处理
        if (Status is GameStatus.Won or GameStatus.Lost) { return; }

        // 获取当前位置的格子
        var cell = Board[position];

        // 如果该位置不是未打开的格子, 则不需要继续处理
        if (cell.Type is not CellType.Unopened) { return; }

        // 判断打开的格子是否是地雷
        if (_mineField.IsMine(position))
        {
            // 如果是地雷, 则游戏失败
            Timer.Pause();
            cell.Type = CellType.Mine;
            Status = GameStatus.Lost;
            UpdateGameResult();
            return;
        }

        // 更新当前位置的格子类型
        cell.Type = cell.AdjacentMineCount == 0 ? CellType.Empty : CellType.Number;

        // 如果该位置周围有地雷, 则不需要继续递归打开相邻格子
        if (cell.AdjacentMineCount > 0) { return; }

        // 遍历该位置的所有相邻位置, 递归打开相邻格子
        foreach (var adjacentPosition in position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth))
        {
            FloodOpen(adjacentPosition);
        }
    }

    /// <summary>
    /// 在泛洪打开格子后检查游戏是否已完成, 如果已完成则更新游戏状态为胜利
    /// </summary>
    private void CheckGameCompletion()
    {
        // 如果游戏已失败, 则不需要检查游戏是否已完成
        if (Status is GameStatus.Lost) { return; }

        // 更新游戏完成度
        if (UpdateCompletion())
        {
            // 如果游戏已完成, 则游戏胜利
            Timer.Pause();
            Status = GameStatus.Won;
            UpdateGameResult();
        }
        else
        {
            // 如果游戏未完成, 则检查所有数字格子的警告状态是否需要更新
            CheckAndUpdateWarningStates();
        }
    }

    /// <summary>
    /// 在游戏结束时创建并更新游戏结果
    /// </summary>
    private void UpdateGameResult()
    {
        Debug.Assert(Timer.FirstStartTime is not null, $"{nameof(Timer.FirstStartTime)} should not be null when updating game result.");

        if (Status is GameStatus.Won)
        {
            Result = Difficulty is GameDifficulty.Custom
                ? GameResult.CreateCustomWin(Seed, Timer.FirstStartTime.Value, Timer.Elapsed, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
                : GameResult.CreateWin(Seed, Difficulty, Timer.FirstStartTime.Value, Timer.Elapsed);
        }
        else if (Status is GameStatus.Lost)
        {
            Result = Difficulty is GameDifficulty.Custom
                ? GameResult.CreateCustomLoss(Seed, Timer.FirstStartTime.Value, Timer.Elapsed, Completion, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
                : GameResult.CreateLoss(Seed, Difficulty, Timer.FirstStartTime.Value, Timer.Elapsed, Completion);
        }
    }

    /// <summary>
    /// 检测所有数字格子的警告状态是否需要更新
    /// </summary>
    private void CheckAndUpdateWarningStates()
    {
        // 如果游戏棋盘字典为空, 则不需要更新警告状态
        if (Board is null) { return; }

        // 遍历所有位置和格子, 检测数字格子的警告状态是否需要更新
        foreach (var (position, cell) in Board)
        {
            // 如果该格子是数字格子
            if (cell.Type is CellType.Number or CellType.WarningNumber)
            {
                // 计算该格子周围标记为旗子的格子数量
                var adjacentFlaggedCount = position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth)
                    .Count(adjacentPosition => Board[adjacentPosition].Type is CellType.Flagged);

                // 如果周围标记为旗子的格子数量大于实际地雷数量, 则将该格子类型设置为警告数字格子, 否则设置为普通数字格子
                cell.Type = adjacentFlaggedCount > cell.AdjacentMineCount ? CellType.WarningNumber : CellType.Number;
            }
        }
    }
}
