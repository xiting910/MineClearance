namespace MineClearance.Models.Enums;

/// <summary>
/// 格子类型枚举
/// </summary>
internal enum GridType
{
    /// <summary>
    /// 未打开的格子
    /// </summary>
    Unopened,

    /// <summary>
    /// 空白格子, 表示没有地雷且周围没有地雷
    /// </summary>
    Empty,

    /// <summary>
    /// 数字格子, 表示周围地雷的数量
    /// </summary>
    Number,

    /// <summary>
    /// 警告数字格子, 表示玩家在其周围插旗的数量超过了实际地雷数量
    /// </summary>
    WarningNumber,

    /// <summary>
    /// 标记的地雷
    /// </summary>
    Flagged,

    /// <summary>
    /// 地雷
    /// </summary>
    Mine
}