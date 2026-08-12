using MineClearance.Core.Models.Records;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="Position"/> 结构的单元测试, 覆盖一维索引转换、相邻位置计算和边界判断
/// </summary>
public sealed class PositionTests
{
    [Theory]
    [InlineData(0, 0, 9, 0)]
    [InlineData(0, 8, 9, 8)]
    [InlineData(8, 0, 9, 72)]
    [InlineData(8, 8, 9, 80)]
    [InlineData(2, 3, 5, 13)]
    public void ToIndex_给定行列和列数_返回行优先的一维索引(int row, int col, int colCount, int expected)
    {
        Assert.Equal(expected, new Position(row, col).ToIndex(colCount));
    }

    [Theory]
    [InlineData(0, 9)]
    [InlineData(80, 9)]
    [InlineData(13, 5)]
    public void FromIndex_与ToIndex互逆_往返结果一致(int index, int colCount)
    {
        var position = Position.FromIndex(index, colCount);
        Assert.Equal(index, position.ToIndex(colCount));
    }

    [Fact]
    public void GetAdjacentPositions_左上角_只返回三个相邻位置()
    {
        Position[] expected = [new(0, 1), new(1, 1), new(1, 0)];
        Assert.Equal(expected, new Position(0, 0).GetAdjacentPositions(9, 9));
    }

    [Fact]
    public void GetAdjacentPositions_上边缘_返回五个相邻位置()
    {
        Position[] expected = [new(0, 4), new(1, 4), new(1, 3), new(1, 2), new(0, 2)];
        Assert.Equal(expected, new Position(0, 3).GetAdjacentPositions(9, 9));
    }

    [Fact]
    public void GetAdjacentPositions_中央_返回八个相邻位置()
    {
        Position[] expected = [
            new(3, 4), new(3, 5), new(4, 5), new(5, 5), new(5, 4), new(5, 3), new(4, 3), new(3, 3)
        ];
        Assert.Equal(expected, new Position(4, 4).GetAdjacentPositions(9, 9));
    }

    [Fact]
    public void GetAdjacentPositions_一乘一棋盘_不返回任何位置()
    {
        Assert.Empty(new Position(0, 0).GetAdjacentPositions(1, 1));
    }

    [Theory]
    [InlineData(9, 9, 81)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 3, 6)]
    public void GetAllPositions_给定棋盘大小_返回全部位置且不重复(int rows, int cols, int expectedCount)
    {
        var positions = Position.GetAllPositions(rows, cols).ToArray();

        Assert.Equal(expectedCount, positions.Length);
        Assert.True(positions.All(position => position.IsInBounds(rows, cols)));
        Assert.Equal(positions.Length, positions.Distinct().Count());
    }

    [Theory]
    [InlineData(0, 0, 9, 9, true)]
    [InlineData(8, 8, 9, 9, true)]
    [InlineData(-1, 0, 9, 9, false)]
    [InlineData(9, 0, 9, 9, false)]
    [InlineData(0, 9, 9, 9, false)]
    public void IsInBounds_给定边界_判断位置是否在范围内(int row, int col, int rowCount, int colCount, bool expected)
    {
        Assert.Equal(expected, new Position(row, col).IsInBounds(rowCount, colCount));
    }

    [Fact]
    public void AddOperator_两个位置相加_返回新的位置()
    {
        Assert.Equal(new Position(3, 5), new Position(1, 2) + new Position(2, 3));
    }

    [Fact]
    public void SubtractOperator_两个位置相减_返回新的位置()
    {
        Assert.Equal(new Position(-1, -1), new Position(1, 2) - new Position(2, 3));
    }
}
