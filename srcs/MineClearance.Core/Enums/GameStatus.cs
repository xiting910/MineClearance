namespace MineClearance.Core.Enums;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameStatus
{
    /// <summary>
    /// 等待开始 (已创建等待玩家首次点击)
    /// </summary>
    WaitingStarted,

    /// <summary>
    /// 游戏进行中
    /// </summary>
    InProgress,

    /// <summary>
    /// 已暂停
    /// </summary>
    Paused,

    /// <summary>
    /// 游戏胜利
    /// </summary>
    Won,

    /// <summary>
    /// 游戏失败
    /// </summary>
    Lost
}
