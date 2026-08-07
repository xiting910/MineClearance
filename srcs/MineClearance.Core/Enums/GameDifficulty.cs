using System.ComponentModel;

namespace MineClearance.Core.Enums;

/// <summary>
/// 游戏难度级别枚举
/// </summary>
public enum GameDifficulty
{
    /// <summary>
    /// 初级: 9×9, 10 雷, 地雷率: 12.34568%
    /// </summary>
    [Description("初级")]
    Beginner,

    /// <summary>
    /// 中级: 16×16, 40 雷, 地雷率: 15.625%
    /// </summary>
    [Description("中级")]
    Intermediate,

    /// <summary>
    /// 高级: 16×30, 99 雷, 地雷率: 20.625%
    /// </summary>
    [Description("高级")]
    Expert,

    /// <summary>
    /// 大师: 30×50, 309 雷, 地雷率: 20.6%
    /// </summary>
    [Description("大师")]
    Master,

    /// <summary>
    /// 自定义: 由玩家指定行列数和雷数, 并允许玩家指定使用的随机种子
    /// </summary>
    [Description("自定义")]
    Custom
}
