namespace MineClearance.Infrastructure.Models;

/// <summary>
/// 更新状态
/// </summary>
public enum UpdateState
{
    /// <summary>
    /// 空闲
    /// </summary>
    Idle,

    /// <summary>
    /// 检查中
    /// </summary>
    Checking,

    /// <summary>
    /// 已是最新
    /// </summary>
    UpToDate,

    /// <summary>
    /// 需要更新
    /// </summary>
    NeedUpdate,

    /// <summary>
    /// 检查失败
    /// </summary>
    CheckFailed,

    /// <summary>
    /// 下载中
    /// </summary>
    Downloading,

    /// <summary>
    /// 下载完成
    /// </summary>
    DownloadCompleted,

    /// <summary>
    /// 下载失败
    /// </summary>
    DownloadFailed
}
