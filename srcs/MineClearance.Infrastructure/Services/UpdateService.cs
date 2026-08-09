using Downloader;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.Infrastructure.Services;

/// <summary>
/// 更新服务实现类
/// </summary>
/// <param name="_logger">日志记录器</param>
internal sealed partial class UpdateService(ILogger<UpdateService> _logger) : IUpdateService, IDisposable
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public UpdateState State => (UpdateState)_state;

    /// <inheritdoc/>
    public string? LatestVersion
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(LatestVersion)));
            }
        }
    }

    /// <inheritdoc/>
    public long? TotalBytes
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(TotalBytes)));
            }
        }
    }

    /// <inheritdoc/>
    public long? DownloadedBytes
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(DownloadedBytes)));
            }
        }
    }

    /// <inheritdoc/>
    public double? ProgressPercentage
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(ProgressPercentage)));
            }
        }
    }

    /// <inheritdoc/>
    public double? SpeedBytesPerSecond
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(SpeedBytesPerSecond)));
            }
        }
    }

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
        const int upToDateState = (int)UpdateState.UpToDate;
        const int needUpdateState = (int)UpdateState.NeedUpdate;

        // 只有在空闲状态下才能发起检查更新请求, 否则忽略
        if (Interlocked.CompareExchange(ref _state, checkingState, idleState) is idleState)
        {
            Exception = null;
            PropertyChanged?.Invoke(this, new(nameof(State)));
            LogCheckingForUpdates(author, repository, version);
            _currentVersion = version;
            try
            {
                // 请求 GitHub 仓库最新 release 信息
                using var response = (await _httpClient.GetAsync(
                    $"https://api.github.com/repos/{author}/{repository}/releases/latest", ct))
                    .EnsureSuccessStatusCode();

                // 读取响应内容流并解析 JSON
                await using var contentStream = await response.Content.ReadAsStreamAsync(ct);

                // 解析最新 release 信息
                using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: ct);

                // 提取最新版本号 (tag 形如 v1.0.1, 去掉前导 v)
                string? latestVersion = null;
                if (document.RootElement.TryGetProperty("tag_name", out var tagName))
                {
                    latestVersion = tagName.GetString()?.TrimStart('v');
                }

                // 没有新版本: 清空最新版本信息并置为已是最新
                if (!IsNewerVersion(latestVersion, _currentVersion))
                {
                    LatestVersion = null;
                    _downloadUri = null;
                    TotalBytes = null;
                    _ = Interlocked.Exchange(ref _state, upToDateState);
                    PropertyChanged?.Invoke(this, new(nameof(State)));
                    LogUpToDate(_currentVersion);
                    return;
                }

                // 查找当前平台的更新包资产, 未找到时视为检查失败
                if (!TryFindUpdateAsset(document.RootElement))
                {
                    throw new InvalidOperationException($"{_targetName} is not found in the release assets.");
                }

                // 发现新版本: 保存最新版本信息并置为需要更新
                LatestVersion = latestVersion;
                _ = Interlocked.Exchange(ref _state, needUpdateState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                LogFoundUpdate(latestVersion);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _ = Interlocked.Exchange(ref _state, idleState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
            }
            catch (Exception ex)
            {
                Exception = ex;
                _ = Interlocked.Exchange(ref _state, idleState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                LogCheckingFailed(ex);
            }
        }
    }

    /// <inheritdoc/>
    public async Task DownloadAsync(CancellationToken ct = default)
    {
        const int downloadingState = (int)UpdateState.Downloading;
        const int needUpdateState = (int)UpdateState.NeedUpdate;
        const int completedState = (int)UpdateState.DownloadCompleted;
        const int failedState = (int)UpdateState.DownloadFailed;

        // 只有在需要更新或者下载失败状态下才能发起下载请求, 否则忽略
        if (Interlocked.CompareExchange(ref _state, downloadingState, needUpdateState) is needUpdateState ||
            Interlocked.CompareExchange(ref _state, downloadingState, failedState) is failedState)
        {
            Debug.Assert(
                _downloadUri is not null,
                "Download URI should not be null when downloading."
            );
            PropertyChanged?.Invoke(this, new(nameof(State)));

            // 重置进度信息与异常
            DownloadedBytes = null;
            ProgressPercentage = null;
            SpeedBytesPerSecond = null;
            Exception = null;

            try
            {
                // 更新包已存在且大小与服务器资产一致: 无需重新下载, 直接完成 (恢复已完成的下载)
                if (IsUpdatePackageComplete())
                {
                    _ = Interlocked.Exchange(ref _state, completedState);
                    PropertyChanged?.Invoke(this, new(nameof(State)));
                    LogUpdatePackageAlreadyComplete(Constants.UpdatePackageFilePath);
                    return;
                }

                // 确保更新数据目录存在
                _ = Directory.CreateDirectory(Constants.UpdateDataDirectory);

                // 存在断点文件时先校验版本标识: 与当前要下载的版本不一致时删除断点, 避免跨版本续传
                var tempFilePath = Constants.UpdatePackageFilePath + Constants.DownloadTempFileSuffix;
                if (File.Exists(tempFilePath) && !IsNewVersionFileMatch())
                {
                    File.Delete(tempFilePath);
                }

                // 保存最新版本号到文件, 作为断点文件与完整更新包的版本标识
                File.WriteAllText(Constants.NewVersionFilePath, LatestVersion);

                // 创建下载服务: 启用断点续传, 失败时保留断点文件供下次续传
                _downloadService = new(new DownloadConfiguration
                {
                    ParallelDownload = true,
                    ChunkCount = 4,
                    MaxTryAgainOnFailure = 3,
                    HttpClientTimeout = RequestTimeout,
                    EnableAutoResumeDownload = true,
                    ClearPackageOnCompletionWithFailure = false,
                    FileExistPolicy = FileExistPolicy.Delete,
                    DownloadFileExtension = Constants.DownloadTempFileSuffix
                });

                // 存在断点文件时, Downloader 会自动从断点继续下载
                if (File.Exists(tempFilePath))
                {
                    LogResumingDownload(Constants.UpdatePackageFilePath);
                }

                // 订阅下载事件, 用于更新进度信息
                _downloadService.DownloadStarted += OnDownloadStarted;
                _downloadService.DownloadProgressChanged += OnDownloadProgressChanged;

                // 记录下载开始日志, 并发起下载任务
                LogDownloadStarted(_downloadUri, Constants.UpdatePackageFilePath);
                await _downloadService.DownloadFileTaskAsync(
                    _downloadUri, Constants.UpdatePackageFilePath, ct
                );

                // 用户取消: 保留断点文件供下次续传, 回到需要更新状态
                if (ct.IsCancellationRequested || _downloadService.IsCancelled)
                {
                    _ = Interlocked.Exchange(ref _state, needUpdateState);
                    PropertyChanged?.Invoke(this, new(nameof(State)));
                    LogDownloadCancelled();
                    return;
                }

                _ = Interlocked.Exchange(ref _state, completedState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                LogDownloadCompleted(LatestVersion!);
            }
            catch (OperationCanceledException)
            {
                _ = Interlocked.Exchange(ref _state, needUpdateState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                LogDownloadCancelled();
            }
            catch (Exception ex)
            {
                Exception = ex;
                _ = Interlocked.Exchange(ref _state, failedState);
                PropertyChanged?.Invoke(this, new(nameof(State)));
                LogDownloadFailed(ex);
            }
            finally
            {
                _downloadService?.DownloadStarted -= OnDownloadStarted;
                _downloadService?.DownloadProgressChanged -= OnDownloadProgressChanged;
                if (_downloadService is not null)
                {
                    await _downloadService.DisposeAsync();
                    _downloadService = null;
                }
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
            // 请求取消当前下载, 断点文件保留供下次续传
            try
            {
                _downloadService?.CancelAsync();
            }
            catch { /* 忽略取消下载时的异常 */ }
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

    /// <inheritdoc/>
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
