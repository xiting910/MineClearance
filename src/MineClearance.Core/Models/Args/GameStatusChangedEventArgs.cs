using System;

namespace MineClearance.Core.Models.Args;

/// <summary>
/// 游戏状态变更事件参数类, 用于表示游戏状态变更时的事件数据
/// </summary>
/// <param name="status">变更后的游戏状态</param>
public sealed class GameStatusChangedEventArgs(Enums.GameStatus status) : EventArgs
{
    /// <summary>
    /// 获取变更后的游戏状态
    /// </summary>
    public Enums.GameStatus Status { get; } = status;
}
