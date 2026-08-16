using MineClearance.Core.Enums;
using System.ComponentModel;

namespace MineClearance.Core.Models;

/// <summary>
/// 游戏格子类, 实现 <see cref="INotifyPropertyChanged"/> 接口
/// </summary>
public sealed class Cell : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

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
}
