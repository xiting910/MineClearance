using System.Collections.Generic;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏棋盘接口, 负责与玩家交互的格子集合, 不包含真实的地雷信息
/// </summary>
public interface IGameBoard
{
    /// <summary>
    /// 获取棋盘行数
    /// </summary>
    int Rows { get; }

    /// <summary>
    /// 获取棋盘列数
    /// </summary>
    int Columns { get; }

    /// <summary>
    /// 获取当前已打开的格子数量
    /// </summary>
    int OpenedCount { get; }

    /// <summary>
    /// 获取当前已插旗的数量
    /// </summary>
    int FlagCount { get; }

    /// <summary>
    /// 获取当前已标记问号的数量
    /// </summary>
    int QuestionCount { get; }

    /// <summary>
    /// 获取格子集合的扁平化列表, 按行优先顺序排列
    /// </summary>
    IReadOnlyList<Models.Cell> Cells { get; }

    /// <summary>
    /// 按坐标索引获取格子实例
    /// </summary>
    /// <param name="position">格子坐标</param>
    /// <returns>格子实例</returns>
    Models.Cell this[Models.Records.Position position] { get; }

    /// <summary>
    /// 在地雷生成完成后, 设置所有格子的周围地雷数量, 以便在打开格子时显示正确的数字
    /// </summary>
    /// <param name="adjacentMineCounts">按行优先顺序排列的周围地雷数量数组</param>
    void SetAdjacentMineCounts(int[] adjacentMineCounts);
}
