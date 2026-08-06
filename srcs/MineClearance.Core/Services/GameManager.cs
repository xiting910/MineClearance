using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models;
using MineClearance.Core.Models.Records;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏管理器实现类, 负责游戏实例的创建、销毁和存档管理
/// </summary>
/// <param name="gameFactory">游戏工厂</param>
/// <param name="dataRepository">游戏数据存储库</param>
internal sealed class GameManager(
    IGameFactory gameFactory,
    IGameDataRepository dataRepository) : IGameManager
{
    /// <summary>
    /// 游戏工厂字段
    /// </summary>
    private readonly IGameFactory _gameFactory = gameFactory;

    /// <summary>
    /// 游戏数据存储库字段
    /// </summary>
    private readonly IGameDataRepository _dataRepository = dataRepository;

    /// <inheritdoc/>
    public event EventHandler<GameChangedEventArgs>? GameChanged;

    /// <inheritdoc/>
    public IGame? Game
    {
        get;
        private set
        {
            if (field != value)
            {
                field?.PropertyChanged -= OnGamePropertyChanged;
                field = value;
                field?.PropertyChanged += OnGamePropertyChanged;
                GameChanged?.Invoke(this, new(field));
            }
        }
    }

    /// <inheritdoc/>
    public void RestartCurrentGame()
    {
        // 如果当前没有游戏正在进行, 则不需要重新开始
        if (Game is null) { return; }

        // 获取当前游戏的难度
        var difficulty = Game.Difficulty;

        // 根据当前游戏的难度重新开始游戏
        if (difficulty is GameDifficulty.Custom)
        {
            // 如果当前游戏是自定义难度, 则使用当前游戏的配置和种子重新开始游戏
            StartNewGame(Game.Config, Game.Seed);
        }
        else
        {
            // 如果当前游戏是非自定义难度, 则使用当前游戏的难度重新开始游戏
            StartNewGame(difficulty);
        }
    }

    /// <inheritdoc/>
    public void StartNewGame(GameDifficulty difficulty)
    {
        // 如果难度为自定义, 则不允许使用此方法创建游戏, 应该使用 StartNewGame(GameConfig config, int? seed) 方法
        if (difficulty is GameDifficulty.Custom)
        {
            throw new ArgumentException(Constants.CustomDifficultyMissingInfoMessage, nameof(difficulty));
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

    /// <inheritdoc/>
    public void ExitWithoutSaving()
    {
        // 释放当前游戏实例
        Game?.Dispose();

        // 将当前游戏实例设置为 null, 表示没有游戏正在进行
        Game = null;
    }

    /// <summary>
    /// 游戏实例属性变更事件处理方法, 当游戏实例的属性发生变更时触发
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private async void OnGamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 如果事件发送者不是游戏实例, 则忽略此事件
        if (sender is not IGame game) { return; }

        // 游戏结果属性变更时, 将游戏结果保存到游戏数据存储库
        if (e.PropertyName is nameof(IGame.Result))
        {
            // 此时游戏结果应该已经被设置为非 null, 因为游戏结果属性在游戏结束时才会被设置
            Debug.Assert(game.Result is not null, "Game result should not be null when the game ends.");

            // 将游戏结果保存到游戏数据存储库
            if (!await _dataRepository.AddGameResultAsync(game.Result).ConfigureAwait(false))
            {
                // 如果保存失败, 则记录错误日志
                Debug.WriteLine("Failed to save game result to the data repository.");
            }
        }
    }
}
