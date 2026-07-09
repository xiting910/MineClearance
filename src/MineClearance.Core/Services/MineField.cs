using System;
using System.Collections;

namespace MineClearance.Core.Services;

/// <summary>
/// 地雷场实现类, 负责地雷的放置、查询和相邻雷数计算
/// </summary>
/// <param name="mineGenerator">地雷生成器</param>
internal sealed class MineField(Interfaces.IMineGenerator mineGenerator) : Interfaces.IMineField
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

    /// <summary>
    /// 地雷生成器字段
    /// </summary>
    private readonly Interfaces.IMineGenerator _mineGenerator = mineGenerator;

    /// <inheritdoc/>
    public int[] Generate(Models.Records.GameConfig config, Models.Records.Position firstClick, int seed)
    {
        // 更新行数和列数
        _rows = config.BoardHeight;
        _columns = config.BoardWidth;
        _adjacentMineCounts = new int[_rows * _columns];

        // 使用地雷生成器生成地雷位置集合, 并遍历所有地雷位置
        foreach (var minePosition in _mineGenerator.GenerateMines(config, firstClick, seed))
        {
            // 标记该位置为地雷
            _adjacentMineCounts[minePosition.ToIndex(_columns)] = Constants.MineValue;

            // 遍历该地雷位置的所有相邻位置
            foreach (var adjacentPosition in minePosition.GetAdjacentPositions(_rows, _columns))
            {
                // 获取相邻位置的一维索引
                var index = adjacentPosition.ToIndex(_columns);

                // 如果相邻位置不是地雷, 则增加其地雷计数
                if (_adjacentMineCounts[index] != Constants.MineValue)
                {
                    _adjacentMineCounts[index]++;
                }
            }
        }

        // 返回表示每个位置周围地雷数量的数组, -1 表示该位置是地雷
        return _adjacentMineCounts;
    }

    /// <inheritdoc/>
    public int[] Generate(Models.Records.GameConfig config, BitArray mineMap)
    {
        // 更新行数和列数
        _rows = config.BoardHeight;
        _columns = config.BoardWidth;
        _adjacentMineCounts = new int[_rows * _columns];

        // 遍历所有位置, 根据位图表示设置地雷和相邻地雷计数
        foreach (var position in Models.Records.Position.GetAllPositions(_rows, _columns))
        {
            // 获取该位置的一维索引
            var index = position.ToIndex(_columns);

            // 如果该位置是地雷
            if (mineMap[index])
            {
                // 标记该位置为地雷
                _adjacentMineCounts[index] = Constants.MineValue;

                // 遍历该地雷位置的所有相邻位置
                foreach (var adjacentPosition in position.GetAdjacentPositions(_rows, _columns))
                {
                    // 获取相邻位置的一维索引
                    var adjacentIndex = adjacentPosition.ToIndex(_columns);

                    // 如果相邻位置不是地雷, 则增加其地雷计数
                    if (_adjacentMineCounts[adjacentIndex] != Constants.MineValue)
                    {
                        _adjacentMineCounts[adjacentIndex]++;
                    }
                }
            }
        }

        // 返回表示每个位置周围地雷数量的数组, -1 表示该位置是地雷
        return _adjacentMineCounts;
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
    public bool IsMine(Models.Records.Position position)
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
    public int GetAdjacentMineCount(Models.Records.Position position)
    {
        // 如果地雷场尚未生成, 则抛出异常
        if (_adjacentMineCounts is null)
        {
            throw new InvalidOperationException(MineFieldNotGeneratedMessage);
        }

        // 返回该位置周围的地雷数量
        return _adjacentMineCounts[position.ToIndex(_columns)];
    }
}
