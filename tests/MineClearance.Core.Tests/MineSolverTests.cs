using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.Core.Services;
using Moq;
using System.Collections;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="MineSolver"/> 的单元测试, 覆盖二选一僵局、必死格判定、快速路径与雷位重排合法性
/// </summary>
public sealed class MineSolverTests
{
    /// <summary>
    /// 被测试的地雷求解器, 无状态可复用
    /// </summary>
    private static readonly MineSolver Solver = new();

    [Fact]
    public void TrySafeOpen_二选一僵局_点候选格之一_返回安全解()
    {
        // 布局: 3x3, 1 颗雷; 已开 (0,0)=1; 未开 (0,1),(1,0),(1,1) 恰好一雷 (50/50 僵局)
        var config = new GameConfig(3, 3, 1);
        var board = CreateBoard(3, 3, (new(0, 0), CellType.Number));
        var mineField = CreateMineField(3, 3, [new(1, 1)]);
        var target = new Position(1, 1);

        var newMap = Solver.TrySafeOpen(config, board, mineField, target);

        Assert.NotNull(newMap);
        AssertValidRelayout(config, board, mineField, target, newMap);
    }

    [Fact]
    public void TrySafeOpen_二选一僵局_点另一候选格_返回安全解()
    {
        // 与上例对称: 初始雷位在 (0,1), 点 (0,1) 同样可救
        var config = new GameConfig(3, 3, 1);
        var board = CreateBoard(3, 3, (new(0, 0), CellType.Number));
        var mineField = CreateMineField(3, 3, [new(0, 1)]);
        var target = new Position(0, 1);

        var newMap = Solver.TrySafeOpen(config, board, mineField, target);

        Assert.NotNull(newMap);
        AssertValidRelayout(config, board, mineField, target, newMap);
    }

    [Fact]
    public void TrySafeOpen_必死格_唯一解中该格必为雷_返回null()
    {
        // 布局: 4x4, 3 颗雷; 已开 (0,0)=3, 其邻域三格必须全雷; 点 (0,1) 安全则邻域最多 2 雷, 约束无解
        var config = new GameConfig(4, 4, 3);
        var board = CreateBoard(4, 4, (new(0, 0), CellType.Number));
        var mineField = CreateMineField(4, 4, [new(0, 1), new(1, 0), new(1, 1)]);
        var target = new Position(0, 1);

        Assert.Null(Solver.TrySafeOpen(config, board, mineField, target));
    }

    [Fact]
    public void TrySafeOpen_目标远离数字_与最近自由格交换雷位()
    {
        // 布局: 5x5, 2 颗雷在 (0,0) 和 (3,3); 数字格 (2,2)=1, 邻域恰好一雷 (无必安全格); (0,0) 不在其邻域
        var config = new GameConfig(5, 5, 2);
        var board = CreateBoard(5, 5, (new(2, 2), CellType.Number));
        var mineField = CreateMineField(5, 5, [new(0, 0), new(3, 3)]);
        var target = new Position(0, 0);

        var newMap = Solver.TrySafeOpen(config, board, mineField, target);

        Assert.NotNull(newMap);
        AssertValidRelayout(config, board, mineField, target, newMap);
        Assert.True(newMap[new Position(0, 1).ToIndex(5)]); // 距 target 最近的自由格 (0,1) 接收雷位
        Assert.True(newMap[new Position(3, 3).ToIndex(5)]); // 其余雷位保持不变
    }

    [Fact]
    public void TrySafeOpen_存在必安全格_不挽救猜测()
    {
        // 布局: 5x5, 1 颗雷在 (0,0); 数字格 (2,2)=0, 邻域 8 格必安全, 玩家有确定动作; 点远处雷格不挽救
        var config = new GameConfig(5, 5, 1);
        var board = CreateBoard(5, 5, (new(2, 2), CellType.Number));
        var mineField = CreateMineField(5, 5, [new(0, 0)]);
        var target = new Position(0, 0);

        Assert.Null(Solver.TrySafeOpen(config, board, mineField, target));
    }

    [Fact]
    public void TrySafeOpen_旗子标错_不当约束仍可求解()
    {
        // 布局: 3x3, 1 颗雷在 (0,1); 玩家在 (1,1) 插了错误的旗, 求解时不应将其固定
        var config = new GameConfig(3, 3, 1);
        var board = CreateBoard(3, 3, (new(0, 0), CellType.Number), (new(1, 1), CellType.Flagged));
        var mineField = CreateMineField(3, 3, [new(0, 1)]);
        var target = new Position(0, 1);

        var newMap = Solver.TrySafeOpen(config, board, mineField, target);

        Assert.NotNull(newMap);
        AssertValidRelayout(config, board, mineField, target, newMap);
    }

    [Fact]
    public void TrySafeOpen_无已开数字格_目标直接交换雷位()
    {
        // 布局: 3x3, 1 颗雷在 (0,0), 全棋盘未开, 无任何约束
        var config = new GameConfig(3, 3, 1);
        var board = CreateBoard(3, 3);
        var mineField = CreateMineField(3, 3, [new(0, 0)]);
        var target = new Position(0, 0);

        var newMap = Solver.TrySafeOpen(config, board, mineField, target);

        Assert.NotNull(newMap);
        AssertValidRelayout(config, board, mineField, target, newMap);
    }

    [Fact]
    public void TrySafeOpen_相同输入_结果一致()
    {
        // 确定性: 相同局面与目标, 两次求解返回完全相同的雷位图
        var config = new GameConfig(5, 5, 3);
        var board = CreateBoard(5, 5, (new(0, 0), CellType.Number), (new(2, 2), CellType.Number));
        var target = new Position(0, 1);
        Position[] mines = [new(0, 1), new(2, 3), new(4, 4)];

        var first = Solver.TrySafeOpen(config, board, CreateMineField(5, 5, mines), target);
        var second = Solver.TrySafeOpen(config, board, CreateMineField(5, 5, mines), target);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.Cast<bool>().ToArray(), second.Cast<bool>().ToArray());
    }

    /// <summary>
    /// 创建指定格子状态的棋盘
    /// </summary>
    /// <param name="rows">棋盘行数</param>
    /// <param name="columns">棋盘列数</param>
    /// <param name="cells">已开格子及其类型</param>
    /// <returns>棋盘实例</returns>
    private static IGameBoardDictionary CreateBoard(int rows, int columns, params (Position Position, CellType Type)[] cells)
    {
        var board = new GameBoardDictionaryFactory().CreateGameBoardDictionary(rows, columns);
        foreach (var (position, type) in cells)
        {
            board[position].Type = type;
        }
        return board;
    }

    /// <summary>
    /// 创建指定雷位的地雷场
    /// </summary>
    /// <param name="rows">棋盘行数</param>
    /// <param name="columns">棋盘列数</param>
    /// <param name="mines">地雷位置集合</param>
    /// <returns>地雷场实例</returns>
    private static MineField CreateMineField(int rows, int columns, Position[] mines)
    {
        var mineMap = new BitArray(rows * columns);
        foreach (var mine in mines)
        {
            mineMap[mine.ToIndex(columns)] = true;
        }
        var mineField = new MineField(new Mock<IMineGenerator>().Object);
        mineField.Apply(new(rows, columns, mines.Length), mineMap);
        return mineField;
    }

    /// <summary>
    /// 断言重排后的雷位图合法: 雷数守恒, 目标格安全, 且所有已开数字格的计数保持不变
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="board">棋盘</param>
    /// <param name="mineField">地雷场 (换雷前的状态)</param>
    /// <param name="target">目标格</param>
    /// <param name="newMap">重排后的雷位图</param>
    private static void AssertValidRelayout(
        GameConfig config, IGameBoardDictionary board, MineField mineField, Position target, BitArray newMap)
    {
        // 雷数守恒
        Assert.Equal(config.MineCount, newMap.Cast<bool>().Count(isMine => isMine));

        // 目标格安全
        Assert.False(newMap[target.ToIndex(config.BoardWidth)]);

        // 记录换雷前所有已开数字格的计数
        var originalCounts = board
            .Where(kvp => kvp.Value.Type is CellType.Number or CellType.WarningNumber)
            .ToDictionary(kvp => kvp.Key, kvp => mineField.GetAdjacentMineCount(kvp.Key));

        // 应用新雷位后, 所有已开数字格的计数必须保持不变
        mineField.Apply(config, newMap);
        foreach (var (position, originalCount) in originalCounts)
        {
            Assert.Equal(originalCount, mineField.GetAdjacentMineCount(position));
        }
    }
}
