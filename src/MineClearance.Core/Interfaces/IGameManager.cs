using System;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏管理器接口
/// </summary>
public interface IGameManager
{
    /// <summary>
    /// 当游戏变更时触发的事件
    /// </summary>
    event EventHandler<Models.Args.GameChangedEventArgs>? GameChanged;

    /// <summary>
    /// 获取当前游戏实例, 如果当前没有游戏正在进行, 则返回 <see langword="null"/>
    /// </summary>
    IGame? Game { get; }

    /// <summary>
    /// 以指定的配置开始新游戏, 如果当前有游戏正在进行, 则会结束当前游戏并开始新游戏
    /// </summary>
    /// <param name="config">游戏配置</param>
    void StartNewGame(Models.Records.GameConfig config);

    /// <summary>
    /// 从存档数据恢复游戏
    /// </summary>
    /// <param name="data">存档数据</param>
    void RestoreFromSaveData(Models.Records.GameSaveData data);

    /// <summary>
    /// 创建当前游戏的存档数据
    /// </summary>
    /// <returns>存档数据, 如果当前没有游戏正在进行, 则返回 <see langword="null"/></returns>
    Models.Records.GameSaveData? CreateSaveData();
}
