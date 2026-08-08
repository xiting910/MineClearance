using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MineClearance.Core.Enums;
using MineClearance.Core.Models;
using MineClearance.Core.Models.Records;
using System.Collections.Generic;
using System.ComponentModel;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 格子视图模型, 包装棋盘中的单个格子, 负责显示文本与配色
/// </summary>
public sealed partial class CellViewModel : ObservableObject
{
    private static readonly IBrush UnopenedBrush = new SolidColorBrush(Color.Parse("#C8C8C8"));
    private static readonly IBrush EmptyBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly IBrush NumberBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly IBrush WarningBrush = new SolidColorBrush(Color.Parse("#FFE14D"));
    private static readonly IBrush FlaggedBrush = new SolidColorBrush(Color.Parse("#7BD88F"));
    private static readonly IBrush QuestionBrush = new SolidColorBrush(Color.Parse("#E3E3E3"));
    private static readonly IBrush MineBrush = new SolidColorBrush(Color.Parse("#FF6B6B"));
    private static readonly IBrush HitMineBrush = new SolidColorBrush(Color.Parse("#B71C1C"));
    private static readonly IBrush ErrorFlagBrush = new SolidColorBrush(Color.Parse("#FFCDD2"));
    private static readonly IBrush TextBrush = new SolidColorBrush(Color.Parse("#1A1D24"));

    /// <summary>
    /// 数字 1-8 经典配色
    /// </summary>
    private static readonly IReadOnlyList<IBrush> NumberColors =
    [
        new SolidColorBrush(Color.Parse("#1976D2")), // 1 蓝
        new SolidColorBrush(Color.Parse("#2E7D32")), // 2 绿
        new SolidColorBrush(Color.Parse("#C62828")), // 3 红
        new SolidColorBrush(Color.Parse("#6A1B9A")), // 4 紫
        new SolidColorBrush(Color.Parse("#6D4C41")), // 5 栗
        new SolidColorBrush(Color.Parse("#00838F")), // 6 青
        new SolidColorBrush(Color.Parse("#212121")), // 7 黑
        new SolidColorBrush(Color.Parse("#757575"))  // 8 灰
    ];

    /// <summary>
    /// 被包装的格子
    /// </summary>
    private Cell _cell;

    /// <summary>
    /// 格子位置
    /// </summary>
    public Position Position { get; }

    /// <summary>
    /// 格子水平像素坐标, 供 Canvas 绝对定位, 由最大棋盘位置计算且固定不变
    /// </summary>
    public double X { get; }

    /// <summary>
    /// 格子垂直像素坐标, 供 Canvas 绝对定位, 由最大棋盘位置计算且固定不变
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// 格子是否在当前棋盘内可见, 低难度时超出棋盘的行列格子隐藏
    /// </summary>
    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    /// <summary>
    /// 显示文本 (数字/旗/问号/地雷符号)
    /// </summary>
    [ObservableProperty]
    public partial string DisplayText { get; set; }

    /// <summary>
    /// 格子背景
    /// </summary>
    [ObservableProperty]
    public partial IBrush Background { get; set; }

    /// <summary>
    /// 格子文本前景
    /// </summary>
    [ObservableProperty]
    public partial IBrush Foreground { get; set; }

    /// <summary>
    /// 创建格子视图模型
    /// </summary>
    /// <param name="position">格子位置</param>
    /// <param name="cell">被包装的格子</param>
    public CellViewModel(Position position, Cell cell)
    {
        _cell = cell;
        _cell.PropertyChanged += OnCellPropertyChanged;
        Position = position;
        X = position.Col * Constants.CellSize;
        Y = position.Row * Constants.CellSize;
        IsVisible = false;
        DisplayText = string.Empty;
        Background = UnopenedBrush;
        Foreground = TextBrush;
        UpdateDisplay();
    }

    /// <summary>
    /// 替换内部格子引用 (占位格子升级为真实格子), 不重建视图模型
    /// </summary>
    /// <param name="cell">新的格子</param>
    public void UpdateCell(Cell cell)
    {
        if (!ReferenceEquals(_cell, cell))
        {
            _cell.PropertyChanged -= OnCellPropertyChanged;
            _cell = cell;
            _cell.PropertyChanged += OnCellPropertyChanged;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 格子类型变化时刷新显示
    /// </summary>
    /// <param name="sender">格子</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Cell.Type))
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 按格子类型更新显示文本与配色
    /// </summary>
    private void UpdateDisplay()
    {
        switch (_cell.Type)
        {
            case CellType.Unopened:
                DisplayText = string.Empty;
                Background = UnopenedBrush;
                Foreground = TextBrush;
                break;

            case CellType.Empty:
                DisplayText = string.Empty;
                Background = EmptyBrush;
                Foreground = TextBrush;
                break;

            case CellType.Number:
                DisplayText = _cell.AdjacentMineCount.ToString();
                Background = NumberBrush;
                Foreground = NumberColors[_cell.AdjacentMineCount - 1];
                break;

            case CellType.WarningNumber:
                DisplayText = _cell.AdjacentMineCount.ToString();
                Background = WarningBrush;
                Foreground = NumberColors[_cell.AdjacentMineCount - 1];
                break;

            case CellType.Flagged:
                DisplayText = "⚑";
                Background = FlaggedBrush;
                Foreground = TextBrush;
                break;

            case CellType.Question:
                DisplayText = "?";
                Background = QuestionBrush;
                Foreground = TextBrush;
                break;

            case CellType.Mine:
                DisplayText = "💣";
                Background = MineBrush;
                Foreground = TextBrush;
                break;

            case CellType.ErrorFlag:
                DisplayText = "⚑";
                Background = ErrorFlagBrush;
                Foreground = TextBrush;
                break;

            case CellType.OpenedMine:
                DisplayText = "💣";
                Background = HitMineBrush;
                Foreground = TextBrush;
                break;
        }
    }
}
