using MineClearance.Models.Enums;

namespace MineClearance.UI;

/// <summary>
/// UI 方法类, 提供一些 UI 相关的常用方法
/// </summary>
internal static class UIMethods
{
    /// <summary>
    /// 根据难度返回对应的文本
    /// </summary>
    /// <param name="difficulty">难度枚举值</param>
    /// <returns>返回对应的难度文本</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果难度未知则抛出异常</exception>
    public static string GetDifficultyText(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => "简单",
            DifficultyLevel.Medium => "普通",
            DifficultyLevel.Hard => "困难",
            DifficultyLevel.Hell => "地狱",
            DifficultyLevel.Custom => "自定义",
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), "未知的难度")
        };
    }

    /// <summary>
    /// 根据当前要排序的游戏结果属性获取对应的优先级
    /// </summary>
    /// <param name="propertyName">要排序的属性名称</param>
    /// <returns>返回对应的优先级</returns>
    /// <exception cref="ArgumentException">如果属性名不支持</exception>
    public static int GetSortPriority(string propertyName)
    {
        return propertyName switch
        {
            "难度" or "Difficulty" => 1,
            "开始时间" or "StartTime" => 4,
            "用时" or "Duration" => 3,
            "结果" or "IsWin" => 0,
            "完成度" or "Completion" => 2,
            _ => throw new ArgumentException($"不支持的属性名: {propertyName}", nameof(propertyName))
        };
    }
}