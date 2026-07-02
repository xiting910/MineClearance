using System;

namespace MineClearance.Core.Models.Args;

/// <summary>
/// 地雷场生成事件参数类, 用于表示地雷场生成时的事件数据
/// </summary>
/// <param name="adjacentMineCounts">按行优先顺序排列的周围地雷数量数组</param>
public sealed class MineFieldGeneratedEventArgs(int[] adjacentMineCounts) : EventArgs
{
    /// <summary>
    /// 获取按行优先顺序排列的周围地雷数量数组
    /// </summary>
    public int[] AdjacentMineCounts { get; } = adjacentMineCounts;
}
