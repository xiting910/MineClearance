using MineClearance.Models.Enums;

namespace MineClearance.Models;

/// <summary>
/// 表示扫雷游戏的结果
/// </summary>
/// <param name="difficulty">游戏难度级别</param>
/// <param name="startTime">游戏开始时间</param>
/// <param name="duration">游戏持续时间</param>
/// <param name="isWin">游戏是否获胜</param>
/// <param name="completion">游戏完成度, 取值范围 [0, 100), null 表示胜利</param>
/// <param name="boardWidth">棋盘宽度</param>
/// <param name="boardHeight">棋盘高度</param>
/// <param name="mineCount">地雷总数</param>
internal sealed class GameResult(DifficultyLevel difficulty, DateTime startTime, TimeSpan duration, bool isWin, double? completion = null, int? boardWidth = null, int? boardHeight = null, int? mineCount = null) : IEquatable<GameResult>
{
    /// <summary>
    /// 游戏的难度级别
    /// </summary>
    public DifficultyLevel Difficulty { get; private init; } = difficulty;

    /// <summary>
    /// 游戏的开始时间
    /// </summary>
    public DateTime StartTime { get; private init; } = startTime;

    /// <summary>
    /// 游戏时间
    /// </summary>
    public TimeSpan Duration { get; private init; } = duration;

    /// <summary>
    /// 游戏的结果(是否获胜)
    /// </summary>
    public bool IsWin { get; private init; } = isWin;

    /// <summary>
    /// 游戏完成度, 取值范围 [0, 100), null 表示胜利
    /// </summary>
    public double? Completion { get; private init; } = isWin ? null : completion;

    /// <summary>
    /// 棋盘宽度
    /// </summary>
    public int? BoardWidth { get; private init; } = difficulty == DifficultyLevel.Custom ? boardWidth : null;

    /// <summary>
    /// 棋盘高度
    /// </summary>
    public int? BoardHeight { get; private init; } = difficulty == DifficultyLevel.Custom ? boardHeight : null;

    /// <summary>
    /// 地雷数量
    /// </summary>
    public int? MineCount { get; private init; } = difficulty == DifficultyLevel.Custom ? mineCount : null;

    /// <summary>
    /// 重写 ToString 方法, 返回游戏结果的字符串表示
    /// </summary>
    /// <returns>游戏结果的字符串表示</returns>
    public override string ToString()
    {
        // 结果信息
        var resultInfo = IsWin ? "胜利" : $"失败, 完成度: {Completion:0.##}%";

        // 自定义难度信息
        var customInfo = Difficulty == DifficultyLevel.Custom ? $", 大小: {BoardWidth}x{BoardHeight}, 地雷数: {MineCount}" : "";

        // 拼接所有信息
        return $"[{StartTime}] 难度: {Difficulty}, 用时: {Duration}, 结果: {resultInfo}{customInfo}]";
    }

    /// <summary>
    /// 重写==操作符, 用于比较两个游戏结果是否相等
    /// </summary>
    /// <param name="left">左侧游戏结果</param>
    /// <param name="right">右侧游戏结果</param>
    /// <returns>是否相等</returns>
    public static bool operator ==(GameResult? left, GameResult? right)
    {
        return (left is null && right is null) || (left is not null && left.Equals(right));
    }

    /// <summary>
    /// 重写!=操作符, 用于比较两个游戏结果是否不相等
    /// </summary>
    /// <param name="left">左侧游戏结果</param>
    /// <param name="right">右侧游戏结果</param>
    /// <returns>是否不相等</returns>
    public static bool operator !=(GameResult? left, GameResult? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 判断当前游戏结果是否与另一个游戏结果相等
    /// </summary>
    /// <param name="other">另一个游戏结果</param>
    /// <returns>是否相等</returns>
    public bool Equals(GameResult? other)
    {
        return ReferenceEquals(this, other) || (other is not null && StartTime == other.StartTime);
    }

    /// <summary>
    /// 重写Equals方法, 用于比较两个游戏结果是否相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as GameResult);
    }

    /// <summary>
    /// 获取对象的哈希码
    /// </summary>
    /// <returns>对象的哈希码</returns>
    public override int GetHashCode()
    {
        return StartTime.GetHashCode();
    }
}