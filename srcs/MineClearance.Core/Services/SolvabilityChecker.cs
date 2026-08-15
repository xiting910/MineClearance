using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System.Collections.Generic;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 可解性检查器实现类, 只在 Core 层使用, 用于检查给定的地雷布局是否可解
/// </summary>
internal sealed class SolvabilityChecker : ISolvabilityChecker
{
    /// <summary>
    /// 确保完全可解性的地雷密度阈值
    /// </summary>
    private const double SolvableDensityThreshold = 0.25;

    /// <summary>
    /// 确保每个地雷周围至少有一个安全格子的地雷密度阈值
    /// </summary>
    private const double SafeNeighborDensityThreshold = 0.8;

    /// <inheritdoc/>
    public bool IsSolvable(GameConfig config, Position firstClick, IEnumerable<Position> mines)
    {
        return config.MineDensity switch
        {
            < SolvableDensityThreshold => IsSolvableCore(config, firstClick, mines),
            < SafeNeighborDensityThreshold => HasSafeNeighbor(config, mines),
            _ => true
        };
    }

    /// <summary>
    /// 检查给定的地雷布局能否在不进行任何猜测的情况下完成游戏
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="firstClick">首次点击位置</param>
    /// <param name="mines">地雷位置集合</param>
    /// <returns><see langword="true"/> 表示无需猜测即可完成, <see langword="false"/> 表示需要猜测</returns>
    private bool IsSolvableCore(GameConfig config, Position firstClick, IEnumerable<Position> mines)
    {
        // TODO: 完成可解性检查逻辑, 目前暂时返回 true, 表示所有布局都可解
        return true;
    }

    /// <summary>
    /// 判断是否满足每个地雷周围至少有一个安全格子的条件
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="mines">地雷位置集合</param>
    /// <returns><see langword="true"/> 表示满足条件, <see langword="false"/> 表示不满足条件</returns>
    private static bool HasSafeNeighbor(GameConfig config, IEnumerable<Position> mines)
    {
        var mineSet = mines.ToHashSet();
        foreach (var mine in mineSet)
        {
            var neighbors = mine.GetAdjacentPositions(config.BoardHeight, config.BoardWidth);
            if (neighbors.All(mineSet.Contains))
            {
                return false;
            }
        }
        return true;
    }
}
