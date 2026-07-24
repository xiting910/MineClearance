namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏棋盘字典工厂接口, 用于创建游戏棋盘字典实例
/// </summary>
internal interface IGameBoardDictionaryFactory
{
    /// <summary>
    /// 创建游戏棋盘字典实例
    /// </summary>
    /// <param name="rows">棋盘行数</param>
    /// <param name="columns">棋盘列数</param>
    /// <param name="adjacentMineCounts">相邻地雷数量数组</param>
    /// <returns>游戏棋盘字典实例</returns>
    IGameBoardDictionary CreateGameBoardDictionary(int rows, int columns, int[] adjacentMineCounts);
}
