using MineClearance.Infrastructure.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.Infrastructure;

/// <summary>
/// 更新服务接口
/// </summary>
public interface IUpdateService : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 获取当前状态
    /// </summary>
    UpdateState State { get; }

    /// <summary>
    /// 获取最新版本号
    /// </summary>
    /// <exception cref="InvalidOperationException">如果在检查最新版本之前访问此属性, 则抛出异常</exception>
    string LatestVersion { get; }

    /// <summary>
    /// 获取总字节数
    /// </summary>
    long TotalBytes { get; }

    /// <summary>
    /// 获取当前已下载字节数
    /// </summary>
    long DownloadedBytes { get; }

    /// <summary>
    /// 获取当前下载进度百分比
    /// </summary>
    double ProgressPercentage { get; }

    /// <summary>
    /// 获取当前下载速度
    /// </summary>
    double SpeedBytesPerSecond { get; }

    /// <summary>
    /// 检查或下载失败时发生的异常
    /// </summary>
    /// <exception cref="InvalidOperationException">如果在没有发生异常的情况下访问此属性, 则抛出异常</exception>
    Exception Exception { get; }

    /// <summary>
    /// 获取上次更新的更新信息并清理残留
    /// </summary>
    /// <returns>上次更新的更新信息</returns>
    UpdateInfo? GetLastUpdateInfoAndCleanUp();

    /// <summary>
    /// 检查最新版本并更新属性值
    /// </summary>
    /// <param name="author">GitHub 仓库作者</param>
    /// <param name="repository">GitHub 仓库名</param>
    /// <param name="version">当前版本号</param>
    /// <param name="ct">取消令牌</param>
    Task CheckNewestAsync(string author, string repository, string version, CancellationToken ct = default);

    /// <summary>
    /// 下载更新
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task DownloadAsync(CancellationToken ct = default);

    /// <summary>
    /// 取消当前下载
    /// </summary>
    void CancelDownload();

    /// <summary>
    /// 如果必要, 执行引导更新
    /// </summary>
    void PerformBootstrapUpdateIfNecessary();
}
