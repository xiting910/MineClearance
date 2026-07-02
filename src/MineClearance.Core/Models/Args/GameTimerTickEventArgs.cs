using System;

namespace MineClearance.Core.Models.Args;

/// <summary>
/// 游戏计时器滴答事件参数类, 用于表示游戏计时器每次滴答时的事件数据
/// </summary>
/// <param name="elapsed">计时器已运行的时间</param>
public sealed class GameTimerTickEventArgs(TimeSpan elapsed) : EventArgs
{
    /// <summary>
    /// 获取计时器已运行的时间
    /// </summary>
    public TimeSpan Elapsed { get; } = elapsed;
}
