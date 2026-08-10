using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MineClearance.Core;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 历史记录视图模型, 负责游戏结果的统计聚合, 排序, 筛选, 删除与清空
/// </summary>
public sealed partial class HistoryViewModel : ObservableObject
{
    /// <summary>
    /// 空统计行文本, 用于无数据时显示
    /// </summary>
    private const string EmptyStatsText = "--";

    /// <summary>
    /// 游戏数据存储库, 用于读取与修改游戏结果记录
    /// </summary>
    private readonly IGameDataRepository _dataRepository;

    /// <summary>
    /// 全局短暂提示视图模型
    /// </summary>
    private readonly ToastViewModel _toast;

    /// <summary>
    /// 全部结果行 (未筛选未排序)
    /// </summary>
    private List<GameResultRow> _allRows = [];

    /// <summary>
    /// 筛选后的结果行
    /// </summary>
    private List<GameResultRow> _filteredRows = [];

    /// <summary>
    /// 全部统计行 (未排序), 首行为固定置顶的"全部"行
    /// </summary>
    private List<StatsRow> _allStats = [];

    /// <summary>
    /// 统计表格当前排序键
    /// </summary>
    private string? _statsSortKey;

    /// <summary>
    /// 统计表格是否降序排序
    /// </summary>
    private bool _statsSortDescending;

    /// <summary>
    /// 是否处于清空二次确认状态
    /// </summary>
    private bool _isClearConfirmed;

    /// <summary>
    /// 总览信息行文本, 显示总游戏数与胜利局数
    /// </summary>
    [ObservableProperty]
    public partial string TotalSummaryText { get; set; } = string.Empty;

    /// <summary>
    /// 统计行集合, 6 组: 全部/初级/中级/高级/大师/自定义
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<StatsRow> StatsRows { get; set; } = [];

    /// <summary>
    /// 筛选与排序后的结果行, 绑定详细记录表格
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<GameResultRow> DisplayedResults { get; set; } = [];

    /// <summary>
    /// 起始日期筛选, 为空表示不限
    /// </summary>
    [ObservableProperty]
    public partial DateTimeOffset? FromDate { get; set; }

    /// <summary>
    /// 结束日期筛选, 为空表示不限
    /// </summary>
    [ObservableProperty]
    public partial DateTimeOffset? ToDate { get; set; }

    /// <summary>
    /// 当前选中的结果筛选
    /// </summary>
    [ObservableProperty]
    public partial ResultFilterOption? SelectedResultFilter { get; set; }

    /// <summary>
    /// 详细记录表格中选中的行, 由视图层在选中变化时同步
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<GameResultRow> SelectedRows { get; set; } = [];

    /// <summary>
    /// 清空历史按钮文本, 二次确认时切换为确认提示
    /// </summary>
    [ObservableProperty]
    public partial string ClearAllButtonText { get; set; } = "清空历史";

    /// <summary>
    /// 难度筛选选项列表, 支持多选, 不选任何项表示全部难度
    /// </summary>
    public IReadOnlyList<DifficultyFilterOption> DifficultyFilters { get; } =
        [.. Enum.GetValues<GameDifficulty>().Select(static d => new DifficultyFilterOption(d))];

    /// <summary>
    /// 结果筛选选项列表
    /// </summary>
    public IReadOnlyList<ResultFilterOption> ResultFilters { get; } =
    [
        new(null, "全部"),
        new(true, "胜利"),
        new(false, "失败")
    ];

    /// <summary>
    /// 统计表格难度列头排序箭头 (▲/▼/空)
    /// </summary>
    public string StatsDifficultyArrow => GetStatsArrow(SortKeys.DifficultyText);

    /// <summary>
    /// 统计表格游戏次数列头排序箭头 (▲/▼/空)
    /// </summary>
    public string GamesArrow => GetStatsArrow(SortKeys.Games);

    /// <summary>
    /// 统计表格胜利次数列头排序箭头 (▲/▼/空)
    /// </summary>
    public string WinsArrow => GetStatsArrow(SortKeys.Wins);

    /// <summary>
    /// 统计表格胜率列头排序箭头 (▲/▼/空)
    /// </summary>
    public string WinRateArrow => GetStatsArrow(SortKeys.WinRate);

    /// <summary>
    /// 统计表格平均胜利用时列头排序箭头 (▲/▼/空)
    /// </summary>
    public string AvgWinDurationArrow => GetStatsArrow(SortKeys.AvgWinDuration);

    /// <summary>
    /// 统计表格最短胜利用时列头排序箭头 (▲/▼/空)
    /// </summary>
    public string MinWinDurationArrow => GetStatsArrow(SortKeys.MinWinDuration);

    /// <summary>
    /// 统计表格平均完成度列头排序箭头 (▲/▼/空)
    /// </summary>
    public string AvgCompletionArrow => GetStatsArrow(SortKeys.AvgCompletion);

    /// <summary>
    /// 请求返回主视图的事件, 由壳视图模型处理
    /// </summary>
    public event Action? MainViewRequested;

    /// <summary>
    /// 创建历史记录视图模型
    /// </summary>
    /// <param name="dataRepository">游戏数据存储库</param>
    /// <param name="toast">全局短暂提示视图模型</param>
    public HistoryViewModel(IGameDataRepository dataRepository, ToastViewModel toast)
    {
        _dataRepository = dataRepository;
        _toast = toast;

        // 默认结果筛选全部
        SelectedResultFilter = ResultFilters[0];

        // 订阅每个难度选项的选中状态变化, 重新应用筛选
        foreach (var option in DifficultyFilters)
        {
            option.PropertyChanged += OnDifficultyFilterOptionChanged;
        }

        Refresh();
    }

    /// <summary>
    /// 起始日期变化时重新应用筛选
    /// </summary>
    /// <param name="value">新的起始日期</param>
    partial void OnFromDateChanged(DateTimeOffset? value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// 结束日期变化时重新应用筛选
    /// </summary>
    /// <param name="value">新的结束日期</param>
    partial void OnToDateChanged(DateTimeOffset? value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// 结果筛选变化时重新应用筛选
    /// </summary>
    /// <param name="value">新的结果筛选</param>
    partial void OnSelectedResultFilterChanged(ResultFilterOption? value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// 返回主视图
    /// </summary>
    [RelayCommand]
    private void BackToMain()
    {
        MainViewRequested?.Invoke();
    }

    /// <summary>
    /// 清除筛选, 恢复显示全部记录
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        foreach (var option in DifficultyFilters)
        {
            option.IsSelected = false;
        }
        FromDate = null;
        ToDate = null;
        SelectedResultFilter = ResultFilters[0];
    }

    /// <summary>
    /// 删除选中的记录, 不做确认弹窗, 删除后刷新并提示
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedRows is not { Count: > 0 } rows) { return; }

        // 收集选中的游戏结果并逐个删除
        var results = rows.Select(static row => row.Result).ToArray();
        var failed = 0;
        foreach (var result in results)
        {
            if (!await _dataRepository.DeleteGameResultAsync(result, App.ExitCts.Token).ConfigureAwait(false))
            {
                failed++;
            }
        }

        Refresh();
        _toast.Show(failed == 0 ? $"已删除 {results.Length} 条记录" : $"{failed} 条记录删除失败");
    }

    /// <summary>
    /// 清空历史: 首次点击进入确认状态, 3 秒内再次点击才执行
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ClearAllAsync()
    {
        // 第一次点击: 进入确认状态并显示提示文本, 3 秒后自动恢复
        if (!_isClearConfirmed)
        {
            _isClearConfirmed = true;
            ClearAllButtonText = "确认清空";
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), App.ExitCts.Token);
            }
            finally
            {
                _isClearConfirmed = false;
                ClearAllButtonText = "清空历史";
            }
            return;
        }

        // 第二次点击: 执行清空
        _isClearConfirmed = false;
        ClearAllButtonText = "清空历史";
        var ok = await _dataRepository.ClearGameResultsAsync(App.ExitCts.Token).ConfigureAwait(false);
        Refresh();
        _toast.Show(ok ? "历史记录已清空" : "清空历史失败");
    }

    /// <summary>
    /// 刷新数据: 重读仓储中的游戏结果, 重算统计并应用筛选与排序, 由壳视图模型在切换到历史视图时调用
    /// </summary>
    public void Refresh()
    {
        var results = _dataRepository.GameResults;
        TotalSummaryText = $"共 {results.Count} 局游戏, 胜利 {results.Count(static result => result.IsWin)} 局";
        _allStats = [.. CreateStatsRows(results)];
        ApplyStatsSort();
        _allRows = [.. results.Select(static result => new GameResultRow(result))];
        ApplyFilters();
    }

    /// <summary>
    /// 切换统计表格排序: "全部"行固定置顶不参与排序, 同一列再次点击切换方向, 新列默认升序
    /// </summary>
    /// <param name="key">排序键</param>
    public void ToggleStatsSort(string key)
    {
        if (string.Equals(_statsSortKey, key, StringComparison.Ordinal))
        {
            _statsSortDescending = !_statsSortDescending;
        }
        else
        {
            _statsSortKey = key;
            _statsSortDescending = false;
        }

        ApplyStatsSort();
        NotifyStatsHeaders();
    }

    /// <summary>
    /// 显示短暂提示
    /// </summary>
    /// <param name="message">提示文本</param>
    public void Show(string message)
    {
        _toast.Show(message);
    }

    /// <summary>
    /// 按当前筛选条件过滤结果行: 难度多选 (不选任何项表示全部), 日期范围, 结果单选
    /// </summary>
    private void ApplyFilters()
    {
        // 难度多选: 选中集合非空时仅保留选中难度的记录
        var selectedDifficulties = DifficultyFilters.Where(static option => option.IsSelected).Select(static option => option.Difficulty).ToHashSet();
        var fromDate = FromDate?.Date;
        var toDate = ToDate?.Date;

        IEnumerable<GameResultRow> filtered = _allRows;
        if (selectedDifficulties.Count > 0)
        {
            filtered = filtered.Where(row => selectedDifficulties.Contains(row.Result.Difficulty));
        }
        if (fromDate is not null)
        {
            filtered = filtered.Where(row => row.Result.StartTime >= fromDate.Value);
        }
        if (toDate is not null)
        {
            // 结束日期包含当天, 以次日零点为界
            filtered = filtered.Where(row => row.Result.StartTime < toDate.Value.AddDays(1));
        }
        if (SelectedResultFilter is { IsWin: { } isWin })
        {
            filtered = filtered.Where(row => row.Result.IsWin == isWin);
        }

        _filteredRows = [.. filtered];

        // 详细记录排序由 DataGrid 内置排序负责, 视图模型只提供筛选后的原始数据
        DisplayedResults = _filteredRows;
    }

    /// <summary>
    /// 按当前统计排序键与方向排列统计行, 首行的"全部"行始终固定在第一
    /// </summary>
    private void ApplyStatsSort()
    {
        if (_allStats is not { Count: > 0 } allStats) { return; }

        // "全部"行固定置顶, 只对剩余行排序
        var fixedRow = allStats[0];
        var others = allStats.Skip(1);
        var descending = _statsSortDescending;
        var sorted = _statsSortKey switch
        {
            SortKeys.DifficultyText => descending
                ? others.OrderByDescending(static row => row.DifficultyText)
                : others.OrderBy(static row => row.DifficultyText),
            SortKeys.Games => descending
                ? others.OrderByDescending(static row => row.Games)
                : others.OrderBy(static row => row.Games),
            SortKeys.Wins => descending
                ? others.OrderByDescending(static row => row.Wins)
                : others.OrderBy(static row => row.Wins),
            // 缺项数值为 -1, 升序映射为最大值排最后, 降序映射为最小值排最后
            SortKeys.WinRate => descending
                ? others.OrderByDescending(static row => row.WinRate >= 0 ? row.WinRate : double.MinValue)
                : others.OrderBy(static row => row.WinRate >= 0 ? row.WinRate : double.MaxValue),
            // 缺项时长为 null, 升序映射为最大时长排最后, 降序映射为最小时长排最后
            SortKeys.AvgWinDuration => descending
                ? others.OrderByDescending(static row => row.AvgWinDuration ?? TimeSpan.MinValue)
                : others.OrderBy(static row => row.AvgWinDuration ?? TimeSpan.MaxValue),
            SortKeys.MinWinDuration => descending
                ? others.OrderByDescending(static row => row.MinWinDuration ?? TimeSpan.MinValue)
                : others.OrderBy(static row => row.MinWinDuration ?? TimeSpan.MaxValue),
            SortKeys.AvgCompletion => descending
                ? others.OrderByDescending(static row => row.AvgCompletion >= 0 ? row.AvgCompletion : double.MinValue)
                : others.OrderBy(static row => row.AvgCompletion >= 0 ? row.AvgCompletion : double.MaxValue),
            _ => null
        };

        StatsRows = sorted is null ? allStats : [fixedRow, .. sorted];
    }

    /// <summary>
    /// 统计排序变化时通知所有列头文本, 刷新方向指示箭头
    /// </summary>
    private void NotifyStatsHeaders()
    {
        OnPropertyChanged(nameof(StatsDifficultyArrow));
        OnPropertyChanged(nameof(GamesArrow));
        OnPropertyChanged(nameof(WinsArrow));
        OnPropertyChanged(nameof(WinRateArrow));
        OnPropertyChanged(nameof(AvgWinDurationArrow));
        OnPropertyChanged(nameof(MinWinDurationArrow));
        OnPropertyChanged(nameof(AvgCompletionArrow));
    }

    /// <summary>
    /// 获取统计列头排序箭头, 当前排序列返回方向箭头, 其余为空
    /// </summary>
    /// <param name="key">排序键</param>
    /// <returns>排序方向箭头文本</returns>
    private string GetStatsArrow(string key)
    {
        return _statsSortKey == key ? (_statsSortDescending ? "▼" : "▲") : string.Empty;
    }

    /// <summary>
    /// 难度选项选中状态变化时重新应用筛选
    /// </summary>
    /// <param name="sender">难度筛选选项</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnDifficultyFilterOptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DifficultyFilterOption.IsSelected))
        {
            ApplyFilters();
        }
    }

    /// <summary>
    /// 构建 6 组统计行 (全部/初级/中级/高级/大师/自定义)
    /// </summary>
    /// <param name="results">全部游戏结果</param>
    /// <returns>统计行集合</returns>
    private static IReadOnlyList<StatsRow> CreateStatsRows(IReadOnlyList<GameResult> results)
    {
        return
        [
            CreateStats("全部", results),
            CreateStats(
                GameDifficulty.Beginner.GetDescription(),
                results.Where(static result => result.Difficulty is GameDifficulty.Beginner)
            ),
            CreateStats(
                GameDifficulty.Intermediate.GetDescription(),
                results.Where(static result => result.Difficulty is GameDifficulty.Intermediate)
            ),
            CreateStats(
                GameDifficulty.Expert.GetDescription(),
                results.Where(static result => result.Difficulty is GameDifficulty.Expert)
            ),
            CreateStats(
                GameDifficulty.Master.GetDescription(),
                results.Where(static result => result.Difficulty is GameDifficulty.Master)
            ),
            CreateStats(
                GameDifficulty.Custom.GetDescription(),
                results.Where(static result => result.Difficulty is GameDifficulty.Custom)
            )
        ];
    }

    /// <summary>
    /// 计算一组游戏结果的汇总统计
    /// </summary>
    /// <param name="text">难度范围文本</param>
    /// <param name="results">该组的游戏结果</param>
    /// <returns>统计行</returns>
    private static StatsRow CreateStats(string text, IEnumerable<GameResult> results)
    {
        var list = results.ToList();
        var wins = list.Where(static result => result.IsWin).ToList();
        var losses = list.Where(static result => !result.IsWin).ToList();

        TimeSpan? avgWinDuration = wins.Count == 0 ? null : TimeSpan.FromTicks((long)wins.Average(static result => result.Duration.Ticks));
        TimeSpan? minWinDuration = wins.Count == 0 ? null : wins.Min(static result => result.Duration);
        double? avgCompletion = losses.Count == 0 ? null : losses.Average(static result => result.Completion!.Value);

        return new(
            DifficultyText: text,
            Games: list.Count,
            Wins: wins.Count,
            WinRateText: list.Count == 0
                ? EmptyStatsText
                : $"{wins.Count * Constants.PercentBase / list.Count:0.##}%",
            WinRate: list.Count == 0 ? -1 : wins.Count * Constants.PercentBase / list.Count,
            AvgWinDurationText: avgWinDuration is null
                ? EmptyStatsText
                : FormatTimeSpan(avgWinDuration.Value),
            AvgWinDuration: avgWinDuration,
            MinWinDurationText: minWinDuration is null
                ? EmptyStatsText
                : FormatTimeSpan(minWinDuration.Value),
            MinWinDuration: minWinDuration,
            AvgCompletionText: avgCompletion is null
                ? EmptyStatsText
                : $"{avgCompletion.Value * Constants.PercentBase:0.##}%",
            AvgCompletion: avgCompletion ?? -1
        );
    }

    /// <summary>
    /// 格式化统计用时为 MM:SS
    /// </summary>
    /// <param name="time">用时</param>
    /// <returns>格式化后的文本</returns>
    private static string FormatTimeSpan(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }
}
