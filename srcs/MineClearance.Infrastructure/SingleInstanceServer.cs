using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.Infrastructure;

/// <summary>
/// 应用程序单实例服务器类
/// </summary>
public sealed class SingleInstanceServer : IDisposable
{
    /// <summary>
    /// 管道名称, 用于连接异常时重建管道实例
    /// </summary>
    private readonly string _pipeName;

    /// <summary>
    /// 命名管道服务端, 用于保证应用程序的单实例运行和跨进程通信
    /// </summary>
    private NamedPipeServerStream _server;

    /// <summary>
    /// 私有构造函数, 仅允许通过 <see cref="TryCreate"/> 创建实例
    /// </summary>
    /// <param name="pipeName">管道名称</param>
    private SingleInstanceServer(string pipeName)
    {
        _pipeName = pipeName;
        _server = new(pipeName);
    }

    /// <summary>
    /// 循环等待激活请求, 每次收到请求时调用回调
    /// </summary>
    /// <param name="onActivated">收到激活请求时的回调</param>
    /// <param name="token">取消令牌</param>
    public async Task WaitForActivationRequestsAsync(Action onActivated, CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                try
                {
                    await _server.WaitForConnectionAsync(token);
                    if (_server.ReadByte() == Constants.ActivateRequestByte)
                    {
                        onActivated();
                    }
                }
                finally
                {
                    try { _server.Disconnect(); } catch (InvalidOperationException) { }
                }
            }
            catch (IOException)
            {
                _server.Dispose();
                _server = new(_pipeName);
            }
            catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
            {
                break;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 尝试创建单实例服务器
    /// </summary>
    /// <param name="pipeName">管道名称</param>
    /// <param name="server">创建成功时返回的单实例服务器</param>
    /// <returns><see langword="true"/> 表示创建成功, <see langword="false"/> 表示已有实例在运行</returns>
    public static bool TryCreate(string pipeName, [MaybeNullWhen(false)] out SingleInstanceServer server)
    {
        try
        {
            server = new(pipeName);
            return true;
        }
        catch (IOException)
        {
            server = default;
            return false;
        }
    }

    /// <summary>
    /// 发送激活请求给已有实例, 请求其激活并显示主窗口
    /// </summary>
    /// <param name="pipeName">管道名称</param>
    public static void SendActivateRequest(string pipeName)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(TimeSpan.FromSeconds(Constants.MaxWaitTimeForActivationRequest));
            client.WriteByte(Constants.ActivateRequestByte);
        }
        catch { /* 激活请求发送失败时忽略 */ }
    }
}
