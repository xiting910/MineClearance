using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
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
    /// 行加载时更新行头序号, 序号为当前显示顺序 (随筛选与排序变化), 从 1 递增
    /// </summary>
    /// <param name="sender">详细记录表格</param>
    /// <param name="e">行加载事件参数</param>
    private void OnRowLoading(object? sender, DataGridRowEventArgs e)
    {
        var index = (e.Row.Index + 1).ToString();
        e.Row.Header = index;
        if (e.Row.FindDescendantOfType<DataGridRowHeader>() is { } header)
        {
            header.Content = index;
        }
    }

    /// <summary>
    /// 选中变化时同步选中的行到视图模型
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

    /// <summary>
    /// 双击记录行时复制该局种子到剪贴板, 并显示 Toast 提示
    /// </summary>
    /// <param name="sender">详细记录表格</param>
    /// <param name="e">单元格指针按下事件参数</param>
    private async void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.ClickCount != 2) { return; }
        if (e.Row.DataContext is not GameResultRow row) { return; }
        if (DataContext is not HistoryViewModel viewModel) { return; }

        // 剪贴板不可用时提示, 否则写入并提示
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            viewModel.Show("剪贴板不可用");
            return;
        }

        await clipboard.SetTextAsync(row.Result.Seed.ToString());
        viewModel.Show($"种子: {row.Result.Seed} 已复制");
    }
}
