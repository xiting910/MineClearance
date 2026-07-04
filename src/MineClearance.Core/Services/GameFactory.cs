using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏工厂实现类, 负责组装游戏对象图
/// </summary>
/// <param name="serviceScopeFactory">服务作用域工厂</param>
/// <param name="boardFactory">游戏棋盘字典工厂</param>
internal sealed class GameFactory(
    IServiceScopeFactory serviceScopeFactory,
    Interfaces.IGameBoardDictionaryFactory boardFactory) : Interfaces.IGameFactory
{
    /// <summary>
    /// 服务作用域工厂
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    /// <summary>
    /// 游戏棋盘字典工厂
    /// </summary>
    private readonly Interfaces.IGameBoardDictionaryFactory _boardFactory = boardFactory;

    /// <inheritdoc/>
    public Interfaces.IGame CreateGame(Enums.GameDifficulty difficulty)
    {
        // 如果难度为自定义, 则该方法不应被调用
        Debug.Assert(difficulty is not Enums.GameDifficulty.Custom, "Cannot create a game with custom difficulty using this method. Use CreateGame(GameConfig config, int? seed) instead.");

        // 创建一个新的服务作用域, 用于管理依赖注入的生命周期
        var serviceScope = _serviceScopeFactory.CreateScope();

        // 从服务作用域中获取内部地雷场实例
        var mineField = serviceScope.ServiceProvider.GetRequiredService<Interfaces.IMineField>();

        // 从服务作用域中获取游戏计时器实例
        var timer = serviceScope.ServiceProvider.GetRequiredService<Interfaces.IGameTimer>();

        // 获取游戏配置
        var config = Models.Records.GameConfig.FromDifficulty(difficulty);

        // 生成一个随机种子
        var seed = Random.Shared.Next();

        // 创建并返回一个新的游戏实例
        return new Game(serviceScope, _boardFactory, mineField, timer, difficulty, config, seed);
    }

    /// <inheritdoc/>
    public Interfaces.IGame CreateGame(Models.Records.GameConfig config, int? seed = null)
    {
        // 创建一个新的服务作用域, 用于管理依赖注入的生命周期
        var serviceScope = _serviceScopeFactory.CreateScope();

        // 从服务作用域中获取内部地雷场实例
        var mineField = serviceScope.ServiceProvider.GetRequiredService<Interfaces.IMineField>();

        // 从服务作用域中获取游戏计时器实例
        var timer = serviceScope.ServiceProvider.GetRequiredService<Interfaces.IGameTimer>();

        // 如果未提供种子, 则生成一个随机种子
        seed ??= Random.Shared.Next();

        // 创建并返回一个新的游戏实例
        return new Game(serviceScope, _boardFactory, mineField, timer, Enums.GameDifficulty.Custom, config, seed.Value);
    }

    /// <inheritdoc/>
    public Interfaces.IGame CreateGame(Models.Records.GameSaveData saveData)
    {
        // 创建一个新的服务作用域, 用于管理依赖注入的生命周期
        var serviceScope = _serviceScopeFactory.CreateScope();

        // 从服务作用域中获取内部地雷场实例
        var mineField = serviceScope.ServiceProvider.GetRequiredService<Interfaces.IMineField>();

        // 从服务作用域中获取游戏计时器实例
        var timer = serviceScope.ServiceProvider.GetRequiredService<Interfaces.IGameTimer>();

        // 创建并返回一个新的游戏实例, 使用存档数据中的配置和种子
        return new Game(serviceScope, _boardFactory, mineField, timer, saveData);
    }
}
