using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 游戏棋盘字典实现类, 以字典形式存储格子集合, 支持通过位置快速访问
/// </summary>

internal sealed class GameBoardDictionary : Interfaces.IGameBoardDictionary
{
    /// <summary>
    /// 内部字典, 用于存储位置和格子对象的映射关系
    /// </summary>
    private readonly Dictionary<Models.Records.Position, Models.Cell> _cells = [];

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public int OpenedCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(OpenedCount)));
            }
        }
    }

    /// <inheritdoc/>
    public int FlagCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(FlagCount)));
            }
        }
    }

    /// <inheritdoc/>
    public int QuestionCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(QuestionCount)));
            }
        }
    }

    /// <inheritdoc/>
    public Models.Cell this[Models.Records.Position key] => _cells[key];

    /// <inheritdoc/>
    public IEnumerable<Models.Records.Position> Keys => _cells.Keys;

    /// <inheritdoc/>
    public IEnumerable<Models.Cell> Values => _cells.Values;

    /// <inheritdoc/>
    public int Count => _cells.Count;

    /// <summary>
    /// 构造函数, 根据棋盘行数、列数和周围地雷数量数组初始化棋盘字典
    /// </summary>
    /// <param name="rows">棋盘行数</param>
    /// <param name="columns">棋盘列数</param>
    /// <param name="adjacentMineCounts">按行优先顺序排列的周围地雷数量数组</param>
    public GameBoardDictionary(int rows, int columns, int[] adjacentMineCounts)
    {
        Debug.Assert(adjacentMineCounts.Length == rows * columns, $"The length of {nameof(adjacentMineCounts)} must be equal to {nameof(rows)} * {nameof(columns)}.");
        foreach (var position in Models.Records.Position.GetAllPositions(rows, columns))
        {
            _cells[position] = new()
            {
                AdjacentMineCount = adjacentMineCounts[position.ToIndex(columns)]
            };
            _cells[position].PropertyChanged += OnCellPropertyChanged;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<Models.Records.Position, Enums.CellType> GetCellStates()
    {
        return _cells.Where(kvp => kvp.Value.Type is not Enums.CellType.Unopened)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Type);
    }

    /// <inheritdoc/>
    public bool ContainsKey(Models.Records.Position key)
    {
        return _cells.ContainsKey(key);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Models.Records.Position, Models.Cell>> GetEnumerator()
    {
        return _cells.GetEnumerator();
    }

    /// <inheritdoc/>
    public bool TryGetValue(Models.Records.Position key, [MaybeNullWhen(false)] out Models.Cell value)
    {
        return _cells.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _cells.GetEnumerator();
    }

    /// <summary>
    /// 当格子属性发生变化时触发, 用于更新已打开、已插旗和已标记问号的数量
    /// </summary>
    /// <param name="sender">触发事件的格子对象</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Models.Cell.Type))
        {
            OpenedCount = _cells.Values.Count(cell => cell.Type is Enums.CellType.Empty or Enums.CellType.Number or Enums.CellType.WarningNumber);
            FlagCount = _cells.Values.Count(cell => cell.Type is Enums.CellType.Flagged);
            QuestionCount = _cells.Values.Count(cell => cell.Type is Enums.CellType.Question);
        }
    }
}
