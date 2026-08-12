using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using Moq;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="MainViewModel"/> 的单元测试, 覆盖初始状态, 难度联动, 开始游戏与导航
/// </summary>
public sealed class MainViewModelTests
{
    /// <summary>
    /// 游戏数据存储库模拟
    /// </summary>
    private readonly Mock<IGameDataRepository> _dataRepository = new();

    /// <summary>
    /// 游戏管理器模拟
    /// </summary>
    private readonly Mock<IGameManager> _gameManager = new();

    /// <summary>
    /// 主视图模型实例
    /// </summary>
    private readonly MainViewModel _viewModel;

    /// <summary>
    /// 初始化模拟与主视图模型
    /// </summary>
    public MainViewModelTests()
    {
        _viewModel = new(_dataRepository.Object, _gameManager.Object);
    }

    [Fact]
    public void 构造_默认选中初级难度且参数联动()
    {
        Assert.Equal(GameDifficulty.Beginner, _viewModel.SelectedDifficulty);
        Assert.Equal(9, _viewModel.Height);
        Assert.Equal(9, _viewModel.Width);
        Assert.Equal(10, _viewModel.MineCount);
        Assert.Equal(80, _viewModel.MaxMineCount);
        Assert.Empty(_viewModel.SeedText);
        Assert.False(_viewModel.IsCustomDifficulty);
    }

    [Fact]
    public void 构造_仓储无存档_HasSaveData为false()
    {
        Assert.False(_viewModel.HasSaveData);
    }

    [Fact]
    public void 构造_仓储有存档_HasSaveData为true()
    {
        _ = _dataRepository.SetupGet(r => r.SaveData).Returns(CreateSaveData());

        var viewModel = new MainViewModel(_dataRepository.Object, _gameManager.Object);

        Assert.True(viewModel.HasSaveData);
    }

    [Fact]
    public void 切换预设难度_参数与上限联动更新且清空种子()
    {
        _viewModel.SeedText = "123";
        _viewModel.SelectedDifficulty = GameDifficulty.Expert;

        Assert.Equal(16, _viewModel.Height);
        Assert.Equal(30, _viewModel.Width);
        Assert.Equal(99, _viewModel.MineCount);
        Assert.Equal(479, _viewModel.MaxMineCount);
        Assert.Empty(_viewModel.SeedText);
        Assert.False(_viewModel.IsCustomDifficulty);
    }

    [Fact]
    public void 切换到自定义难度_允许编辑参数并保留当前参数()
    {
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;

        Assert.True(_viewModel.IsCustomDifficulty);
        Assert.Null(_viewModel.ParameterInputTip);
    }

    [Fact]
    public void 切换回预设难度_参数输入提示恢复()
    {
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;
        _viewModel.SelectedDifficulty = GameDifficulty.Intermediate;

        Assert.False(_viewModel.IsCustomDifficulty);
        Assert.NotNull(_viewModel.ParameterInputTip);
    }

    [Fact]
    public void 自定义难度修改宽高_地雷上限随之更新并钳制旧值()
    {
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;
        _viewModel.MineCount = 99;
        _viewModel.Height = 5;
        _viewModel.Width = 7;

        Assert.Equal(34, _viewModel.MaxMineCount);
        Assert.Equal(34, _viewModel.MineCount);
    }

    [Fact]
    public void 自定义难度显式设置地雷数_不触发钳制()
    {
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;
        _viewModel.Height = 5;
        _viewModel.Width = 7;

        _viewModel.MineCount = 99;

        Assert.Equal(34, _viewModel.MaxMineCount);
        Assert.Equal(99, _viewModel.MineCount);
    }

    [Fact]
    public async Task 开始新游戏_预设难度_调用游戏管理器并导航至游戏视图()
    {
        NavigationTarget? navigated = null;
        _viewModel.NavigationRequested += target => navigated = target;

        await _viewModel.StartNewGameCommand.ExecuteAsync(null);

        _gameManager.Verify(m => m.StartNewGame(GameDifficulty.Beginner), Times.Once);
        Assert.Equal(NavigationTarget.GameView, navigated);
    }

    [Fact]
    public async Task 开始新游戏_自定义难度_以输入参数构建配置并解析种子()
    {
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;
        _viewModel.Height = 5;
        _viewModel.Width = 7;
        _viewModel.MineCount = 8;
        _viewModel.SeedText = "42";

        await _viewModel.StartNewGameCommand.ExecuteAsync(null);

        _gameManager.Verify(m => m.StartNewGame(new(5, 7, 8), 42), Times.Once);
    }

    [Fact]
    public async Task 开始新游戏_自定义难度种子为空_不传种子()
    {
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;
        _viewModel.Height = 5;
        _viewModel.Width = 7;
        _viewModel.MineCount = 8;
        _viewModel.SeedText = string.Empty;

        await _viewModel.StartNewGameCommand.ExecuteAsync(null);

        _gameManager.Verify(m => m.StartNewGame(new(5, 7, 8), null), Times.Once);
    }

    [Fact]
    public async Task 开始新游戏_自定义难度配置无效_不开始游戏也不导航()
    {
        NavigationTarget? navigated = null;
        _viewModel.NavigationRequested += target => navigated = target;
        _viewModel.SelectedDifficulty = GameDifficulty.Custom;
        _viewModel.Height = 0;
        _viewModel.Width = 0;
        _viewModel.MineCount = 0;

        await _viewModel.StartNewGameCommand.ExecuteAsync(null);

        _gameManager.Verify(
            m => m.StartNewGame(It.IsAny<GameConfig>(), It.IsAny<int?>()), Times.Never
        );
        Assert.Null(navigated);
    }

    [Fact]
    public void 继续游戏_无存档_不执行任何操作()
    {
        NavigationTarget? navigated = null;
        _viewModel.NavigationRequested += target => navigated = target;

        _viewModel.ContinueGameCommand.Execute(null);

        _gameManager.Verify(m => m.RestoreFromSaveData(), Times.Never);
        Assert.Null(navigated);
    }

    [Fact]
    public void 继续游戏_有存档_恢复游戏并导航()
    {
        _ = _dataRepository.SetupGet(r => r.SaveData).Returns(CreateSaveData());
        var viewModel = new MainViewModel(_dataRepository.Object, _gameManager.Object);
        NavigationTarget? navigated = null;
        viewModel.NavigationRequested += target => navigated = target;

        viewModel.ContinueGameCommand.Execute(null);

        _gameManager.Verify(m => m.RestoreFromSaveData(), Times.Once);
        Assert.Equal(NavigationTarget.GameView, navigated);
    }

    [Fact]
    public void 显示历史记录_触发导航事件()
    {
        NavigationTarget? navigated = null;
        _viewModel.NavigationRequested += target => navigated = target;

        _viewModel.ShowHistoryCommand.Execute(null);

        Assert.Equal(NavigationTarget.HistoryView, navigated);
    }

    [Fact]
    public void 显示设置_触发导航事件()
    {
        NavigationTarget? navigated = null;
        _viewModel.NavigationRequested += target => navigated = target;

        _viewModel.ShowSettingsCommand.Execute(null);

        Assert.Equal(NavigationTarget.SettingsDrawer, navigated);
    }

    [Fact]
    public void 退出_触发退出事件()
    {
        var exited = false;
        _viewModel.ExitRequested += () => exited = true;

        _viewModel.ExitCommand.Execute(null);

        Assert.True(exited);
    }

    [Fact]
    public void 刷新存档状态_按仓储当前存档更新()
    {
        Assert.False(_viewModel.HasSaveData);

        _ = _dataRepository.SetupGet(r => r.SaveData).Returns(CreateSaveData());
        _viewModel.RefreshSaveDataState();

        Assert.True(_viewModel.HasSaveData);

        _ = _dataRepository.SetupGet(r => r.SaveData).Returns((GameSaveData?)null);
        _viewModel.RefreshSaveDataState();

        Assert.False(_viewModel.HasSaveData);
    }

    /// <summary>
    /// 创建测试用的存档数据
    /// </summary>
    /// <returns>测试用的存档数据</returns>
    private static GameSaveData CreateSaveData()
    {
        return GameSaveData.Create(
            42, GameDifficulty.Beginner, new(2026, 8, 12, 18, 0, 0), TimeSpan.FromMinutes(1),
            new(81), new Dictionary<Position, CellType>()
        );
    }
}
