namespace MineClearance.Models.Enums;

/// <summary>
/// 底部状态栏状态枚举
/// </summary>
internal enum StatusBarState
{
    /// <summary>
    /// 就绪
    /// </summary>
    Ready,

    /// <summary>
    /// 历史记录
    /// </summary>
    History,

    /// <summary>
    /// 准备游戏
    /// </summary>
    Preparing,

    /// <summary>
    /// 等待游戏开始
    /// </summary>
    WaitingStart,

    /// <summary>
    /// 游戏中
    /// </summary>
    InGame,

    /// <summary>
    /// 游戏暂停
    /// </summary>
    Paused,

    /// <summary>
    /// 游戏胜利
    /// </summary>
    GameWon,

    /// <summary>
    /// 游戏失败
    /// </summary>
    GameLost
}