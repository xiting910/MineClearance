using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏工厂接口, 用于创建游戏相关的实例
/// </summary>
internal interface IGameFactory
{
    /// <summary>
    /// 创建非自定义难度的游戏实例
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <returns>游戏实例</returns>
    IGame CreateGame(GameDifficulty difficulty);

    /// <summary>
    /// 创建自定义难度的游戏实例
    /// </summary>
    /// <param name="config">自定义游戏配置</param>
    /// <param name="seed">随机种子, 可选参数, 用于生成固定的地雷布局</param>
    /// <returns>游戏实例</returns>
    IGame CreateGame(GameConfig config, int? seed = null);

    /// <summary>
    /// 从游戏存档数据创建游戏实例
    /// </summary>
    /// <param name="saveData">游戏存档数据</param>
    /// <returns>游戏实例</returns>
    IGame CreateGame(GameSaveData saveData);
}
