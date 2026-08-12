using MineClearance.Core.Interfaces;
using System;
using System.Diagnostics;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏计时器实现类, 负责计时和触发滴答事件
/// </summary>
internal sealed class GameTimer : IGameTimer
{
    /// <summary>
    /// 高精度计时器字段, 用于计算已运行的时间
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <inheritdoc/>
    public DateTime? FirstStartTime { get; private set; }

    /// <inheritdoc/>
    public TimeSpan Elapsed { get => field + _stopwatch.Elapsed; private set; } = TimeSpan.Zero;

    /// <inheritdoc/>
    public void Initial(DateTime startTime, TimeSpan elapsed)
    {
        Debug.Assert(!_stopwatch.IsRunning, "GameTimer should not be running when initializing.");
        FirstStartTime = startTime;
        Elapsed = elapsed;
    }

    /// <inheritdoc/>
    public void Start()
    {
        // 如果计时器已经在运行, 则不需要重新开始
        if (_stopwatch.IsRunning) { return; }

        // 更新第一次开始计时的时间
        FirstStartTime ??= DateTime.Now;

        // 启动计时器
        _stopwatch.Start();
    }

    /// <inheritdoc/>
    public void Pause()
    {
        // 如果计时器没有在运行, 则不需要暂停
        if (!_stopwatch.IsRunning) { return; }

        // 暂停高精度计时器
        _stopwatch.Stop();
    }
}
