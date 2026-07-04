using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MineClearance.Core.Services;

// TODO: 添加数字格子到警告数字格子的检验
/// <summary>
/// 游戏核心实现类, 负责管理游戏状态、处理玩家操作
/// </summary>
internal sealed class Game : Interfaces.IGame
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
    private readonly Interfaces.IGameBoardDictionaryFactory _boardFactory;

    /// <summary>
    /// 内部地雷场
    /// </summary>
    private readonly Interfaces.IMineField _mineField;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public Interfaces.IGameBoardDictionary? Board
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
    public Interfaces.IGameTimer Timer
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public Enums.GameStatus Status
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
    public Enums.GameDifficulty Difficulty
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
    }

    /// <inheritdoc/>
    public Models.Records.GameConfig Config
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
        Interfaces.IGameBoardDictionaryFactory boardFactory,
        Interfaces.IMineField mineField,
        Interfaces.IGameTimer timer,
        Enums.GameDifficulty difficulty,
        Models.Records.GameConfig config,
        int seed)
    {
        Debug.Assert(difficulty is Enums.GameDifficulty.Custom || Models.Records.GameConfig.FromDifficulty(difficulty) == config, "GameConfig must match the specified difficulty.");
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
        Interfaces.IGameBoardDictionaryFactory boardFactory,
        Interfaces.IMineField mineField,
        Interfaces.IGameTimer timer,
        Models.Records.GameSaveData saveData)
    {
        _serviceScope = serviceScope;
        _boardFactory = boardFactory;
        _mineField = mineField;
        Timer = timer;
        Difficulty = saveData.Difficulty;
        Config = Models.Records.GameConfig.FromGameSaveData(saveData);
        Seed = saveData.Seed;

        // 立刻生成地雷场
        var adjacentMineCounts = _mineField.Generate(Config, saveData.MineField);

        // 创建游戏棋盘字典
        Board = _boardFactory.CreateGameBoardDictionary(Config.BoardHeight, Config.BoardWidth, adjacentMineCounts);

        // 将游戏状态设置为暂停, 等待玩家取消暂停后继续游戏
        Status = Enums.GameStatus.Paused;

        // 遍历存档中的格子状态, 并将其应用到游戏棋盘字典中
        foreach (var (position, cellType) in saveData.CellStates)
        {
            // 将存档中的格子状态应用到游戏棋盘字典中
            Board[position].Type = cellType;
        }

        // 更新游戏完成度
        UpdateCompletion();

        // 设置计时器的初始时间为存档中的已运行时间
        Timer.SetInitialTime(saveData.Duration);
    }

    /// <inheritdoc/>
    public void Pause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Debug.Assert(Status is not (Enums.GameStatus.Won or Enums.GameStatus.Lost), "Cannot pause a game that has already ended.");
        Timer.Pause();
        Status = Enums.GameStatus.Paused;
    }

    /// <inheritdoc/>
    public void CancelPause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Debug.Assert(Status is not (Enums.GameStatus.Won or Enums.GameStatus.Lost), "Cannot cancel pause on a game that has already ended.");
        if (Board is null)
        {
            Status = Enums.GameStatus.WaitingStarted;
        }
        else
        {
            Status = Enums.GameStatus.InProgress;
            Timer.Start();
        }
    }

    /// <inheritdoc/>
    public void OpenCell(Models.Records.Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 在游戏结束后, 不允许再打开格子
        Debug.Assert(Status is not (Enums.GameStatus.Won or Enums.GameStatus.Lost), "Cannot open a cell in a game that has already ended.");

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 需要先生成地雷场和游戏棋盘字典
        if (Board is null)
        {
            // 生成地雷场
            var adjacentMineCounts = _mineField.Generate(Config, position, Seed);

            // 创建游戏棋盘字典
            Board = _boardFactory.CreateGameBoardDictionary(Config.BoardHeight, Config.BoardWidth, adjacentMineCounts);

            // 将游戏状态设置为进行中
            Status = Enums.GameStatus.InProgress;

            // 启动计时器
            Timer.Start();
        }

        // 判断打开的格子是否是地雷
        if (_mineField.IsMine(position))
        {
            // 如果是地雷, 则游戏失败
            Timer.Pause();
            Board[position].Type = Enums.CellType.Mine;
            UpdateCompletion();
            Status = Enums.GameStatus.Lost;
        }
        else
        {
            // 如果不是地雷, 则打开格子
            FloodOpen(position);

            // 更新游戏完成度
            UpdateCompletion();

            // 检查游戏是否已完成
            if (IsGameCompleted())
            {
                // 如果游戏已完成, 则游戏胜利
                Timer.Pause();
                Status = Enums.GameStatus.Won;
            }
        }
    }

    /// <inheritdoc/>
    public void FlagCell(Models.Records.Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 在游戏结束后, 不允许再标记格子
        Debug.Assert(Status is not (Enums.GameStatus.Won or Enums.GameStatus.Lost), "Cannot flag a cell in a game that has already ended.");

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法标记格子
        if (Board is null) { return; }

        // 将指定位置的格子插旗
        Board[position].Type = Enums.CellType.Flagged;
    }

    /// <inheritdoc/>
    public void QuestionCell(Models.Records.Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 在游戏结束后, 不允许再标记格子
        Debug.Assert(Status is not (Enums.GameStatus.Won or Enums.GameStatus.Lost), "Cannot mark a cell in a game that has already ended.");

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法标记格子
        if (Board is null) { return; }

        // 将指定位置的格子标记为问号
        Board[position].Type = Enums.CellType.Question;
    }

    /// <inheritdoc/>
    public void UnmarkCell(Models.Records.Position position)
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 在游戏结束后, 不允许再取消标记格子
        Debug.Assert(Status is not (Enums.GameStatus.Won or Enums.GameStatus.Lost), "Cannot unmark a cell in a game that has already ended.");

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法取消标记格子
        if (Board is null) { return; }

        // 将指定位置的格子取消标记
        Board[position].Type = Enums.CellType.Unopened;
    }

    /// <inheritdoc/>
    public Models.Records.GameSaveData? GetSaveData()
    {
        // 如果当前实例已被释放, 则抛出异常
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 如果游戏棋盘字典为空, 则表示游戏尚未开始, 无法获取存档数据
        if (Board is null) { return null; }

        // 如果当前游戏已结束, 则不用获取存档数据, 因为游戏已结束, 无法继续进行
        if (Status is Enums.GameStatus.Won or Enums.GameStatus.Lost) { return null; }

        // 此时计时器的 FirstStartTime 属性不应为 null, 因为游戏已经开始过
        Debug.Assert(Timer.FirstStartTime is not null, "Timer.FirstStartTime should not be null when getting save data for an ongoing game.");

        var startTime = Timer.FirstStartTime.Value;

        // 获取地雷分布的位图表示
        var mineField = _mineField.GetMineMap();

        // 获取所有非未打开格子的状态
        var cellStates = Board.GetCellStates();

        // 返回游戏存档数据
        return Difficulty is Enums.GameDifficulty.Custom
            ? Models.Records.GameSaveData.CreateCustom(Seed, startTime, Timer.Elapsed, mineField, cellStates, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
            : Models.Records.GameSaveData.Create(Seed, Difficulty, startTime, Timer.Elapsed, mineField, cellStates);
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
    /// 更新当前游戏的完成度, 根据已打开的格子数量和要打开的格子总数计算完成度百分比
    /// </summary>
    private void UpdateCompletion()
    {
        // 如果游戏棋盘字典为空, 则完成度为 0
        if (Board is null)
        {
            Completion = 0;
            return;
        }

        // 计算完成度百分比
        Completion = (double)Board.OpenedCount / Config.GetTotalCellsToOpen();
    }

    /// <summary>
    /// 泛洪打开指定位置的格子, 如果该位置周围没有地雷, 则递归打开所有相邻的格子
    /// </summary>
    /// <param name="position">要打开的格子位置</param>
    private void FloodOpen(Models.Records.Position position)
    {
        // 调用该方法时, 游戏棋盘字典不应为 null, 因为该方法只在游戏进行中调用
        Debug.Assert(Board is not null, "Board should not be null when calling FloodOpen.");

        // 如果该位置不是未打开的格子, 则不需要继续处理
        if (Board[position].Type is not Enums.CellType.Unopened)
        {
            return;
        }

        // 获取该位置周围的地雷数量
        var adjacentMineCount = _mineField.GetAdjacentMineCount(position);

        // 更新当前位置的格子类型
        Board[position].Type = adjacentMineCount == 0 ? Enums.CellType.Empty : Enums.CellType.Number;

        // 如果该位置周围有地雷, 则不需要继续递归打开相邻格子
        if (adjacentMineCount > 0)
        {
            return;
        }

        // 遍历该位置的所有相邻位置, 递归打开相邻格子
        foreach (var adjacentPosition in position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth))
        {
            FloodOpen(adjacentPosition);
        }
    }

    /// <summary>
    /// 判断当前游戏是否已完成, 即所有非地雷格子都已被打开
    /// </summary>
    /// <returns><see langword="true"/> 如果游戏已完成, 否则返回 <see langword="false"/></returns>
    private bool IsGameCompleted()
    {
        // 如果游戏棋盘字典为空, 则游戏不可能已完成
        if (Board is null) { return false; }

        // 如果已打开的格子数量等于要打开的格子总数, 则游戏已完成
        return Board.OpenedCount == Config.GetTotalCellsToOpen();
    }
}
