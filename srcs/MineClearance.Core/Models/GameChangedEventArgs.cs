using MineClearance.Core.Interfaces;
using System;

namespace MineClearance.Core.Models;

/// <summary>
/// 游戏变更事件参数类, 用于表示游戏变更时的事件数据
/// </summary>
/// <param name="game">当前游戏实例</param>
public sealed class GameChangedEventArgs(IGame? game) : EventArgs
{
    /// <summary>
    /// 获取当前游戏实例
    /// </summary>
    public IGame? Game { get; } = game;
}
