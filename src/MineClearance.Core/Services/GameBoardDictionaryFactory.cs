using MineClearance.Core.Interfaces;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏棋盘字典工厂实现类, 负责创建棋盘字典实例
/// </summary>
internal sealed class GameBoardDictionaryFactory : IGameBoardDictionaryFactory
{
    /// <inheritdoc/>
    public IGameBoardDictionary CreateGameBoardDictionary(int rows, int columns, int[] adjacentMineCounts)
    {
        return new GameBoardDictionary(rows, columns, adjacentMineCounts);
    }
}
