namespace MineClearance.Models;

/// <summary>
/// 位置类, 用于表示扫雷游戏中的位置
/// </summary>
/// <param name="row">行</param>
/// <param name="col">列</param>
internal sealed class Position(int row, int col) : IEquatable<Position>
{
    /// <summary>
    /// 无效位置
    /// </summary>
    public static Position Invalid { get; } = new(-1, -1);

    /// <summary>
    /// 行
    /// </summary>
    public int Row { get; } = row;

    /// <summary>
    /// 列
    /// </summary>
    public int Col { get; } = col;

    /// <summary>
    /// 重写+运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>返回新的位置</returns>
    public static Position operator +(Position left, Position right) => new(left.Row + right.Row, left.Col + right.Col);

    /// <summary>
    /// 重写==运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>返回是否相等</returns>
    public static bool operator ==(Position left, Position right) => left.Row == right.Row && left.Col == right.Col;

    /// <summary>
    /// 重写!=运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>返回是否不相等</returns>
    public static bool operator !=(Position left, Position right) => !(left == right);

    /// <summary>
    /// 与其他位置比较
    /// </summary>
    /// <param name="other">其他位置</param>
    /// <returns>返回是否相等</returns>
    public bool Equals(Position? other) => other is not null && this == other;

    /// <summary>
    /// 重写Equals方法
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>返回是否相等</returns>
    public override bool Equals(object? obj) => obj is Position other && this == other;

    /// <summary>
    /// 重写GetHashCode方法
    /// </summary>
    /// <returns>返回对象的哈希代码</returns>
    public override int GetHashCode() => HashCode.Combine(Row, Col);
}