using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MineClearance.Core;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using Moq;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="HistoryViewModel"/> 的单元测试, 覆盖统计聚合, 筛选, 排序, 删除与清空
/// </summary>
public sealed class HistoryViewModelTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    /// <summary>
    /// 游戏数据存储库模拟
    /// </summary>
    private readonly Mock<IGameDataRepository> _repository = new();

    /// <summary>
    /// 仓储返回的结果列表
    /// </summary>
    private readonly List<GameResult> _results = [];

    /// <summary>
    /// 历史记录视图模型实例
    /// </summary>
    private readonly HistoryViewModel _viewModel;

    /// <summary>
    /// 初始化模拟与历史记录视图模型
    /// </summary>
    public HistoryViewModelTests()
    {
        _ = _repository.SetupGet(r => r.GameResults).Returns(_results);
        _ = _repository.Setup(
            r => r.DeleteGameResultAsync(It.IsAny<GameResult>(), It.IsAny<CancellationToken>())
        ).Callback<GameResult, CancellationToken>((result, _) => _results.Remove(result)).ReturnsAsync(true);
        _ = _repository.Setup(
            r => r.ClearGameResultsAsync(It.IsAny<CancellationToken>())
        ).ReturnsAsync(true);
        _viewModel = new(
            _repository.Object,
            new(NullLogger<ToastViewModel>.Instance, new(new ConfigurationBuilder().Build()))
        );
    }

    [Fact]
    public void 构造_无记录_总览文本保持空且无结果行()
    {
        Assert.Equal(string.Empty, _viewModel.TotalSummaryText);
        Assert.Empty(_viewModel.DisplayedResults);
        Assert.Empty(_viewModel.StatsRows);
    }

    [Fact]
    public void 刷新_有记录_统计行聚合正确()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(
            GameResult.CreateLoss(2, GameDifficulty.Intermediate, StartTime, TimeSpan.FromMinutes(2), 0.5)
        );
        _results.Add(GameResult.CreateCustomWin(3, StartTime, TimeSpan.FromMinutes(3), 5, 7, 8));

        _viewModel.Refresh();

        Assert.Equal("共 3 局游戏, 胜利 2 局", _viewModel.TotalSummaryText);
        Assert.Equal(3, _viewModel.StatsRows[0].Games);
        Assert.Equal(2, _viewModel.StatsRows[0].Wins);
        Assert.Equal(1, _viewModel.StatsRows[1].Games); // 初级
        Assert.Equal(1, _viewModel.StatsRows[2].Games); // 中级
        Assert.Equal(1, _viewModel.StatsRows[5].Games); // 自定义
        Assert.Equal(3, _viewModel.DisplayedResults.Count);
    }

    [Fact]
    public void 刷新_数据未变化_跳过重建()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _viewModel.Refresh();
        var rowsBefore = _viewModel.StatsRows;

        _viewModel.Refresh();

        Assert.Same(rowsBefore, _viewModel.StatsRows);
    }

    [Fact]
    public void 筛选_选中单个难度_仅显示该难度记录()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(GameResult.CreateWin(2, GameDifficulty.Expert, StartTime, TimeSpan.FromMinutes(2)));
        _viewModel.Refresh();

        _viewModel.DifficultyFilters[0].IsSelected = true; // 初级

        Assert.Equal(GameDifficulty.Beginner, Assert.Single(_viewModel.DisplayedResults).Result.Difficulty);
    }

    [Fact]
    public void 筛选_多选难度_显示任一难度记录()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(GameResult.CreateWin(2, GameDifficulty.Expert, StartTime, TimeSpan.FromMinutes(2)));
        _results.Add(GameResult.CreateWin(3, GameDifficulty.Master, StartTime, TimeSpan.FromMinutes(3)));
        _viewModel.Refresh();

        _viewModel.DifficultyFilters[0].IsSelected = true; // 初级
        _viewModel.DifficultyFilters[2].IsSelected = true; // 高级

        Assert.Equal(2, _viewModel.DisplayedResults.Count);
    }

    [Fact]
    public void 筛选_按起始日期_仅显示当天及之后的记录()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(
            GameResult.CreateWin(2, GameDifficulty.Beginner, StartTime.AddDays(1), TimeSpan.FromMinutes(2))
        );
        _viewModel.Refresh();

        _viewModel.FromDate = StartTime.AddDays(1);

        Assert.Equal(StartTime.AddDays(1), Assert.Single(_viewModel.DisplayedResults).Result.StartTime);
    }

    [Fact]
    public void 筛选_按结束日期_包含当天记录()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(
            GameResult.CreateWin(2, GameDifficulty.Beginner, StartTime.AddDays(2), TimeSpan.FromMinutes(2))
        );
        _viewModel.Refresh();

        _viewModel.ToDate = StartTime.Date;

        Assert.Equal(StartTime, Assert.Single(_viewModel.DisplayedResults).Result.StartTime);
    }

    [Fact]
    public void 筛选_按结果_仅显示胜利记录()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(
            GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2), 0.5)
        );
        _viewModel.Refresh();

        _viewModel.SelectedResultFilter = _viewModel.ResultFilters[1]; // 胜利

        Assert.True(Assert.Single(_viewModel.DisplayedResults).Result.IsWin);
    }

    [Fact]
    public void 清除筛选_恢复显示全部记录()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(GameResult.CreateLoss(2, GameDifficulty.Expert, StartTime, TimeSpan.FromMinutes(2), 0.5));
        _viewModel.Refresh();
        _viewModel.DifficultyFilters[0].IsSelected = true;
        _viewModel.FromDate = StartTime.AddDays(1);
        _viewModel.SelectedResultFilter = _viewModel.ResultFilters[1];

        _viewModel.ClearFiltersCommand.Execute(null);

        Assert.Equal(2, _viewModel.DisplayedResults.Count);
        Assert.DoesNotContain(_viewModel.DifficultyFilters, static option => option.IsSelected);
        Assert.Null(_viewModel.FromDate);
        Assert.Null(_viewModel.ToDate);
        Assert.Same(_viewModel.ResultFilters[0], _viewModel.SelectedResultFilter);
    }

    [Fact]
    public void 切换统计排序_按游戏次数升序_全部行固定置顶()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Master, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(GameResult.CreateWin(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2)));
        _viewModel.Refresh();

        _viewModel.ToggleStatsSort(SortKeys.Games);

        Assert.Equal("全部", _viewModel.StatsRows[0].DifficultyText);
        Assert.Equal(GameDifficulty.Intermediate.GetDescription(), _viewModel.StatsRows[1].DifficultyText);
        Assert.Equal(0, _viewModel.StatsRows[1].Games);
        Assert.Equal(GameDifficulty.Beginner.GetDescription(), _viewModel.StatsRows[4].DifficultyText);
        Assert.Equal(GameDifficulty.Master.GetDescription(), _viewModel.StatsRows[5].DifficultyText);
        Assert.Equal(1, _viewModel.StatsRows[5].Games);
        Assert.Equal("▲", _viewModel.GamesArrow);
    }

    [Fact]
    public void 切换统计排序_同列再次点击_切换为降序()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(GameResult.CreateWin(2, GameDifficulty.Expert, StartTime, TimeSpan.FromMinutes(2)));
        _viewModel.Refresh();

        _viewModel.ToggleStatsSort(SortKeys.Wins);
        _viewModel.ToggleStatsSort(SortKeys.Wins);

        Assert.Equal("▼", _viewModel.WinsArrow);
        Assert.Equal(GameDifficulty.Beginner.GetDescription(), _viewModel.StatsRows[1].DifficultyText);
    }

    [Fact]
    public void 统计排序_缺项数值排最后()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _results.Add(GameResult.CreateLoss(2, GameDifficulty.Expert, StartTime, TimeSpan.FromMinutes(2), 0.5));
        _results.Add(GameResult.CreateLoss(3, GameDifficulty.Master, StartTime, TimeSpan.FromMinutes(3), 0.5));
        _viewModel.Refresh();

        _viewModel.ToggleStatsSort(SortKeys.WinRate);

        Assert.Equal("全部", _viewModel.StatsRows[0].DifficultyText);
        Assert.Equal(GameDifficulty.Intermediate.GetDescription(), _viewModel.StatsRows[4].DifficultyText);
        Assert.Equal(GameDifficulty.Custom.GetDescription(), _viewModel.StatsRows[5].DifficultyText);
    }

    [Fact]
    public async Task 删除选中记录_调用仓储删除并刷新()
    {
        var result = GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        _results.Add(result);
        _viewModel.Refresh();
        _viewModel.SelectedRows = [_viewModel.DisplayedResults[0]];

        await _viewModel.DeleteSelectedCommand.ExecuteAsync(null);

        _repository.Verify(r => r.DeleteGameResultAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(_viewModel.DisplayedResults);
        Assert.NotEmpty(_viewModel.SelectedRows);
    }

    [Fact]
    public async Task 删除选中记录_无选中_不调用仓储()
    {
        await _viewModel.DeleteSelectedCommand.ExecuteAsync(null);

        _repository.Verify(
            r => r.DeleteGameResultAsync(It.IsAny<GameResult>(), It.IsAny<CancellationToken>()), Times.Never
        );
    }

    [Fact]
    public void 清空历史_首次点击_进入确认状态()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));

        _ = _viewModel.ClearAllCommand.ExecuteAsync(null);

        Assert.Equal("确认清空", _viewModel.ClearAllButtonText);
    }

    [Fact]
    public async Task 清空历史_确认后再次点击_清空并恢复按钮文本()
    {
        _results.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        _viewModel.Refresh();

        _ = _viewModel.ClearAllCommand.ExecuteAsync(null); // 首次点击进入确认
        await _viewModel.ClearAllCommand.ExecuteAsync(null); // 确认后立即再点

        _repository.Verify(r => r.ClearGameResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("清空历史", _viewModel.ClearAllButtonText);
    }

    [Fact]
    public void 返回主视图_触发返回事件()
    {
        var requested = false;
        _viewModel.MainViewRequested += () => requested = true;

        _viewModel.BackToMainCommand.Execute(null);

        Assert.True(requested);
    }
}
