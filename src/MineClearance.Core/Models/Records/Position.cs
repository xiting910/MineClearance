namespace MineClearance.Core.Models.Records;

/// <summary>
/// 位置类, 用于表示扫雷游戏中的位置
/// </summary>
/// <param name="Row">行</param>
/// <param name="Col">列</param>
public readonly record struct Position(int Row, int Col)
{
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
