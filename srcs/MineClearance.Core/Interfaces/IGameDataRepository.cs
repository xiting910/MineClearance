using MineClearance.Core.Models.Records;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏数据仓储接口, 用于管理游戏存档和游戏结果记录
/// </summary>
public interface IGameDataRepository
{
    /// <summary>
    /// 获取是否存在未完成的游戏存档
    /// </summary>
    bool HasGameSaveData { get; }

    /// <summary>
    /// 获取存档数据, 如果没有存档数据, 则返回 <see langword="null"/>
    /// </summary>
    /// <returns>存档数据, 如果没有存档数据, 则返回 <see langword="null"/></returns>
    Task<GameSaveData?> GetGameSaveDataAsync();

    /// <summary>
    /// 保存存档数据, 会覆盖之前的存档数据, 如果传入 <see langword="null"/> 则会删除存档数据
    /// </summary>
    /// <param name="data">存档数据</param>
    /// <returns><see langword="true"/> 表示保存成功, <see langword="false"/> 表示保存失败</returns>
    Task<bool> SaveGameSaveDataAsync(GameSaveData? data);

    /// <summary>
    /// 获取游戏结果记录列表, 按照时间倒序排列
    /// </summary>
    /// <returns>游戏结果记录列表</returns>
    IAsyncEnumerable<GameResult> GetGameResultsAsync();

    /// <summary>
    /// 添加一条游戏结果记录
    /// </summary>
    /// <param name="result">游戏结果记录</param>
    /// <returns><see langword="true"/> 表示添加成功, <see langword="false"/> 表示添加失败</returns>
    Task<bool> AddGameResultAsync(GameResult result);

    /// <summary>
    /// 删除一条游戏结果记录
    /// </summary>
    /// <param name="result">游戏结果记录</param>
    /// <returns><see langword="true"/> 表示删除成功, <see langword="false"/> 表示删除失败</returns>
    Task<bool> DeleteGameResultAsync(GameResult result);

    /// <summary>
    /// 清空所有游戏结果记录
    /// </summary>
    /// <returns><see langword="true"/> 表示清空成功, <see langword="false"/> 表示清空失败</returns>
    Task<bool> ClearGameResultsAsync();
}
