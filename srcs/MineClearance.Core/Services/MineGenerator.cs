using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 地雷生成器实现类, 负责生成地雷位置集合, 确保首次点击位置不是地雷
/// </summary>
internal sealed class MineGenerator : IMineGenerator
{
    /// <inheritdoc/>
    public IEnumerable<Position> GenerateMines(GameConfig config, Position firstClick, int seed)
    {
        var allPositions = Position.GetAllPositions(config.BoardHeight, config.BoardWidth)
            .Where(pos => pos != firstClick)
            .ToArray();

        var neighbors = firstClick.GetAdjacentPositions(config.BoardHeight, config.BoardWidth);
        var excludeNeighbors = allPositions.Except(neighbors).ToArray();

        var available = excludeNeighbors.Length >= config.MineCount ? excludeNeighbors : allPositions;
        var random = new Random(seed);
        random.Shuffle(available);
        return available.Take(config.MineCount);
    }
}
