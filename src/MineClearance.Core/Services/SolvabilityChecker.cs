using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System.Collections.Generic;

namespace MineClearance.Core.Services;

/// <summary>
/// 可解性检查器实现类, 只在 Core 层使用, 用于检查给定的地雷布局是否可解
/// </summary>
internal sealed class SolvabilityChecker : ISolvabilityChecker
{
    /// <inheritdoc/>
    public bool IsSolvable(GameConfig config, Position firstClick, IEnumerable<Position> mines)
    {
        // TODO: 完成可解性检查逻辑, 目前暂时返回 true, 表示所有布局都可解
        return true;
    }
}
