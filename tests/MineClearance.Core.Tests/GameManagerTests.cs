using Microsoft.Extensions.Logging.Abstractions;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.Core.Services;
using Moq;
using System.ComponentModel;

namespace MineClearance.Core.Tests;

/// <summary>
/// GameManager 的单元测试, 覆盖游戏创建、恢复、重开、存档保存与结果持久化
/// </summary>
public sealed class GameManagerTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    /// <summary>
    /// 测试用的自定义配置
    /// </summary>
    private static readonly GameConfig Config = new(5, 5, 2);

    /// <summary>
    /// 游戏工厂模拟
    /// </summary>
    private readonly Mock<IGameFactory> _gameFactory = new();

    /// <summary>
    /// 游戏数据存储库模拟
    /// </summary>
    private readonly Mock<IGameDataRepository> _dataRepository = new();

    /// <summary>
    /// 游戏管理器实例
    /// </summary>
    private readonly GameManager _manager;

    /// <summary>
    /// 构造函数, 初始化模拟和游戏管理器
    /// </summary>
    public GameManagerTests()
    {
        _manager = new(_gameFactory.Object, _dataRepository.Object, NullLogger<GameManager>.Instance);
    }

    /// <summary>
    /// 创建指定难度的游戏模拟
    /// </summary>
    /// <param name="difficulty">游戏难度</param>
    /// <param name="config">游戏配置</param>
    /// <returns>游戏模拟</returns>
    private static Mock<IGame> CreateGameMock(GameDifficulty difficulty, GameConfig config)
    {
        var mock = new Mock<IGame>();
        _ = mock.SetupGet(game => game.Difficulty).Returns(difficulty);
        _ = mock.SetupGet(game => game.Config).Returns(config);
        return mock;
    }

    /// <summary>
    /// 创建有效的存档数据
    /// </summary>
    /// <returns>存档数据</returns>
    private static GameSaveData CreateSaveData()
    {
        return GameSaveData.Create(
            42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1),
            new(81), new Dictionary<Position, CellType>()
        );
    }

    [Fact]
    public void 构造_初始状态_当前没有游戏()
    {
        Assert.Null(_manager.Game);
    }

    [Fact]
    public void StartNewGame_内置难度_创建游戏并删除旧存档()
    {
        var game = new Mock<IGame>();
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);

        _manager.StartNewGame(GameDifficulty.Beginner);

        Assert.Same(game.Object, _manager.Game);
        _gameFactory.Verify(factory => factory.CreateGame(GameDifficulty.Beginner), Times.Once);
        _dataRepository.Verify(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>()), Times.Once
        );
    }

    [Fact]
    public void StartNewGame_自定义难度_抛出ArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(() => _manager.StartNewGame(GameDifficulty.Custom));
        Assert.Null(_manager.Game);
    }

    [Fact]
    public void StartNewGame_配置无效_抛出ArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(() => _manager.StartNewGame(new GameConfig(0, 9, 10)));
    }

    [Fact]
    public void StartNewGame_有效配置_使用配置创建游戏()
    {
        var game = new Mock<IGame>();
        _ = _gameFactory.Setup(factory => factory.CreateGame(Config, It.IsAny<int?>())).Returns(game.Object);

        _manager.StartNewGame(Config);

        Assert.Same(game.Object, _manager.Game);
        _gameFactory.Verify(factory => factory.CreateGame(Config, null), Times.Once);
    }

    [Fact]
    public void StartNewGame_指定种子_传递给工厂()
    {
        _ = _gameFactory.Setup(factory => factory.CreateGame(Config, 123)).Returns(new Mock<IGame>().Object);

        _manager.StartNewGame(Config, 123);

        _gameFactory.Verify(factory => factory.CreateGame(Config, 123), Times.Once);
    }

    [Fact]
    public void StartNewGame_更换游戏_释放旧游戏并触发通知事件()
    {
        var oldGame = new Mock<IGame>();
        var newGame = new Mock<IGame>();
        _ = _gameFactory.SetupSequence(factory => factory.CreateGame(It.IsAny<GameDifficulty>()))
            .Returns(oldGame.Object).Returns(newGame.Object);
        var propertyChangedCount = 0;
        _manager.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IGameManager.Game)) { propertyChangedCount++; }
        };

        _manager.StartNewGame(GameDifficulty.Beginner);
        _manager.StartNewGame(GameDifficulty.Intermediate);

        oldGame.Verify(game => game.Dispose(), Times.Once);
        Assert.Same(newGame.Object, _manager.Game);
        Assert.Equal(2, propertyChangedCount);
    }

    [Fact]
    public void RestoreFromSaveData_存在存档_从存档恢复游戏()
    {
        var saveData = CreateSaveData();
        var game = new Mock<IGame>();
        _ = _dataRepository.SetupGet(repository => repository.SaveData).Returns(saveData);
        _ = _gameFactory.Setup(factory => factory.CreateGame(saveData)).Returns(game.Object);

        _manager.RestoreFromSaveData();

        Assert.Same(game.Object, _manager.Game);
        _gameFactory.Verify(factory => factory.CreateGame(saveData), Times.Once);
    }

    [Fact]
    public void RestartCurrentGame_没有游戏_不执行任何操作()
    {
        _manager.RestartCurrentGame();

        _gameFactory.Verify(factory => factory.CreateGame(It.IsAny<GameDifficulty>()), Times.Never);
        _gameFactory.Verify(factory => factory.CreateGame(
            It.IsAny<GameConfig>(), It.IsAny<int?>()), Times.Never
        );
    }

    [Fact]
    public void RestartCurrentGame_自定义难度_使用配置重新开始()
    {
        var game = CreateGameMock(GameDifficulty.Custom, Config);
        _ = _gameFactory.Setup(factory => factory.CreateGame(Config, It.IsAny<int?>())).Returns(game.Object);
        _manager.StartNewGame(Config);

        _manager.RestartCurrentGame();

        _gameFactory.Verify(factory => factory.CreateGame(Config, It.IsAny<int?>()), Times.Exactly(2));
    }

    [Fact]
    public void RestartCurrentGame_内置难度_使用难度重新开始()
    {
        var game = CreateGameMock(GameDifficulty.Beginner, Constants.BeginnerConfig);
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _manager.StartNewGame(GameDifficulty.Beginner);

        _manager.RestartCurrentGame();

        _gameFactory.Verify(factory => factory.CreateGame(GameDifficulty.Beginner), Times.Exactly(2));
    }

    [Fact]
    public void ExitWithoutSaving_清空当前游戏并释放()
    {
        var game = new Mock<IGame>();
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _manager.StartNewGame(GameDifficulty.Beginner);

        _manager.ExitWithoutSaving();

        Assert.Null(_manager.Game);
        game.Verify(g => g.Dispose(), Times.Once);
    }

    [Fact]
    public async Task SaveAndExitAsync_没有游戏_直接返回成功()
    {
        var result = await _manager.SaveAndExitAsync(TestContext.Current.CancellationToken);

        Assert.True(result);
        _dataRepository.Verify(
            repository =>
                repository.SaveGameSaveDataAsync(It.IsAny<GameSaveData>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _dataRepository.Verify(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>()), Times.Never
        );
    }

    [Fact]
    public async Task SaveAndExitAsync_有存档_保存存档并退出()
    {
        var saveData = CreateSaveData();
        var game = new Mock<IGame>();
        _ = game.Setup(g => g.GetSaveData()).Returns(saveData);
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _ = _dataRepository.Setup(
            repository => repository.SaveGameSaveDataAsync(saveData, It.IsAny<CancellationToken>())
        ).ReturnsAsync(true);
        _manager.StartNewGame(GameDifficulty.Beginner);

        var result = await _manager.SaveAndExitAsync(TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Null(_manager.Game);
        _dataRepository.Verify(
            repository => repository.SaveGameSaveDataAsync(saveData, It.IsAny<CancellationToken>()),
            Times.Once
        );
        game.Verify(g => g.Dispose(), Times.Once);
    }

    [Fact]
    public async Task SaveAndExitAsync_没有存档数据_删除旧存档并退出()
    {
        var game = new Mock<IGame>();
        _ = game.Setup(g => g.GetSaveData()).Returns((GameSaveData?)null);
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _ = _dataRepository.Setup(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>())
        ).ReturnsAsync(true);
        _manager.StartNewGame(GameDifficulty.Beginner);

        var result = await _manager.SaveAndExitAsync(TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Null(_manager.Game);
        _dataRepository.Verify(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)
        );
    }

    [Fact]
    public async Task SaveAndExitAsync_保存失败_返回false()
    {
        var saveData = CreateSaveData();
        var game = new Mock<IGame>();
        _ = game.Setup(g => g.GetSaveData()).Returns(saveData);
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _ = _dataRepository.Setup(
            repository => repository.SaveGameSaveDataAsync(saveData, It.IsAny<CancellationToken>())
        ).ReturnsAsync(false);
        _manager.StartNewGame(GameDifficulty.Beginner);

        var result = await _manager.SaveAndExitAsync(TestContext.Current.CancellationToken);

        Assert.False(result);
        Assert.Null(_manager.Game);
    }

    [Fact]
    public async Task 游戏结束_自动保存结果并删除存档()
    {
        var addResultTcs = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        _ = _dataRepository.Setup(
            repository => repository.AddGameResultAsync(It.IsAny<GameResult>(), It.IsAny<CancellationToken>())
        ).Returns(addResultTcs.Task);
        _ = _dataRepository.Setup(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>())
        ).ReturnsAsync(true);
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var game = new Mock<IGame>();
        _ = game.SetupGet(g => g.Result).Returns(result);
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _manager.StartNewGame(GameDifficulty.Beginner);

        game.Raise(
            g => g.PropertyChanged += null, game.Object, new PropertyChangedEventArgs(nameof(IGame.Result))
        );
        addResultTcs.SetResult(true);
        await Task.Delay(200, TestContext.Current.CancellationToken);

        _dataRepository.Verify(
            repository => repository.AddGameResultAsync(result, It.IsAny<CancellationToken>()), Times.Once
        );
        _dataRepository.Verify(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)
        );
    }

    [Fact]
    public async Task 游戏结束_保存结果失败_不删除存档()
    {
        _ = _dataRepository.Setup(
            repository => repository.AddGameResultAsync(It.IsAny<GameResult>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(false);
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var game = new Mock<IGame>();
        _ = game.SetupGet(g => g.Result).Returns(result);
        _ = _gameFactory.Setup(factory => factory.CreateGame(GameDifficulty.Beginner)).Returns(game.Object);
        _manager.StartNewGame(GameDifficulty.Beginner);

        game.Raise(
            g => g.PropertyChanged += null, game.Object, new PropertyChangedEventArgs(nameof(IGame.Result))
        );
        await Task.Delay(200, TestContext.Current.CancellationToken);

        _dataRepository.Verify(
            repository => repository.DeleteGameSaveDataAsync(It.IsAny<CancellationToken>()), Times.Once
        );
    }

    [Fact]
    public void 更换游戏后_旧游戏的属性变化不再触发保存()
    {
        var oldGame = new Mock<IGame>();
        var newGame = new Mock<IGame>();
        _ = _gameFactory.SetupSequence(factory => factory.CreateGame(It.IsAny<GameDifficulty>()))
            .Returns(oldGame.Object).Returns(newGame.Object);
        _manager.StartNewGame(GameDifficulty.Beginner);
        _manager.StartNewGame(GameDifficulty.Intermediate);

        oldGame.Raise(
            g => g.PropertyChanged += null, oldGame.Object, new PropertyChangedEventArgs(nameof(IGame.Result))
        );

        _dataRepository.Verify(
            repository => repository.AddGameResultAsync(It.IsAny<GameResult>(), It.IsAny<CancellationToken>()), Times.Never
        );
    }
}
