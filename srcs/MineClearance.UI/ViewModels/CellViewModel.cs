using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models;
using MineClearance.Core.Models.Records;
using System;
using System.ComponentModel;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 格子视图模型, 包装棋盘中的单个格子, 负责显示文本与配色
/// </summary>
public sealed partial class CellViewModel : ObservableObject
{
    /// <summary>
    /// 主题资源键: 未翻开格子背景
    /// </summary>
    private const string UnopenedBrushKey = "CellUnopenedBrush";

    /// <summary>
    /// 主题资源键: 已翻开空白格背景
    /// </summary>
    private const string EmptyBrushKey = "CellEmptyBrush";

    /// <summary>
    /// 主题资源键: 数字格背景
    /// </summary>
    private const string NumberBrushKey = "CellNumberBrush";

    /// <summary>
    /// 主题资源键: 警告数字格背景
    /// </summary>
    private const string WarningBrushKey = "CellWarningBrush";

    /// <summary>
    /// 主题资源键: 旗格背景
    /// </summary>
    private const string FlaggedBrushKey = "CellFlaggedBrush";

    /// <summary>
    /// 主题资源键: 问号格背景
    /// </summary>
    private const string QuestionBrushKey = "CellQuestionBrush";

    /// <summary>
    /// 主题资源键: 地雷格背景
    /// </summary>
    private const string MineBrushKey = "CellMineBrush";

    /// <summary>
    /// 主题资源键: 踩中地雷格背景
    /// </summary>
    private const string HitMineBrushKey = "CellHitMineBrush";

    /// <summary>
    /// 主题资源键: 错误旗格背景
    /// </summary>
    private const string ErrorFlagBrushKey = "CellErrorFlagBrush";

    /// <summary>
    /// 主题资源键: 保证安全格背景
    /// </summary>
    private const string GuaranteedSafeBrushKey = "CellGuaranteedSafeBrush";

    /// <summary>
    /// 主题资源键: 格子符号文字颜色
    /// </summary>
    private const string TextBrushKey = "CellTextBrush";

    /// <summary>
    /// 主题资源键前缀: 数字 1-8 配色, 完整键为 CellNumber1Brush..CellNumber8Brush
    /// </summary>
    private const string NumberColorBrushKeyPrefix = "CellNumber";

    /// <summary>
    /// 被包装的格子
    /// </summary>
    private Cell _cell;

    /// <summary>
    /// 格子是否在当前棋盘内可见, 低难度时超出棋盘的行列格子隐藏
    /// </summary>
    [ObservableProperty]
    public partial bool IsVisible { get; set; }

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
    /// 显示文本 (数字/旗/问号/地雷符号)
    /// </summary>
    [ObservableProperty]
    public partial string DisplayText { get; set; }

    /// <summary>
    /// 格子索引文本, 热键按下时显示, 由游戏视图模型按当前棋盘填充
    /// </summary>
    [ObservableProperty]
    public partial string IndexText { get; set; }

    /// <summary>
    /// 是否显示格子索引, 等待开始时按住热键为 true
    /// </summary>
    [ObservableProperty]
    public partial bool ShowIndex { get; set; }

    /// <summary>
    /// 游戏实例, 用于查询格子周围地雷数量, 由游戏视图模型在棋盘更新时设置, 游戏解绑时清空
    /// </summary>
    public IGame? Game { get; set; }

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
    /// 创建格子视图模型
    /// </summary>
    /// <param name="cell">被包装的格子</param>
    /// <param name="position">格子位置</param>
    public CellViewModel(Cell cell, Position position)
    {
        _cell = cell;
        _cell.PropertyChanged += OnCellPropertyChanged;
        IsVisible = false;
        Background = ThemeBrush(UnopenedBrushKey);
        Foreground = ThemeBrush(TextBrushKey);
        DisplayText = string.Empty;
        IndexText = string.Empty;
        ShowIndex = false;
        Position = position;
        X = position.Col * Constants.CellSize;
        Y = position.Row * Constants.CellSize;
        UpdateDisplay();
    }

    /// <summary>
    /// 更新被包装的格子
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
    /// 按格子类型更新显示文本与配色
    /// </summary>
    public void UpdateDisplay()
    {
        switch (_cell.Type)
        {
            case CellType.Unopened:
                DisplayText = string.Empty;
                Background = ThemeBrush(UnopenedBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.Empty:
                DisplayText = string.Empty;
                Background = ThemeBrush(EmptyBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.Number:
                UpdateNumberDisplay(ThemeBrush(NumberBrushKey));
                break;

            case CellType.WarningNumber:
                UpdateNumberDisplay(ThemeBrush(WarningBrushKey));
                break;

            case CellType.Flagged:
                DisplayText = "⚑";
                Background = ThemeBrush(FlaggedBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.Question:
                DisplayText = "?";
                Background = ThemeBrush(QuestionBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.Mine:
                DisplayText = "💣";
                Background = ThemeBrush(MineBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.ErrorFlag:
                DisplayText = "⚑";
                Background = ThemeBrush(ErrorFlagBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.OpenedMine:
                DisplayText = "💣";
                Background = ThemeBrush(HitMineBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;

            case CellType.GuaranteedSafe:
                DisplayText = "✓";
                Background = ThemeBrush(GuaranteedSafeBrushKey);
                Foreground = ThemeBrush(TextBrushKey);
                break;
        }
    }

    /// <summary>
    /// 更新数字格子的显示文本与配色, 雷数由游戏实例按位置查询, 游戏未绑定时不显示数字
    /// </summary>
    /// <param name="background">数字格子的背景</param>
    private void UpdateNumberDisplay(IBrush background)
    {
        var mineCount = Game?.GetAdjacentMineCount(Position) ?? 0;
        DisplayText = mineCount.ToString();
        Background = background;
        Foreground = mineCount is > 0 and < 9 ? NumberColor(mineCount) : ThemeBrush(TextBrushKey);
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
    /// 获取指定数字的配色, 从主题资源读取
    /// </summary>
    /// <param name="number">数字</param>
    /// <returns>数字画刷</returns>
    private static IBrush NumberColor(int number)
    {
        return ThemeBrush(NumberColorBrushKeyPrefix + number + "Brush");
    }

    /// <summary>
    /// 从当前主题资源字典获取画刷
    /// </summary>
    /// <param name="key">资源键</param>
    /// <returns>主题画刷</returns>
    /// <exception cref="InvalidOperationException">应用未初始化或主题资源字典中不存在指定资源</exception>
    private static IBrush ThemeBrush(string key)
    {
        var app = Application.Current;
        return app?.TryGetResource(key, app.ActualThemeVariant, out var value) is true
            && value is IBrush brush
            ? brush
            : throw new InvalidOperationException($"Theme resource '{key}' not found.");
    }
}
