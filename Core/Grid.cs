using MineClearance.Models.Enums;

namespace MineClearance.Core;

/// <summary>
/// 表示扫雷游戏的棋盘格子
/// </summary>
internal sealed class Grid
{
    /// <summary>
    /// 格子类型, 默认是未打开的格子
    /// </summary>
    public GridType Type { get; set; } = GridType.Unopened;

    /// <summary>
    /// 周围地雷的数量, -1表示不是数字格子
    /// </summary>
    public int SurroundingMines { get; set; } = -1;
}