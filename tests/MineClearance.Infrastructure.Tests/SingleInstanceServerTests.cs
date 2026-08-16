using System.IO.Pipes;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// <see cref="SingleInstanceServer"/> 的单元测试, 覆盖单实例创建、激活请求传递与等待循环退出
/// </summary>
public sealed class SingleInstanceServerTests
{
    [Fact]
    public void TryCreate_首次创建_返回成功()
    {
        var pipeName = CreatePipeName();

        var created = SingleInstanceServer.TryCreate(pipeName, out var server);

        Assert.True(created);
        Assert.NotNull(server);
        server.Dispose();
    }

    [Fact]
    public void TryCreate_已有实例在运行_返回失败()
    {
        using var server = CreateServer(out var pipeName);

        var created = SingleInstanceServer.TryCreate(pipeName, out var second);

        Assert.False(created);
        Assert.Null(second);
    }

    [Fact]
    public async Task SendActivateRequest_已有实例在等待_触发激活回调()
    {
        using var server = CreateServer(out var pipeName);
        var (tcs, cts, waitTask) = StartWaiting(server);

        SingleInstanceServer.SendActivateRequest(pipeName);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cts.Cancel();
        await waitTask;
    }

    [Fact]
    public async Task WaitForActivationRequestsAsync_收到非激活字节_不触发回调()
    {
        using var server = CreateServer(out var pipeName);
        var (tcs, cts, waitTask) = StartWaiting(server);

        using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
        {
            client.Connect(TimeSpan.FromSeconds(5));
            client.WriteByte(1);
        }

        await Task.Delay(300, TestContext.Current.CancellationToken);
        Assert.False(tcs.Task.IsCompleted);

        cts.Cancel();
        await waitTask;
    }

    [Fact]
    public async Task WaitForActivationRequestsAsync_取消令牌_退出等待循环()
    {
        using var server = CreateServer(out _);
        using var cts = new CancellationTokenSource();

        var waitTask = server.WaitForActivationRequestsAsync(() => { }, cts.Token);
        cts.Cancel();

        await waitTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WaitForActivationRequestsAsync_客户端断开_继续等待后续激活请求()
    {
        using var server = CreateServer(out var pipeName);
        var (tcs, cts, waitTask) = StartWaiting(server);

        // 第一个客户端连接后不发激活请求直接断开
        using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
        {
            client.Connect(TimeSpan.FromSeconds(5));
        }

        // 等待服务器处理完客户端断开并重新监听, 避免后续连接与断开处理竞速
        await Task.Delay(300, TestContext.Current.CancellationToken);

        // 第二个客户端发送激活请求
        SingleInstanceServer.SendActivateRequest(pipeName);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cts.Cancel();
        await waitTask;
    }

    /// <summary>
    /// 创建唯一的管道名称, 避免测试间管道冲突
    /// </summary>
    /// <returns>管道名称</returns>
    private static string CreatePipeName()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 创建使用唯一管道名称的单实例服务器
    /// </summary>
    /// <param name="pipeName">创建的管道名称</param>
    /// <returns>单实例服务器</returns>
    private static SingleInstanceServer CreateServer(out string pipeName)
    {
        pipeName = CreatePipeName();
        return SingleInstanceServer.TryCreate(pipeName, out var server)
            ? server
            : throw new InvalidOperationException("单实例服务器创建失败");
    }

    /// <summary>
    /// 启动激活请求等待任务
    /// </summary>
    /// <param name="server">单实例服务器</param>
    /// <returns>激活回调完成源, 取消令牌源和等待任务</returns>
    private static (TaskCompletionSource ActivationTcs, CancellationTokenSource Cts, Task WaitTask)
    StartWaiting(SingleInstanceServer server)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cts = new CancellationTokenSource();
        var waitTask = server.WaitForActivationRequestsAsync(() => tcs.TrySetResult(), cts.Token);
        return (tcs, cts, waitTask);
    }
}
