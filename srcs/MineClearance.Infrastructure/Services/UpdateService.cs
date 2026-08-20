using Downloader;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.Infrastructure.Services;

/// <summary>
/// 更新服务实现类
/// </summary>
/// <param name="_logger">日志记录器</param>
/// <param name="httpClient">用于检查更新的 <see cref="HttpClient"/> 实例, 如果为 null 则使用默认配置创建</param>
internal sealed partial class UpdateService(
    ILogger<UpdateService> _logger,
    HttpClient? httpClient = null
) : IUpdateService
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public UpdateState State
    {
        get => Interlocked.CompareExchange(ref field, default, default);
        private set
        {
            if (Interlocked.Exchange(ref field, value) != value)
            {
                PropertyChanged?.Invoke(this, new(nameof(State)));
            }
        }
    }

    /// <inheritdoc/>
    [AllowNull]
    public string LatestVersion
    {
        get => field ?? throw new InvalidOperationException(
            $"{nameof(LatestVersion)} is not available before checking for updates."
        );
        private set;
    }

    /// <inheritdoc/>
    public long TotalBytes { get; private set; }

    /// <inheritdoc/>
    public long DownloadedBytes
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
    public double ProgressPercentage
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
    public double SpeedBytesPerSecond
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
    [AllowNull]
    public Exception Exception
    {
        get => field ?? throw new InvalidOperationException(
            $"{nameof(Exception)} is not available when there is no exception."
        );
        private set;
    }

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
        // 缓存之前的状态
        var previousState = State;
        if (previousState is UpdateState.Checking or UpdateState.Downloading)
        {
            return;
        }

        // 清空异常信息, 并设置为检查中状态
        Exception = null;
        State = UpdateState.Checking;
        LogCheckingForUpdates(author, repository, version);

        // 保存当前版本号
        _currentVersion = version;

        try
        {
            // 请求 GitHub 仓库最新 release 信息
            using var response = (await _httpClient.GetAsync(
                $"https://api.github.com/repos/{author}/{repository}/releases/latest", ct)
                .ConfigureAwait(false))
                .EnsureSuccessStatusCode();

            // 读取响应内容流并解析 JSON
            await using var contentStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);

            // 解析最新 release 信息
            using var document = await JsonDocument.ParseAsync(contentStream, new(), ct).ConfigureAwait(false);

            // 提取最新版本号 (tag 形如 v1.0.1, 去掉前导 v)
            string? latestVersion = null;
            if (document.RootElement.TryGetProperty("tag_name", out var tagName))
            {
                latestVersion = tagName.GetString()?.TrimStart('v');
            }

            // 没有新版本
            if (!IsNewerVersion(latestVersion, _currentVersion))
            {
                _downloadUri = string.Empty;
                LatestVersion = _currentVersion;
                TotalBytes = 0;
                State = UpdateState.UpToDate;
                LogUpToDate(_currentVersion);
                return;
            }

            // 查找当前平台的更新包资产, 未找到时视为检查失败
            if (!TryFindUpdateAsset(document.RootElement, out var downloadUri, out var totalBytes))
            {
                throw new InvalidOperationException($"{TargetName} is not found in the release assets.");
            }

            // 发现新版本: 保存最新版本信息
            _downloadUri = downloadUri;
            LatestVersion = latestVersion;
            TotalBytes = totalBytes;

            // 如果之前的状态是已经下载完成, 并且更新包完整, 则直接置为下载完成状态
            if (previousState is UpdateState.DownloadCompleted && IsUpdatePackageComplete())
            {
                State = UpdateState.DownloadCompleted;
                LogUpdatePackageAlreadyComplete(Constants.UpdatePackageFilePath);
                return;
            }

            // 设置为需要更新状态, 以便用户可以发起下载
            State = UpdateState.NeedUpdate;
            LogFoundUpdate(latestVersion);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // 取消检查更新: 回到之前的状态
            State = previousState;
        }
        catch (Exception ex)
        {
            // 检查更新失败: 将各种状态重置为初始值
            _downloadUri = string.Empty;
            LatestVersion = null;
            TotalBytes = 0;

            // 记录异常信息, 并设置为检查失败状态
            Exception = ex;
            State = UpdateState.CheckFailed;
            LogCheckingFailed(ex);
        }
    }

    /// <inheritdoc/>
    public async Task DownloadAsync(CancellationToken ct = default)
    {
        // 只有在需要更新或者下载失败状态下才能发起下载请求, 否则忽略
        if (State is not UpdateState.NeedUpdate and not UpdateState.DownloadFailed)
        {
            return;
        }

        // 更新包已存在且大小与服务器资产一致: 无需重新下载, 直接完成 (恢复已完成的下载)
        if (IsUpdatePackageComplete())
        {
            State = UpdateState.DownloadCompleted;
            LogUpdatePackageAlreadyComplete(Constants.UpdatePackageFilePath);
            return;
        }

        // 重置异常并更新状态为下载中
        Exception = null;
        State = UpdateState.Downloading;

        // 重置下载进度信息
        DownloadedBytes = 0;
        ProgressPercentage = 0;
        SpeedBytesPerSecond = 0;

        try
        {
            // 确保更新数据目录存在
            _ = Directory.CreateDirectory(Constants.UpdateDataDirectory);

            // 存在断点文件时先校验版本标识: 与当前要下载的版本不一致时删除断点, 避免跨版本续传
            if (File.Exists(_tempFilePath) && !IsNewVersionFileMatch())
            {
                File.Delete(_tempFilePath);
            }

            // 保存最新版本号到文件, 作为断点文件与完整更新包的版本标识
            File.WriteAllText(Constants.NewVersionFilePath, LatestVersion);

            // 创建下载服务: 启用断点续传, 失败时保留断点文件供下次续传
            _downloadService = new(new DownloadConfiguration
            {
                ParallelDownload = true,
                ChunkCount = 4,
                MaxTryAgainOnFailure = 3,
                EnableAutoResumeDownload = true,
                ClearPackageOnCompletionWithFailure = false,
                FileExistPolicy = FileExistPolicy.Delete,
                DownloadFileExtension = Constants.DownloadTempFileSuffix
            });

            // 存在断点文件时, Downloader 会自动从断点继续下载
            if (File.Exists(_tempFilePath))
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
            ).ConfigureAwait(false);

            // 用户取消: 保留断点文件供下次续传, 回到需要更新状态, 并重置下载进度信息
            if (ct.IsCancellationRequested || _downloadService.IsCancelled)
            {
                State = UpdateState.NeedUpdate;
                DownloadedBytes = 0;
                ProgressPercentage = 0;
                SpeedBytesPerSecond = 0;
                LogDownloadCancelled();
                return;
            }

            // 检查下载完成后更新包的完整性: 文件大小与服务器资产一致, 且版本标识匹配
            if (!IsUpdatePackageComplete())
            {
                throw new InvalidOperationException(
                    $"Downloaded update package is incomplete or corrupted:" +
                    Constants.UpdatePackageFilePath
                );
            }

            State = UpdateState.DownloadCompleted;
            DownloadedBytes = TotalBytes;
            ProgressPercentage = Core.Constants.PercentBase;
            SpeedBytesPerSecond = 0;
            LogDownloadCompleted(LatestVersion);
        }
        catch (OperationCanceledException)
        {
            State = UpdateState.NeedUpdate;
            DownloadedBytes = 0;
            ProgressPercentage = 0;
            SpeedBytesPerSecond = 0;
            LogDownloadCancelled();
        }
        catch (Exception ex)
        {
            SpeedBytesPerSecond = 0;
            Exception = ex;
            State = UpdateState.DownloadFailed;
            LogDownloadFailed(ex);
        }
        finally
        {
            if (_downloadService is not null)
            {
                _downloadService.DownloadStarted -= OnDownloadStarted;
                _downloadService.DownloadProgressChanged -= OnDownloadProgressChanged;
                await _downloadService.DisposeAsync().ConfigureAwait(false);
                _downloadService = null;
            }
        }
    }

    /// <inheritdoc/>
    public void CancelDownload()
    {
        if (State is UpdateState.Downloading)
        {
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
