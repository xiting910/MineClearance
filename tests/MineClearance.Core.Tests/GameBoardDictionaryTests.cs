using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Services;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="GameBoardDictionary"/> 的单元测试, 覆盖格子访问、计数统计和状态导出
/// </summary>
public sealed class GameBoardDictionaryTests
{
    /// <summary>
    /// 棋盘行数
    /// </summary>
    private const int Rows = 3;

    /// <summary>
    /// 棋盘列数
    /// </summary>
    private const int Columns = 3;

    /// <summary>
    /// 测试布局: (0,1) 是地雷, 其余位置是周围地雷数量, 按行优先顺序排列
    /// </summary>
    private static readonly int[] AdjacentMineCounts = [1, -1, 1, 1, 1, 1, 0, 0, 0];

    /// <summary>
    /// 创建测试棋盘
    /// </summary>
    /// <returns>棋盘实例</returns>
    private static GameBoardDictionary CreateBoard()
    {
        return new(Rows, Columns, AdjacentMineCounts);
    }

    [Fact]
    public void 构造_创建全部格子_数量与邻居数正确()
    {
        var board = CreateBoard();

        Assert.Equal(9, board.Count);
        Assert.Equal(CellType.Unopened, board[new(0, 0)].Type);
        Assert.Equal(Constants.MineValue, board[new(0, 1)].AdjacentMineCount);
        Assert.Equal(1, board[new(0, 0)].AdjacentMineCount);
        Assert.Equal(0, board[new(2, 2)].AdjacentMineCount);
    }

    [Fact]
    public void 索引器_访问不存在的格子_抛出KeyNotFoundException()
    {
        var board = CreateBoard();

        _ = Assert.Throws<KeyNotFoundException>(() => board[new(9, 9)]);
    }

    [Fact]
    public void ContainsKey_判断位置是否存在()
    {
        var board = CreateBoard();

        Assert.True(board.ContainsKey(new(0, 0)));
        Assert.False(board.ContainsKey(new(9, 9)));
    }

    [Fact]
    public void TryGetValue_存在_返回格子和true()
    {
        var board = CreateBoard();

        var found = board.TryGetValue(new(0, 1), out var cell);

        Assert.True(found);
        Assert.Same(board[new(0, 1)], cell);
    }

    [Fact]
    public void TryGetValue_不存在_返回false()
    {
        var board = CreateBoard();

        var found = board.TryGetValue(new(9, 9), out var cell);

        Assert.False(found);
        Assert.Null(cell);
    }

    [Fact]
    public void 枚举_遍历全部格子()
    {
        var board = CreateBoard();

        var count = 0;
        foreach (var (position, _) in board)
        {
            count++;
            Assert.True(board.ContainsKey(position));
        }

        Assert.Equal(9, count);
        Assert.Equal(9, board.Keys.Count());
        Assert.Equal(9, board.Values.Count());
    }

    [Fact]
    public void GetCellStates_只包含非未打开格子()
    {
        var board = CreateBoard();
        board[new(0, 0)].Type = CellType.Number;
        board[new(2, 2)].Type = CellType.Flagged;

        var states = board.GetCellStates();

        Assert.Equal(2, states.Count);
        Assert.Equal(CellType.Number, states[new(0, 0)]);
        Assert.Equal(CellType.Flagged, states[new(2, 2)]);
    }

    [Fact]
    public void OpenedCount_随格子打开而变化()
    {
        var board = CreateBoard();
        Assert.Equal(0, board.OpenedCount);

        board[new(0, 0)].Type = CellType.Empty;
        board[new(1, 1)].Type = CellType.Number;
        board[new(2, 2)].Type = CellType.WarningNumber;
        Assert.Equal(3, board.OpenedCount);

        board[new(0, 0)].Type = CellType.Flagged; // 从打开变为旗子, 不再计入已打开
        Assert.Equal(2, board.OpenedCount);
    }

    [Fact]
    public void FlagCount_随插旗而变化()
    {
        var board = CreateBoard();
        Assert.Equal(0, board.FlagCount);

        board[new(0, 0)].Type = CellType.Flagged;
        board[new(1, 1)].Type = CellType.Flagged;
        Assert.Equal(2, board.FlagCount);

        board[new(0, 0)].Type = CellType.Question;
        Assert.Equal(1, board.FlagCount);
    }

    [Fact]
    public void QuestionCount_随问号而变化()
    {
        var board = CreateBoard();
        Assert.Equal(0, board.QuestionCount);

        board[new(0, 0)].Type = CellType.Question;
        Assert.Equal(1, board.QuestionCount);

        board[new(0, 0)].Type = CellType.Unopened;
        Assert.Equal(0, board.QuestionCount);
    }

    [Fact]
    public void 计数变化_触发属性变化事件()
    {
        var board = CreateBoard();
        var eventCount = 0;
        board.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IGameBoardDictionary.OpenedCount)) { eventCount++; }
        };

        board[new(0, 0)].Type = CellType.Number;
        Assert.Equal(1, eventCount);

        board[new(0, 0)].Type = CellType.Number; // 值不变, 不触发事件
        Assert.Equal(1, eventCount);
    }
}
