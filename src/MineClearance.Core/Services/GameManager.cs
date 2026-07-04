using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;

namespace MineClearance.Core.Services;

// TODO: 添加游戏胜利或失败后自动将游戏结果保存到游戏数据存储库的功能
/// <summary>
/// 游戏管理器实现类, 负责游戏实例的创建、销毁和存档管理
/// </summary>
/// <param name="gameFactory">游戏工厂</param>
/// <param name="dataRepository">游戏数据存储库</param>
internal sealed class GameManager(
    Interfaces.IGameFactory gameFactory,
    Interfaces.IGameDataRepository dataRepository) : Interfaces.IGameManager
{
    /// <summary>
    /// 游戏工厂字段
    /// </summary>
    private readonly Interfaces.IGameFactory _gameFactory = gameFactory;

    /// <summary>
    /// 游戏数据存储库字段
    /// </summary>
    private readonly Interfaces.IGameDataRepository _dataRepository = dataRepository;

    /// <inheritdoc/>
    public event EventHandler<Models.GameChangedEventArgs>? GameChanged;

    /// <inheritdoc/>
    public Interfaces.IGame? Game
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                GameChanged?.Invoke(this, new(field));
            }
        }
    }

    /// <inheritdoc/>
    public void StartNewGame(GameDifficulty difficulty)
    {
        // 如果难度为自定义, 则不允许使用此方法创建游戏, 应该使用 StartNewGame(GameConfig config, int? seed) 方法
        if (difficulty is GameDifficulty.Custom)
        {
            throw new ArgumentException("Cannot start a new game with custom difficulty using this method. Use StartNewGame(GameConfig config, int? seed) instead.", nameof(difficulty));
        }

        // 释放当前游戏实例
        Game?.Dispose();

        // 使用游戏工厂创建一个新的游戏实例
        Game = _gameFactory.CreateGame(difficulty);
    }

    /// <inheritdoc/>
    public void StartNewGame(GameConfig config, int? seed = null)
    {
        // 释放当前游戏实例
        Game?.Dispose();

        // 使用游戏工厂创建一个新的游戏实例
        Game = _gameFactory.CreateGame(config, seed);
    }

    /// <inheritdoc/>
    public async Task RestoreFromSaveDataAsync()
    {
        // 释放当前游戏实例
        Game?.Dispose();

        // 从游戏数据存储库获取存档数据
        var saveData = await _dataRepository.GetGameSaveDataAsync().ConfigureAwait(false);

        // 存档数据应该存在, 因为不存在存档数据的情况下, UI 不应该显示调用此方法的选项
        Debug.Assert(saveData is not null, "Save data should exist when restoring a game.");

        // 使用游戏工厂创建一个新的游戏实例, 并传入存档数据
        Game = _gameFactory.CreateGame(saveData);
    }

    /// <inheritdoc/>
    public async Task<bool> SaveAndExitAsync()
    {
        // 如果当前没有游戏正在进行, 则不需要保存
        if (Game is null) { return true; }

        // 获取当前游戏的存档数据
        var saveData = Game.GetSaveData();

        // 将存档数据保存到游戏数据存储库
        var saveResult = await _dataRepository.SaveGameSaveDataAsync(saveData).ConfigureAwait(false);

        // 释放当前游戏实例
        Game.Dispose();

        // 将当前游戏实例设置为 null, 表示没有游戏正在进行
        Game = null;

        // 返回保存结果
        return saveResult;
    }
}
