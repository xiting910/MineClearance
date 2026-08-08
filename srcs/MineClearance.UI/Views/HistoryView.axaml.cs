using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using System.Linq;

namespace MineClearance.UI.Views;

/// <summary>
/// 历史记录视图, 提供游戏结果的统计与展示
/// </summary>
public sealed partial class HistoryView : UserControl
{
    /// <summary>
    /// 创建历史记录视图
    /// </summary>
    public HistoryView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击统计表格可排序列头时切换排序, 点击区域为整个列头单元格, 排序键存放在列的 Tag 中
    /// </summary>
    /// <param name="sender">排序列</param>
    /// <param name="e">指针按下事件参数</param>
    private void OnStatsSortHeaderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is DataGridColumn { Tag: string key } && DataContext is HistoryViewModel viewModel)
        {
            viewModel.ToggleStatsSort(key);
        }
    }

    /// <summary>
    /// 选中变化时同步选中的行到视图模型 (SelectedItems 为非泛型集合, 编译绑定无法直接绑定, 此处转换为类型安全列表)
    /// </summary>
    /// <param name="sender">详细记录表格</param>
    /// <param name="e">选中变化事件参数</param>
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is HistoryViewModel viewModel && sender is DataGrid grid)
        {
            viewModel.SelectedRows = grid.SelectedItems.Cast<GameResultRow>().ToList();
        }
    }
}
