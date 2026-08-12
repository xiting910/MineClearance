using Microsoft.Extensions.Logging.Abstractions;
using MineClearance.Infrastructure.Models;
using MineClearance.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// <see cref="UpdateService"/> 的单元测试, 覆盖状态守卫、初始状态与检查/下载状态机流转
/// </summary>
public sealed class UpdateServiceTests
{
    /// <summary>
    /// 每个测试开始前重置更新数据目录, 避免测试间互相干扰
    /// </summary>
    public UpdateServiceTests()
    {
        var updateDataDir = new DirectoryInfo(Constants.UpdateDataDirectory);
        if (updateDataDir.Exists)
        {
            updateDataDir.Delete(recursive: true);
        }
        updateDataDir.Create();
    }

    [Fact]
    public void 构造_初始状态为空闲()
    {
        using var service = CreateService();

        Assert.Equal(UpdateState.Idle, service.State);
    }

    [Fact]
    public void LatestVersion_尚未检查更新_抛出InvalidOperationException()
    {
        using var service = CreateService();

        _ = Assert.Throws<InvalidOperationException>(() => service.LatestVersion);
    }

    [Fact]
    public void Exception_未发生异常_抛出InvalidOperationException()
    {
        using var service = CreateService();

        _ = Assert.Throws<InvalidOperationException>(() => service.Exception);
    }

    [Fact]
    public async Task DownloadAsync_非需要更新状态_直接返回()
    {
        using var service = CreateService();

        await service.DownloadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(UpdateState.Idle, service.State);
    }

    [Fact]
    public void CancelDownload_非下载状态_不抛出异常()
    {
        using var service = CreateService();

        service.CancelDownload();
    }

    [Fact]
    public void PerformBootstrapUpdateIfNecessary_非下载完成状态_不抛出异常()
    {
        using var service = CreateService();

        service.PerformBootstrapUpdateIfNecessary();
    }

    [Fact]
    public void GetLastUpdateInfoAndCleanUp_无更新信息_返回null()
    {
        using var service = CreateService();

        Assert.Null(service.GetLastUpdateInfoAndCleanUp());
    }

    [Fact]
    public void Dispose_释放资源_不抛出异常()
    {
        var service = CreateService();

        service.Dispose();
    }

    [Fact]
    public async Task CheckNewestAsync_最新版本与当前相同_进入UpToDate状态()
    {
        var service = CreateService(CreateHandler(CreateReleaseJson("v2.0.0", 1024)));

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "2.0.0", TestContext.Current.CancellationToken
        );

        Assert.Equal(UpdateState.UpToDate, service.State);
        Assert.Equal("2.0.0", service.LatestVersion);
        Assert.Equal(0, service.TotalBytes);
    }

    [Fact]
    public async Task CheckNewestAsync_发现新版本且资产匹配_进入NeedUpdate状态()
    {
        var service = CreateService(CreateHandler(CreateReleaseJson("v2.0.0", 1024)));

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );

        Assert.Equal(UpdateState.NeedUpdate, service.State);
        Assert.Equal("2.0.0", service.LatestVersion);
        Assert.Equal(1024, service.TotalBytes);
    }

    [Fact]
    public async Task CheckNewestAsync_网络异常_进入CheckFailed状态并重置信息()
    {
        var handler = new Mock<HttpMessageHandler>();
        _ = handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network error"));
        var service = CreateService(handler.Object);

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );

        Assert.Equal(UpdateState.CheckFailed, service.State);
        _ = Assert.IsType<HttpRequestException>(service.Exception);
        Assert.Equal(0, service.TotalBytes);
        _ = Assert.Throws<InvalidOperationException>(() => service.LatestVersion);
    }

    [Fact]
    public async Task CheckNewestAsync_发布资产不含当前平台包_进入CheckFailed状态()
    {
        var service = CreateService(CreateHandler(CreateReleaseJson("v2.0.0", 0, assets: false)));

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );

        Assert.Equal(UpdateState.CheckFailed, service.State);
        var exception = Assert.IsType<InvalidOperationException>(service.Exception);
        Assert.Contains("is not found in the release assets", exception.Message);
    }

    [Fact]
    public async Task CheckNewestAsync_检查被取消_恢复到检查前的状态()
    {
        var service = CreateService(CreateHandler(CreateReleaseJson("v2.0.0", 1024)));

        await service.CheckNewestAsync("xiting910", "MineClearance", "1.0.0", new(true));

        Assert.Equal(UpdateState.Idle, service.State);
    }

    [Fact]
    public async Task CheckNewestAsync_从NeedUpdate状态取消检查_恢复到NeedUpdate状态()
    {
        var service = CreateService(CreateHandler(CreateReleaseJson("v2.0.0", 1024)));

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );
        Assert.Equal(UpdateState.NeedUpdate, service.State);

        await service.CheckNewestAsync("xiting910", "MineClearance", "1.0.0", new(true));
        Assert.Equal(UpdateState.NeedUpdate, service.State);
    }

    [Fact]
    public async Task CheckNewestAsync_检查进行中再次调用_直接返回()
    {
        var tcs = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new Mock<HttpMessageHandler>();
        _ = handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(tcs.Task);
        var service = CreateService(handler.Object);

        var firstCheck = service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );
        Assert.Equal(UpdateState.Checking, service.State);

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "9.9.9", TestContext.Current.CancellationToken
        );

        handler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
        tcs.SetResult(CreateResponse(CreateReleaseJson("v1.0.0", 1024)));
        await firstCheck;
        Assert.Equal(UpdateState.UpToDate, service.State);
    }

    [Fact]
    public async Task 下载时完整包已存在_直接进入DownloadCompleted并保持到再次检查()
    {
        const long size = 1024;
        File.WriteAllBytes(Constants.UpdatePackageFilePath, new byte[size]);
        File.WriteAllText(Constants.NewVersionFilePath, "2.0.0");
        var service = CreateService(CreateHandler(CreateReleaseJson("v2.0.0", size)));

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );
        Assert.Equal(UpdateState.NeedUpdate, service.State);

        await service.DownloadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(UpdateState.DownloadCompleted, service.State);

        await service.CheckNewestAsync(
            "xiting910", "MineClearance", "1.0.0", TestContext.Current.CancellationToken
        );
        Assert.Equal(UpdateState.DownloadCompleted, service.State);
    }

    /// <summary>
    /// 创建使用 <see cref="NullLogger{T}"/> 的更新服务实例
    /// </summary>
    /// <returns>更新服务实例</returns>
    private static UpdateService CreateService()
    {
        return new(NullLogger<UpdateService>.Instance);
    }

    /// <summary>
    /// 创建使用指定消息处理程序的更新服务实例
    /// </summary>
    /// <param name="handler">HTTP 消息处理程序</param>
    /// <returns>更新服务实例</returns>
    private static UpdateService CreateService(HttpMessageHandler handler)
    {
        return new(NullLogger<UpdateService>.Instance, new(handler));
    }

    /// <summary>
    /// 创建返回指定 JSON 的 HTTP 消息处理程序, 每次调用返回新的响应实例
    /// </summary>
    /// <param name="json">响应内容 JSON</param>
    /// <returns>HTTP 消息处理程序</returns>
    private static HttpMessageHandler CreateHandler(string json)
    {
        var handler = new Mock<HttpMessageHandler>();
        _ = handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(CreateResponse(json)));
        return handler.Object;
    }

    /// <summary>
    /// 创建 GitHub release 响应的 JSON, 包含当前平台在内的更新包资产
    /// </summary>
    /// <param name="tagName">版本标签</param>
    /// <param name="size">更新包大小</param>
    /// <param name="assets">是否包含当前平台的更新包资产</param>
    /// <returns>GitHub release 响应的 JSON</returns>
    private static string CreateReleaseJson(string tagName, long size, bool assets = true)
    {
        return assets
            ? $$"""
                {
                  "tag_name": "{{tagName}}",
                  "assets": [
                    { "name": "MineClearance-win-x64.zip", "browser_download_url": "https://example.com/MineClearance-win-x64.zip", "size": {{size}} },
                    { "name": "MineClearance-linux-x64.zip", "browser_download_url": "https://example.com/MineClearance-linux-x64.zip", "size": {{size}} }
                  ]
                }
                """
            : $$""" { "tag_name": "{{tagName}}", "assets": [] } """;
    }

    /// <summary>
    /// 创建 HTTP 响应
    /// </summary>
    /// <param name="json">响应内容 JSON</param>
    /// <returns>HTTP 响应</returns>
    private static HttpResponseMessage CreateResponse(string json)
    {
        return new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}
