using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.Infrastructure.Services;

/// <summary>
/// 更新服务实现类
/// </summary>
/// <param name="_logger">日志记录器</param>
internal sealed partial class UpdateService(ILogger<UpdateService> _logger) : IUpdateService
{
    /// <summary>
    /// 当前状态的后备 int 字段
    /// </summary>
    private volatile int _state;

    /// <summary>
    /// 当前版本号
    /// </summary>
    private string? _currentVersion;

    /// <summary>
    /// 下载地址
    /// </summary>
    private Uri? _downloadUri;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public UpdateState State => (UpdateState)_state;

    /// <inheritdoc/>
    public string? LatestVersion { get; private set; }

    /// <inheritdoc/>
    public long? TotalBytes { get; private set; }

    /// <inheritdoc/>
    public long? DownloadedBytes => throw new NotImplementedException();

    /// <inheritdoc/>
    public double? ProgressPercentage => throw new NotImplementedException();

    /// <inheritdoc/>
    public double? SpeedBytesPerSecond => throw new NotImplementedException();

    /// <inheritdoc/>
    public Exception? Exception { get; private set; }

    /// <inheritdoc/>
    public UpdateInfo? GetLastUpdateInfoAndCleanUp()
    {
        return BootstrapUpdateHelper.GetLastUpdateInfoAndCleanUp();
    }

    /// <inheritdoc/>
    public async Task CheckNewestAsync(
        string author, string repository, string version,
        CancellationToken ct = default)
    {
        const int idleState = (int)UpdateState.Idle;
        const int checkingState = (int)UpdateState.Checking;

        // 只有在空闲状态下才能发起检查更新请求, 否则忽略
        if (Interlocked.CompareExchange(ref _state, checkingState, idleState) is idleState)
        {
            PropertyChanged?.Invoke(this, new(nameof(State)));
            LogCheckingForUpdates(author, repository, version);
            _currentVersion = version;

            try
            {
                // TODO: 检查更新逻辑
            }
            catch (Exception ex)
            {
                _ = Interlocked.Exchange(ref _state, idleState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                Exception = ex;
            }
        }
    }

    /// <inheritdoc/>
    public async Task DownloadAsync(CancellationToken ct = default)
    {
        const int needUpdateState = (int)UpdateState.NeedUpdate;
        const int downloadingState = (int)UpdateState.Downloading;
        if (Interlocked.CompareExchange(ref _state, downloadingState, needUpdateState) is needUpdateState)
        {
            try
            {
                // TODO: 下载更新逻辑
            }
            catch (Exception ex)
            {
                _ = Interlocked.Exchange(ref _state, needUpdateState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                Exception = ex;
            }
        }
    }

    /// <inheritdoc/>
    public void CancelDownload()
    {
        const int downloadingState = (int)UpdateState.Downloading;
        const int needUpdateState = (int)UpdateState.NeedUpdate;
        if (Interlocked.CompareExchange(ref _state, needUpdateState, downloadingState) is downloadingState)
        {
            // TODO: 取消下载逻辑
        }
    }

    /// <inheritdoc/>
    public void PerformBootstrapUpdateIfNecessary()
    {
        if (State is UpdateState.DownloadCompleted)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(_currentVersion),
                "Current version should not be null when performing bootstrap update."
            );
            try
            {
                BootstrapUpdateHelper.PrepareBootstrapUpdate(_currentVersion);
            }
            catch (Exception ex)
            {
                LogPerformingBootstrapUpdateFailed(ex);
            }
        }
    }
}
