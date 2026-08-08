using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏管理器接口
/// </summary>
public interface IGameManager : INotifyPropertyChanging, INotifyPropertyChanged
{
    /// <summary>
    /// 获取当前游戏实例, 如果当前没有游戏正在进行, 则返回 <see langword="null"/>
    /// </summary>
    IGame? Game { get; }

    /// <summary>
    /// 开始非自定义难度的游戏, 如果当前有游戏正在进行, 则会结束当前游戏并开始新游戏
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <exception cref="ArgumentException">当难度为自定义时抛出异常</exception>
    void StartNewGame(GameDifficulty difficulty);

    /// <summary>
    /// 开始自定义难度的新游戏, 如果当前有游戏正在进行, 则会结束当前游戏并开始新游戏
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="seed">随机种子</param>
    /// <exception cref="ArgumentException">当配置无效时抛出异常</exception>
    void StartNewGame(GameConfig config, int? seed = null);

    /// <summary>
    /// 从存档数据恢复游戏
    /// </summary>
    void RestoreFromSaveData();

    /// <summary>
    /// 重新开始当前游戏
    /// </summary>
    void RestartCurrentGame();

    /// <summary>
    /// 不保存当前游戏并退出
    /// </summary>
    void ExitWithoutSaving();

    /// <summary>
    /// 保存并退出当前游戏
    /// </summary>
    /// <returns><see langword="true"/> 如果保存成功, <see langword="false"/> 如果保存失败</returns>
    Task<bool> SaveAndExitAsync();
}
