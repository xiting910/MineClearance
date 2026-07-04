using System;
using System.ComponentModel;
using System.Diagnostics;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏计时器实现类, 负责计时和触发滴答事件
/// </summary>
internal sealed class GameTimer : Interfaces.IGameTimer
{
    /// <summary>
    /// 高精度计时器字段, 用于计算已运行的时间
    /// </summary>
    private readonly Stopwatch _stopwatch;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public DateTime? FirstStartTime { get; private set; }

    /// <inheritdoc/>
    public TimeSpan Elapsed { get => field + _stopwatch.Elapsed; private set; }

    /// <inheritdoc/>
    public bool IsRunning => _stopwatch.IsRunning;

    /// <summary>
    /// 构造函数
    /// </summary>
    public GameTimer()
    {
        Elapsed = TimeSpan.Zero;
        _stopwatch = new();
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        // 触发 Elapsed 属性的 PropertyChanged 事件, 以便通知 UI 更新显示的时间
        PropertyChanged?.Invoke(this, new(nameof(Elapsed)));
    }

    /// <inheritdoc/>
    public void Pause()
    {
        // 如果计时器没有在运行, 则不需要暂停
        if (!_stopwatch.IsRunning) { return; }

        // 暂停高精度计时器
        _stopwatch.Stop();

        // 通知 IsRunning 属性已更改
        PropertyChanged?.Invoke(this, new(nameof(IsRunning)));
    }

    /// <inheritdoc/>
    public void Start()
    {
        // 如果计时器已经在运行, 则不需要重新开始
        if (_stopwatch.IsRunning) { return; }

        // 更新第一次开始计时的时间
        FirstStartTime ??= DateTime.Now;

        // 启动高精度计时器
        _stopwatch.Start();

        // 通知 IsRunning 属性已更改
        PropertyChanged?.Invoke(this, new(nameof(IsRunning)));
    }

    /// <inheritdoc/>
    public void ReStart()
    {
        // 更新第一次开始计时的时间
        FirstStartTime ??= DateTime.Now;

        // 重置高精度计时器并重新开始计时
        _stopwatch.Restart();

        // 通知 IsRunning 属性已更改
        PropertyChanged?.Invoke(this, new(nameof(IsRunning)));
    }

    /// <inheritdoc/>
    public void SetInitialTime(TimeSpan initialTime)
    {
        Debug.Assert(!_stopwatch.IsRunning, "Cannot set initial time while the timer is running.");
        Elapsed = initialTime;
    }
}
