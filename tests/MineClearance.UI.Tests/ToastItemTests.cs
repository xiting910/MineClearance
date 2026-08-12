using MineClearance.UI.Models;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="ToastItem"/> 的单元测试, 覆盖剩余时间扣减, 暂停恢复与点击回调
/// </summary>
public sealed class ToastItemTests
{
    /// <summary>
    /// 测试用的固定显示时长
    /// </summary>
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);

    [Fact]
    public void 构造_初始进度为满比例()
    {
        var item = new ToastItem("提示", Duration);

        Assert.Equal(Constants.MaxRatio, item.Progress);
    }

    [Fact]
    public void Tick_经过部分时长_按比例扣减进度()
    {
        var item = new ToastItem("提示", Duration);

        Assert.False(item.Tick(TimeSpan.FromSeconds(1)));
        Assert.Equal(0.8, item.Progress);
    }

    [Fact]
    public void Tick_时间刚好耗尽_返回true且进度为0()
    {
        var item = new ToastItem("提示", Duration);

        Assert.True(item.Tick(Duration));
        Assert.Equal(0, item.Progress);
    }

    [Fact]
    public void Tick_超过总时长_返回true且进度钳制为0()
    {
        var item = new ToastItem("提示", Duration);

        Assert.True(item.Tick(TimeSpan.FromSeconds(10)));
        Assert.Equal(0, item.Progress);
    }

    [Fact]
    public void Tick_悬停暂停期间_不扣减剩余时间()
    {
        var item = new ToastItem("提示", Duration);
        item.Pause();

        Assert.False(item.Tick(TimeSpan.FromSeconds(1)));
        Assert.Equal(Constants.MaxRatio, item.Progress);
    }

    [Fact]
    public void Tick_暂停后恢复_继续扣减剩余时间()
    {
        var item = new ToastItem("提示", Duration);
        item.Pause();
        _ = item.Tick(TimeSpan.FromSeconds(1));
        item.Resume();

        Assert.False(item.Tick(TimeSpan.FromSeconds(1)));
        Assert.Equal(0.8, item.Progress);
    }

    [Fact]
    public void Pause_设置暂停标记()
    {
        var item = new ToastItem("提示", Duration);

        item.Pause();

        Assert.True(item.IsPaused);
    }

    [Fact]
    public void Resume_清除暂停标记()
    {
        var item = new ToastItem("提示", Duration);
        item.Pause();

        item.Resume();

        Assert.False(item.IsPaused);
    }

    [Fact]
    public void InvokeClick_有回调_执行回调()
    {
        var invoked = false;
        var item = new ToastItem("提示", Duration, () => invoked = true);

        item.InvokeClick();

        Assert.True(invoked);
    }

    [Fact]
    public void InvokeClick_无回调_不抛出异常()
    {
        var item = new ToastItem("提示", Duration);

        item.InvokeClick();
    }

    [Fact]
    public void InvokeClick_回调抛出异常_被吞掉()
    {
        var item = new ToastItem("提示", Duration, () => throw new InvalidOperationException());

        item.InvokeClick();
    }
}
