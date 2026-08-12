using MineClearance.Core.Services;

namespace MineClearance.Core.Tests;

/// <summary>
/// GameTimer 的单元测试, 覆盖计时启停、首次开始时间和已用时累计
/// </summary>
public sealed class GameTimerTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    [Fact]
    public void Start_尚未开始_记录首次开始时间()
    {
        var timer = new GameTimer();

        timer.Start();

        _ = Assert.NotNull(timer.FirstStartTime);
    }

    [Fact]
    public void Start_重复调用_首次开始时间保持不变()
    {
        var timer = new GameTimer();

        timer.Start();
        var firstStartTime = timer.FirstStartTime;
        timer.Start();

        Assert.Equal(firstStartTime, timer.FirstStartTime);
    }

    [Fact]
    public void Start_暂停后再次调用_首次开始时间保持不变()
    {
        var timer = new GameTimer();

        timer.Start();
        var firstStartTime = timer.FirstStartTime;
        timer.Pause();
        timer.Start();

        Assert.Equal(firstStartTime, timer.FirstStartTime);
    }

    [Fact]
    public void Pause_运行中暂停_已用时不再增长()
    {
        var timer = new GameTimer();
        timer.Start();
        Thread.Sleep(50);
        timer.Pause();

        var elapsedWhenPaused = timer.Elapsed;
        Thread.Sleep(50);

        Assert.Equal(elapsedWhenPaused, timer.Elapsed);
    }

    [Fact]
    public void Pause_未在运行_不抛异常()
    {
        var timer = new GameTimer();

        timer.Pause();

        // 暂停未运行的计时器后, 已用时保持为零
        Assert.Equal(TimeSpan.Zero, timer.Elapsed);
    }

    [Fact]
    public void Initial_设置开始时间和已用时()
    {
        var timer = new GameTimer();

        timer.Initial(StartTime, TimeSpan.FromMinutes(5));

        Assert.Equal(StartTime, timer.FirstStartTime);
        Assert.Equal(TimeSpan.FromMinutes(5), timer.Elapsed);
    }

    [Fact]
    public void Initial_之后启动_已用时从初始值继续累计()
    {
        var timer = new GameTimer();
        timer.Initial(StartTime, TimeSpan.FromMinutes(5));

        timer.Start();
        Thread.Sleep(50);
        timer.Pause();

        Assert.True(timer.Elapsed > TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Start_暂停后再次启动_计时继续累计()
    {
        var timer = new GameTimer();
        timer.Start();
        Thread.Sleep(50);
        timer.Pause();
        var elapsedWhenPaused = timer.Elapsed;

        timer.Start();
        Thread.Sleep(50);
        timer.Pause();

        Assert.True(timer.Elapsed > elapsedWhenPaused);
    }
}
