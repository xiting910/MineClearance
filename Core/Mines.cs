using MineClearance.Models;

namespace MineClearance.Core;

/// <summary>
/// 表示扫雷游戏中的地雷, 记录地雷的位置
/// </summary>
/// <param name="width">棋盘的宽度</param>
/// <param name="height">棋盘的高度</param>
/// <param name="mineCount">地雷的数量</param>
internal sealed class Mines(int width, int height, int mineCount)
{
    /// <summary>
    /// 棋盘的宽度
    /// </summary>
    private readonly int _width = width;

    /// <summary>
    /// 棋盘的高度
    /// </summary>
    private readonly int _height = height;

    /// <summary>
    /// 地雷的数量
    /// </summary>
    private readonly int _mineCount = mineCount;

    /// <summary>
    /// 二维数组, 记录每个坐标周围地雷的数量, -1表示该坐标是地雷
    /// </summary>
    public int[,] MineGrid { get; } = new int[height, width];

    /// <summary>
    /// 随机生成地雷位置, 尽可能确保首次点击位置和其周围格子不包含地雷
    /// </summary>
    /// <param name="firstClickPos">首次点击的位置</param>
    public void GenerateMines(Position firstClickPos)
    {
        // 首次点击位置的索引
        var firstClickIndex = (firstClickPos.Row * _width) + firstClickPos.Col;

        // 除了首次点击位置外的所有位置
        var allPositions = Enumerable.Range(0, _width * _height).Where(pos => pos != firstClickIndex);

        // 安全位置的索引(与首次点击位置相邻的格子)
        var safePositions = allPositions.Where(pos => Math.Abs((pos / _width) - firstClickPos.Row) <= 1 && Math.Abs((pos % _width) - firstClickPos.Col) <= 1).ToArray();

        // 可放置地雷的位置(排除安全位置)
        var availablePositions = allPositions.Except(safePositions).ToArray();

        // 如果可用位置可以放置所有地雷
        if (_mineCount <= availablePositions.Length)
        {
            Random.Shared.Shuffle(availablePositions);
            var minePositions = availablePositions.Take(_mineCount);
            PlaceMines(minePositions);
        }
        // 如果可用位置不足以放置所有地雷
        else
        {
            Random.Shared.Shuffle(safePositions);
            var additionalMinesNeeded = _mineCount - availablePositions.Length;
            var additionalMinePositions = safePositions.Take(additionalMinesNeeded);
            var allMinePositions = availablePositions.Concat(additionalMinePositions);
            PlaceMines(allMinePositions);
        }
    }

    /// <summary>
    /// 根据地雷位置索引枚举更新 MineGrid 数组
    /// </summary>
    /// <param name="minePositions">地雷位置的索引集合</param>
    private void PlaceMines(IEnumerable<int> minePositions)
    {
        // 在 MineGrid 中标记地雷位置
        foreach (var pos in minePositions)
        {
            var row = pos / _width;
            var column = pos % _width;
            MineGrid[row, column] = -1;
        }

        // 计算不是地雷的格子周围的地雷数量
        for (var row = 0; row < _height; ++row)
        {
            for (var column = 0; column < _width; ++column)
            {
                if (MineGrid[row, column] == -1)
                {
                    continue;
                }

                MineGrid[row, column] = CountAdjacentMines(row, column);
            }
        }
    }

    /// <summary>
    /// 计算指定格子周围的地雷数量
    /// </summary>
    /// <param name="row">格子的行索引</param>
    /// <param name="column">格子的列索引</param>
    /// <returns>周围地雷的数量</returns>
    private int CountAdjacentMines(int row, int column)
    {
        var mineCount = 0;

        for (var r = row - 1; r <= row + 1; ++r)
        {
            for (var c = column - 1; c <= column + 1; ++c)
            {
                if (r < 0 || r >= _height || c < 0 || c >= _width)
                {
                    continue;
                }

                if (MineGrid[r, c] == -1)
                {
                    mineCount++;
                }
            }
        }

        return mineCount;
    }
}