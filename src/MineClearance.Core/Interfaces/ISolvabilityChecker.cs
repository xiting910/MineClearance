using MineClearance.Core.Models.Records;
using System.Collections.Generic;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 可解性检查器接口, 只在 Core 层使用, 用于检查给定的地雷布局是否可解
/// </summary>
internal interface ISolvabilityChecker
{
    /// <summary>
    /// 检查给定的地雷布局是否可解
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="firstClick">首次点击位置</param>
    /// <param name="mines">地雷位置集合</param>
    /// <returns><see langword="true"/> 如果可解, 否则 <see langword="false"/></returns>
    bool IsSolvable(GameConfig config, Position firstClick, IEnumerable<Position> mines);
}
