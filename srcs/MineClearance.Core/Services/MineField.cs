using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System;
using System.Collections;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 地雷场实现类, 负责地雷的放置、查询和相邻雷数计算
/// </summary>
/// <param name="_mineGenerator">地雷生成器</param>
internal sealed class MineField(IMineGenerator _mineGenerator) : IMineField
{
    /// <summary>
    /// 地雷场尚未生成的异常信息
    /// </summary>
    private const string MineFieldNotGeneratedMessage = "The minefield has not been generated yet.";

    /// <summary>
    /// 行数
    /// </summary>
    private int _rows;

    /// <summary>
    /// 列数
    /// </summary>
    private int _columns;

    /// <summary>
    /// 表示每个位置周围地雷数量的数组, <see cref="Constants.MineValue"/> 表示该位置是地雷, 按行优先顺序排列
    /// </summary>
    private int[]? _adjacentMineCounts;

    /// <inheritdoc/>
    public void Generate(GameConfig config, Position firstClick, int seed)
    {
        // 更新行数和列数
        _rows = config.BoardHeight;
        _columns = config.BoardWidth;
        _adjacentMineCounts = new int[_rows * _columns];

        // 使用地雷生成器生成地雷位置集合, 并遍历所有地雷位置
        var mineSet = _mineGenerator.GenerateMines(config, firstClick, seed).ToHashSet();
        foreach (var p1 in mineSet)
        {
            // 标记该位置为地雷
            _adjacentMineCounts[p1.ToIndex(_columns)] = Constants.MineValue;

            // 遍历该地雷位置的所有相邻位置
            foreach (var p2 in p1.GetAdjacentPositions(_rows, _columns).Where(p3 => !mineSet.Contains(p3)))
            {
                // 增加相邻位置的地雷计数
                _adjacentMineCounts[p2.ToIndex(_columns)]++;
            }
        }
    }

    /// <inheritdoc/>
    public void Apply(GameConfig config, BitArray mineMap)
    {
        // 更新行数和列数
        _rows = config.BoardHeight;
        _columns = config.BoardWidth;
        _adjacentMineCounts = new int[_rows * _columns];

        // 遍历所有位置, 根据位图表示设置地雷和相邻地雷计数
        foreach (var position in Position.GetAllPositions(_rows, _columns))
        {
            // 获取该位置的一维索引
            var index = position.ToIndex(_columns);

            // 如果该位置是地雷
            if (mineMap[index])
            {
                // 标记该位置为地雷
                _adjacentMineCounts[index] = Constants.MineValue;

                // 遍历该地雷位置的所有相邻位置
                foreach (var adjacentIndex in position.GetAdjacentPositions(_rows, _columns)
                    .Select(adjacentPos => adjacentPos.ToIndex(_columns))
                    .Where(adjacentIndex => !mineMap[adjacentIndex]))
                {
                    // 增加相邻位置的地雷计数
                    _adjacentMineCounts[adjacentIndex]++;
                }
            }
        }
    }

    /// <inheritdoc/>
    public BitArray GetMineMap()
    {
        // 如果地雷场尚未生成, 则抛出异常
        if (_adjacentMineCounts is null)
        {
            throw new InvalidOperationException(MineFieldNotGeneratedMessage);
        }

        // 创建一个新的 BitArray, 用于表示地雷场的位图, 其中每一位表示一个格子是否是地雷
        var mineMap = new BitArray(_adjacentMineCounts.Length);

        // 遍历所有位置, 将地雷位置标记为 true, 非地雷位置标记为 false
        for (var i = 0; i < _adjacentMineCounts.Length; i++)
        {
            mineMap[i] = _adjacentMineCounts[i] == Constants.MineValue;
        }

        // 返回地雷场的位图表示
        return mineMap;
    }

    /// <inheritdoc/>
    public bool IsMine(Position position)
    {
        // 如果地雷场尚未生成, 则抛出异常
        if (_adjacentMineCounts is null)
        {
            throw new InvalidOperationException(MineFieldNotGeneratedMessage);
        }

        // 返回该位置是否是地雷
        return _adjacentMineCounts[position.ToIndex(_columns)] == Constants.MineValue;
    }

    /// <inheritdoc/>
    public int GetAdjacentMineCount(Position position)
    {
        // 如果地雷场尚未生成, 则抛出异常
        if (_adjacentMineCounts is null)
        {
            throw new InvalidOperationException(MineFieldNotGeneratedMessage);
        }

        // 获取该位置周围的地雷数量
        var count = _adjacentMineCounts[position.ToIndex(_columns)];

        // 如果该位置是地雷, 则抛出异常, 否则返回周围地雷数量
        return count == Constants.MineValue ? throw new InvalidOperationException(
                $"The position {position} is a mine, cannot get adjacent mine count."
            ) : count;
    }
}
