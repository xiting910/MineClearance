using CommunityToolkit.Mvvm.ComponentModel;
using MineClearance.Core;
using MineClearance.Core.Enums;

namespace MineClearance.UI.Models;

/// <summary>
/// 难度筛选选项, 支持多选, 不选任何项表示全部难度
/// </summary>
/// <param name="difficulty">难度</param>
public sealed partial class DifficultyFilterOption(GameDifficulty difficulty) : ObservableObject
{
    /// <summary>
    /// 难度
    /// </summary>
    public GameDifficulty Difficulty { get; } = difficulty;

    /// <summary>
    /// 下拉框显示文本
    /// </summary>
    public string Text { get; } = difficulty.GetDescription();

    /// <summary>
    /// 是否选中, 变化时由视图模型重新应用筛选
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
