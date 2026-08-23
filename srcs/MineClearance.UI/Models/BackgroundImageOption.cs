namespace MineClearance.UI.Models;

/// <summary>
/// 背景图片选项
/// </summary>
/// <param name="DisplayName">下拉框显示文本</param>
/// <param name="FileName">图片文件名, 空值表示不使用背景图片</param>
public sealed record BackgroundImageOption(string DisplayName, string? FileName);
