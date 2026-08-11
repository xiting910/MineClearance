using Downloader;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace MineClearance.Infrastructure.Services;

// 更新服务实现类的私有成员部分
internal sealed partial class UpdateService
{
    /// <summary>
    /// 建立连接的超时时间 (秒)
    /// </summary>
    private const int ConnectTimeout = 5;

    /// <summary>
    /// 请求超时时间 (秒)
    /// </summary>
    private const int RequestTimeout = 30;

    /// <summary>
    /// Windows 平台的更新包文件名
    /// </summary>
    private const string WindowsPackageFileName = $"{nameof(MineClearance)}-win-x64{Constants.ZipFileSuffix}";

    /// <summary>
    /// Linux 平台的更新包文件名
    /// </summary>
    private const string LinuxPackageFileName = $"{nameof(MineClearance)}-linux-x64{Constants.ZipFileSuffix}";

    /// <summary>
    /// MacOS 平台的更新包文件名
    /// </summary>
    private const string OsxPackageFileName = $"{nameof(MineClearance)}-osx-x64{Constants.ZipFileSuffix}";

    /// <summary>
    /// 当前平台的更新包文件名
    /// </summary>
    private static readonly string TargetName = OperatingSystem.IsWindows()
        ? WindowsPackageFileName
        : OperatingSystem.IsLinux()
            ? LinuxPackageFileName
            : OperatingSystem.IsMacOS()
                ? OsxPackageFileName
                : throw new PlatformNotSupportedException("不支持的操作系统平台");

    /// <summary>
    /// 更新包临时文件路径 (用于断点续传)
    /// </summary>
    private static readonly string _tempFilePath = Constants.UpdatePackageFilePath
        + Constants.DownloadTempFileSuffix;

    /// <summary>
    /// 检查更新使用的 <see cref="HttpClient"/> 实例
    /// </summary>
    private readonly HttpClient _httpClient = CreateHttpClient();

    /// <summary>
    /// 当前版本号
    /// </summary>
    private string _currentVersion = string.Empty;

    /// <summary>
    /// 下载地址
    /// </summary>
    private string _downloadUri = string.Empty;

    /// <summary>
    /// 当前下载服务实例, 用于取消下载
    /// </summary>
    private DownloadService? _downloadService;

    /// <summary>
    /// 判断新版本号文件记录的版本与当前要下载的版本是否一致
    /// </summary>
    /// <returns><see langword="true"/> 如果版本一致, 否则 <see langword="false"/></returns>
    private bool IsNewVersionFileMatch()
    {
        return !string.IsNullOrEmpty(LatestVersion) && File.Exists(Constants.NewVersionFilePath) &&
            File.ReadAllText(Constants.NewVersionFilePath).Trim() == LatestVersion;
    }

    /// <summary>
    /// 判断更新包是否已下载完整 (文件存在, 大小与服务器资产一致且版本标识与当前版本一致)
    /// </summary>
    /// <returns><see langword="true"/> 如果更新包已下载完整, 否则 <see langword="false"/></returns>
    private bool IsUpdatePackageComplete()
    {
        if (TotalBytes == 0 || !IsNewVersionFileMatch()) { return false; }
        var file = new FileInfo(Constants.UpdatePackageFilePath);
        return file.Exists && file.Length == TotalBytes;
    }

    /// <summary>
    /// 下载开始时更新总字节数
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">下载开始事件参数</param>
    private void OnDownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        TotalBytes = e.TotalBytesToReceive;
    }

    /// <summary>
    /// 下载进度变化时更新已下载字节数, 进度百分比与下载速度
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">下载进度事件参数</param>
    private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        DownloadedBytes = e.ReceivedBytesSize;
        ProgressPercentage = e.ProgressPercentage;
        SpeedBytesPerSecond = e.BytesPerSecondSpeed;
    }

    /// <summary>
    /// 判断最新版本是否比当前版本新
    /// </summary>
    /// <param name="latest">最新版本号</param>
    /// <param name="current">当前版本号</param>
    /// <returns><see langword="true"/> 如果最新版本更新, 否则 <see langword="false"/></returns>
    private static bool IsNewerVersion([NotNullWhen(true)] string? latest, string current)
    {
        // 版本号可解析时按 Version 比较, 否则按字符串序号比较兜底
        return Version.TryParse(latest, out var latestVersion)
            && Version.TryParse(current, out var currentVersion)
            ? latestVersion > currentVersion
            : string.Compare(latest, current, StringComparison.Ordinal) > 0;
    }

    /// <summary>
    /// 尝试从 release 资产中查找并获取当前平台更新包的下载地址与大小
    /// </summary>
    /// <param name="root">release 信息根元素</param>
    /// <param name="downloadUri">下载地址</param>
    /// <param name="totalBytes">更新包总字节数</param>
    /// <returns><see langword="true"/> 如果找到更新包, 否则 <see langword="false"/></returns>
    private static bool TryFindUpdateAsset(
        JsonElement root,
        [MaybeNullWhen(false)] out string downloadUri,
        [MaybeNullWhen(false)] out long totalBytes)
    {
        downloadUri = default;
        totalBytes = default;
        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.TryGetProperty("name", out var name) && name.GetString() == TargetName &&
                    asset.TryGetProperty("browser_download_url", out var downloadUrl))
                {
                    var urlString = downloadUrl.GetString();
                    if (!string.IsNullOrWhiteSpace(urlString) &&
                        asset.TryGetProperty("size", out var sizeProperty))
                    {
                        downloadUri = urlString;
                        totalBytes = sizeProperty.GetInt64();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 创建检查更新使用的 <see cref="HttpClient"/>
    /// </summary>
    /// <returns>创建的 <see cref="HttpClient"/> 实例</returns>
    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(ConnectTimeout),
        };
        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(RequestTimeout)
        };
        _ = client.DefaultRequestHeaders.UserAgent.TryParseAdd(nameof(MineClearance));
        return client;
    }
}
