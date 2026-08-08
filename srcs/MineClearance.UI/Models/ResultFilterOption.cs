namespace MineClearance.UI.Models;

/// <summary>
/// 结果筛选选项, 空值表示全部结果
/// </summary>
/// <param name="IsWin"><see langword="true"/> 胜利, <see langword="false"/> 失败, <see langword="null"/> 全部</param>
/// <param name="Text">下拉框显示文本</param>
public sealed record ResultFilterOption(bool? IsWin, string Text);
