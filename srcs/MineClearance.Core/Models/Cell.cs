using MineClearance.Core.Enums;
using System.ComponentModel;

namespace MineClearance.Core.Models;

/// <summary>
/// 游戏格子类, 实现 <see cref="INotifyPropertyChanged"/> 接口
/// </summary>
public sealed class Cell : INotifyPropertyChanged
{
    /// <summary>
    /// 格子类型, 赋值时触发 <see cref="PropertyChanged"/> 事件
    /// </summary>
    public CellType Type
    {
        get;
        internal set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Type)));
            }
        }
    } = CellType.Unopened;

    /// <summary>
    /// 周围地雷数量, 只会在生成棋盘时被赋值, 之后不会再改变
    /// </summary>
    public int AdjacentMineCount { get; init; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
}
