using System.Collections.Generic;
using System.Collections.Immutable;

namespace MineClearance.Core.Models.Records;

/// <summary>
/// 位置类, 用于表示扫雷游戏中的位置
/// </summary>
/// <param name="Row">行</param>
/// <param name="Col">列</param>
public readonly record struct Position(int Row, int Col)
{
    /// <summary>
    /// 获取方向的位置偏移量数组, 按顺时针顺序排列
    /// </summary>
    public static ImmutableArray<Position> DirectionOffsets { get; } = ImmutableArray.Create<Position>(
        new(-1, 0), // 上
        new(-1, 1), // 右上
        new(0, 1),  // 右
        new(1, 1),  // 右下
        new(1, 0),  // 下
        new(1, -1), // 左下
        new(0, -1), // 左
        new(-1, -1) // 左上
    );

    /// <summary>
    /// 判断当前位置是否在给定的边界内
    /// </summary>
    /// <param name="rowCount">行数</param>
    /// <param name="colCount">列数</param>
    /// <returns><see langword="true"/> 如果当前位置在边界内, 否则 <see langword="false"/></returns>
    public bool IsInBounds(int rowCount, int colCount)
    {
        return Row >= 0 && Row < rowCount && Col >= 0 && Col < colCount;
    }

    /// <summary>
    /// 将位置转换为一维索引, 按行优先顺序排列
    /// </summary>
    /// <param name="colCount">列数</param>
    /// <returns>一维索引</returns>
    public int ToIndex(int colCount)
    {
        return (Row * colCount) + Col;
    }

    /// <summary>
    /// 获取当前位置的所有相邻位置, 包括对角线方向
    /// </summary>
    /// <param name="rowCount">行数</param>
    /// <param name="colCount">列数</param>
    /// <returns>所有相邻位置的集合</returns>
    public IEnumerable<Position> GetAdjacentPositions(int rowCount, int colCount)
    {
        // 遍历所有方向偏移量
        foreach (var offset in DirectionOffsets)
        {
            // 计算相邻位置
            var adjacentPosition = this + offset;

            // 判断相邻位置是否在边界内, 如果在边界内则返回该位置
            if (adjacentPosition.IsInBounds(rowCount, colCount))
            {
                yield return adjacentPosition;
            }
        }
    }

    /// <summary>
    /// 获取指定行数和列数的所有位置
    /// </summary>
    /// <param name="rowCount">行数</param>
    /// <param name="colCount">列数</param>
    /// <returns>所有位置的集合</returns>
    public static IEnumerable<Position> GetAllPositions(int rowCount, int colCount)
    {
        for (var row = 0; row < rowCount; row++)
        {
            for (var col = 0; col < colCount; col++)
            {
                yield return new(row, col);
            }
        }
    }

    /// <summary>
    /// 重写 <see cref="operator +"/> 运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>新的位置</returns>
    public static Position operator +(Position left, Position right)
    {
        return new(left.Row + right.Row, left.Col + right.Col);
    }

    /// <summary>
    /// 重写 <see cref="operator -"/> 运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>新的位置</returns>
    public static Position operator -(Position left, Position right)
    {
        return new(left.Row - right.Row, left.Col - right.Col);
    }

    /// <summary>
    /// 重写 <see cref="object.ToString()"/> 方法
    /// </summary>
    /// <returns>位置的字符串表示</returns>
    public override string ToString()
    {
        return $"({Row}, {Col})";
    }
}
