using System;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏计时器接口
/// </summary>
public interface IGameTimer : IDisposable
{
    /// <summary>
    /// 当计时器每次滴答时触发的事件
    /// </summary>
    event EventHandler<Models.Args.GameTimerTickEventArgs>? Tick;

    /// <summary>
    /// 获取计时器已运行的时间
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// 获取计时器是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 重新开始计时, 如果计时器已运行则会重置计时器并重新开始计时
    /// </summary>
    void ReStart();

    /// <summary>
    /// 开始计时, 该方法不会重置计时器, 如果计时器已运行则不会有任何效果
    /// </summary>
    void Start();

    /// <summary>
    /// 暂停计时
    /// </summary>
    void Pause();
}
