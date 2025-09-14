using MineClearance.Models.Enums;

namespace MineClearance.Models;

/// <summary>
/// 游戏结果比较器, 用于根据不同属性进行排序
/// </summary>
/// <param name="propertyName">属性名称</param>
/// <param name="sortOrder">排序顺序</param>
internal sealed class GameResultComparer(string propertyName, SortOrder sortOrder) : IComparer<GameResult>
{
    /// <summary>
    /// 属性名称
    /// </summary>
    private readonly string _propertyName = propertyName;

    /// <summary>
    /// 排序顺序
    /// </summary>
    private readonly SortOrder _sortOrder = sortOrder;

    /// <summary>
    /// 比较两个游戏结果对象
    /// </summary>
    /// <param name="x">第一个游戏结果对象</param>
    /// <param name="y">第二个游戏结果对象</param>
    /// <returns>比较结果</returns>
    /// <exception cref="ArgumentException">如果属性名不支持</exception>
    public int Compare(GameResult? x, GameResult? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        var result = _propertyName switch
        {
            "难度" or nameof(GameResult.Difficulty) => x.Difficulty == DifficultyLevel.Custom && y.Difficulty != DifficultyLevel.Custom ? -1 : x.Difficulty != DifficultyLevel.Custom && y.Difficulty == DifficultyLevel.Custom ? 1 : x.Difficulty.CompareTo(y.Difficulty),
            "开始时间" or nameof(GameResult.StartTime) => x.StartTime.CompareTo(y.StartTime),
            "用时" or nameof(GameResult.Duration) => x.Duration.CompareTo(y.Duration),
            "结果" or nameof(GameResult.IsWin) => x.IsWin.CompareTo(y.IsWin),
            "完成度" or nameof(GameResult.Completion) => x.Completion == y.Completion ? 0 : x.Completion is null ? 1 : y.Completion is null ? -1 : x.Completion.Value.CompareTo(y.Completion.Value),
            _ => throw new ArgumentException($"不支持的属性名: {_propertyName}"),
        };
        return _sortOrder == SortOrder.Ascending ? result : -result;
    }
}