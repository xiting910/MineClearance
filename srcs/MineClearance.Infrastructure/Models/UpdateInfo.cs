namespace MineClearance.Infrastructure.Models;

/// <summary>
/// 更新信息记录
/// </summary>
/// <param name="IsSuccess">是否更新成功</param>
/// <param name="OriginalVersion">原始版本</param>
/// <param name="NewVersion">新版本</param>
public sealed record UpdateInfo(bool IsSuccess, string OriginalVersion, string NewVersion);
