using MineClearance.Models;
using MineClearance.Services;

namespace MineClearance.Utilities;

/// <summary>
/// 结果管理类, 用于实现游戏结果的筛选和排序
/// </summary>
internal static class ResultManager
{
    /// <summary>
    /// 筛选条件项
    /// </summary>
    private sealed class FilterConditionItem
    {
        /// <summary>
        /// 筛选器, 用于定义筛选逻辑
        /// </summary>
        public required Func<GameResult, bool> Filter { get; init; }

        /// <summary>
        /// 筛选的属性
        /// </summary>
        public required string Property { get; init; }
    }

    /// <summary>
    /// 排序条件项
    /// </summary>
    private sealed class SortConditionItem
    {
        /// <summary>
        /// 比较器, 用于定义排序逻辑
        /// </summary>
        public required IComparer<GameResult> Comparer { get; init; }

        /// <summary>
        /// 优先级, 数字越小优先级越高
        /// </summary>
        public int Priority { get; init; }
    }

    /// <summary>
    /// 筛选条件集合
    /// </summary>
    private static readonly List<FilterConditionItem> _filterConditions;

    /// <summary>
    /// 排序条件集合
    /// </summary>
    private static readonly List<SortConditionItem> _sortConditions;

    /// <summary>
    /// 原始游戏结果列表
    /// </summary>
    public static IReadOnlyList<GameResult> OriginalResults => Datas.GameResults;

    /// <summary>
    /// 获取操作后的游戏结果(只读列表)
    /// </summary>
    public static IReadOnlyList<GameResult> Results { get; private set; }

    /// <summary>
    /// 条件发生变化事件
    /// </summary>
    public static event Action? ConditionsChanged;

    /// <summary>
    /// 静态构造函数, 初始化结果列表
    /// </summary>
    static ResultManager()
    {
        _filterConditions = [];
        _sortConditions = [];
        Results = [];

        // 订阅条件变化事件
        ConditionsChanged += ProcessResults;
    }

    /// <summary>
    /// 添加筛选条件
    /// </summary>
    /// <param name="condition">筛选条件</param>
    /// <param name="property">筛选的属性</param>
    public static void AddFilterCondition(Func<GameResult, bool> condition, string property)
    {
        _filterConditions.Add(new() { Filter = condition, Property = property });
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 添加排序条件, 并指定优先级, 需要确保优先级唯一
    /// </summary>
    /// <param name="comparer">比较器</param>
    /// <param name="priority">优先级, 数字越小优先级越高</param>
    /// <exception cref="ArgumentException">如果优先级已存在</exception>
    public static void AddSortCondition(IComparer<GameResult> comparer, int priority)
    {
        // 确保优先级唯一
        if (_sortConditions.Any(sc => sc.Priority == priority))
        {
            throw new ArgumentException($"优先级 {priority} 已存在", nameof(priority));
        }
        _sortConditions.Add(new() { Comparer = comparer, Priority = priority });
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 删除指定筛选条件
    /// </summary>
    /// <param name="condition">筛选条件</param>
    public static void RemoveFilterCondition(Func<GameResult, bool> condition)
    {
        _ = _filterConditions.RemoveAll(c => c.Filter == condition);
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 根据指定的优先级删除排序条件
    /// </summary>
    /// <param name="priority">优先级</param>
    public static void RemoveSortCondition(int priority)
    {
        _ = _sortConditions.RemoveAll(sc => sc.Priority == priority);
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 清除所有筛选和排序条件
    /// </summary>
    public static void ClearConditions()
    {
        _filterConditions.Clear();
        _sortConditions.Clear();
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 异步删除指定下标的游戏结果
    /// </summary>
    /// <param name="index">要删除的游戏结果下标</param>
    public static async Task RemoveResultAt(int index)
    {
        await Datas.RemoveGameResultAsync(Results[index]);
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 异步清除所有游戏结果
    /// </summary>
    public static async Task ClearAllResultsAsync()
    {
        await Datas.ClearGameResultsAsync();
        ConditionsChanged?.Invoke();
    }

    /// <summary>
    /// 对游戏结果进行筛选和排序
    /// </summary>
    private static void ProcessResults()
    {
        // 操作后的结果
        var processedResults = Datas.GameResults.AsEnumerable();

        // 应用筛选条件, 如果是相同属性的筛选只需满足一个, 不同属性的筛选需要全部满足
        var groupedFilterConditions = _filterConditions.GroupBy(c => c.Property);
        if (groupedFilterConditions.Any())
        {
            processedResults = processedResults.Where(result =>
                groupedFilterConditions.All(group =>
                    group.Any(condition => condition.Filter(result))
                )
            );
        }

        // 应用排序条件, 按优先级顺序排序
        var orderedSortConditions = _sortConditions.OrderBy(sc => sc.Priority).ToArray();
        if (orderedSortConditions.Length != 0)
        {
            processedResults = orderedSortConditions[1..].Aggregate(processedResults.OrderBy(x => x, orderedSortConditions[0].Comparer), (acc, sc) => acc.ThenBy(x => x, sc.Comparer));
        }

        // 更新结果列表
        Results = processedResults.ToList().AsReadOnly();
    }
}