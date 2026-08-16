using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System.Diagnostics;
using System.Linq;

namespace MineClearance.Core.Services;

// Game 类的私有实现部分
internal partial class Game
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
    /// 日志记录器, 用于记录游戏运行时的日志信息
    /// </summary>
    private readonly ILogger<Game> _logger;

    /// <summary>
    /// 内部地雷场
    /// </summary>
    private readonly IMineField _mineField;

    /// <summary>
    /// 内部地雷求解器
    /// </summary>
    private readonly IMineSolver _mineSolver;

    /// <summary>
    /// 更新当前游戏的完成度, 并返回是否已完成
    /// </summary>
    /// <returns><see langword="true"/> 如果游戏已完成, 否则为 <see langword="false"/></returns>
    private bool UpdateCompletion()
    {
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
        // 如果游戏已经结束, 则不需要继续处理
        if (Status is GameStatus.Won or GameStatus.Lost) { return; }

        // 获取当前位置的格子
        var cell = Board[position];

        // 如果该位置不是未打开的格子, 则不需要继续处理
        if (cell.Type is not CellType.Unopened) { return; }

        // 判断打开的格子是否是地雷
        if (_mineField.IsMine(position))
        {
            // 尝试安全打开格子
            if (_mineSolver.TrySafeOpen(Config, Board, _mineField, position) is not { } newMineMap)
            {
                // 如果是地雷, 则游戏失败
                Timer.Pause();
                cell.Type = CellType.OpenedMine;
                foreach (var (p, c) in Board)
                {
                    var isMine = _mineField.IsMine(p);
                    if (c.Type is CellType.Unopened && isMine)
                    {
                        c.Type = CellType.Mine;
                    }
                    else if (c.Type is CellType.Flagged && !isMine)
                    {
                        c.Type = CellType.ErrorFlag;
                    }
                    else if (c.Type is CellType.Question)
                    {
                        c.Type = isMine ? CellType.Mine : CellType.Unopened;
                    }
                }
                Status = GameStatus.Lost;
                UpdateGameResult();
                return;
            }

            // 如果安全打开格子成功, 则更新地雷场
            _mineField.Apply(Config, newMineMap);
            LogMineFieldReplaced();
        }

        // 获取当前位置是否为空白格子
        var isEmpty = _mineField.GetAdjacentMineCount(position) == 0;

        // 更新当前位置的格子类型
        cell.Type = isEmpty ? CellType.Empty : CellType.Number;

        // 如果当前位置周围有地雷, 则不需要继续处理
        if (!isEmpty) { return; }

        // 遍历该位置的所有相邻位置, 递归打开相邻格子
        foreach (var pos in position.GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth))
        {
            FloodOpen(pos);
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
            foreach (var (p, c) in Board)
            {
                if (c.Type is CellType.Unopened or CellType.Question && _mineField.IsMine(p))
                {
                    c.Type = CellType.Flagged;
                }
            }
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
        Debug.Assert(Timer.FirstStartTime is not null,
            $"{nameof(Timer.FirstStartTime)} should not be null when updating game result."
        );

        if (Status is GameStatus.Won)
        {
            Result = Difficulty is GameDifficulty.Custom
                ? GameResult.CreateCustomWin(Seed, Timer.FirstStartTime.Value, Timer.Elapsed, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
                : GameResult.CreateWin(Seed, Difficulty, Timer.FirstStartTime.Value, Timer.Elapsed);

            LogGameResult(Result);
        }
        else if (Status is GameStatus.Lost)
        {
            Result = Difficulty is GameDifficulty.Custom
                ? GameResult.CreateCustomLoss(Seed, Timer.FirstStartTime.Value, Timer.Elapsed, Completion, Config.BoardHeight, Config.BoardWidth, Config.MineCount)
                : GameResult.CreateLoss(Seed, Difficulty, Timer.FirstStartTime.Value, Timer.Elapsed, Completion);

            LogGameResult(Result);
        }
    }

    /// <summary>
    /// 检测所有数字格子的警告状态是否需要更新
    /// </summary>
    private void CheckAndUpdateWarningStates()
    {
        // 遍历所有位置和格子, 检测数字格子的警告状态是否需要更新
        foreach (var (position, cell) in Board)
        {
            // 如果该格子是数字格子
            if (cell.Type is CellType.Number or CellType.WarningNumber)
            {
                // 计算该格子周围标记为旗子的格子数量
                var adjacentFlaggedCount = position
                    .GetAdjacentPositions(Config.BoardHeight, Config.BoardWidth)
                    .Count(adjacentPosition => Board[adjacentPosition].Type is CellType.Flagged);

                // 如果周围旗子格子数量大于实际地雷数量, 则将该格子类型设置为警告数字格子, 否则设置为普通数字格子
                cell.Type = adjacentFlaggedCount > _mineField.GetAdjacentMineCount(position)
                    ? CellType.WarningNumber
                    : CellType.Number;
            }
        }
    }
}
