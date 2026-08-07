using System.ComponentModel;

namespace MineClearance.Core.Enums;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameStatus
{
    /// <summary>
    /// 等待开始 (已创建等待玩家首次点击)
    /// </summary>
    [Description("等待开始")]
    WaitingStarted,

    /// <summary>
    /// 游戏进行中
    /// </summary>
    [Description("进行中")]
    InProgress,

    /// <summary>
    /// 已暂停
    /// </summary>
    [Description("已暂停")]
    Paused,

    /// <summary>
    /// 游戏胜利
    /// </summary>
    [Description("胜利")]
    Won,

    /// <summary>
    /// 游戏失败
    /// </summary>
    [Description("失败")]
    Lost
}
