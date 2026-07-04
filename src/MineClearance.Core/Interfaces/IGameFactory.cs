namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏工厂接口, 用于创建游戏相关的实例
/// </summary>
internal interface IGameFactory
{
    /// <summary>
    /// 从游戏配置创建游戏实例
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <returns>游戏实例</returns>
    IGame CreateGame(Models.Records.GameConfig config);

    /// <summary>
    /// 从游戏存档数据创建游戏实例
    /// </summary>
    /// <param name="saveData">游戏存档数据</param>
    /// <returns>游戏实例</returns>
    IGame CreateGame(Models.Records.GameSaveData saveData);
}
