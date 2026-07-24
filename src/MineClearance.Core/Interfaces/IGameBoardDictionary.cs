using MineClearance.Core.Enums;
using MineClearance.Core.Models;
using MineClearance.Core.Models.Records;
using System.Collections.Generic;
using System.ComponentModel;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏棋盘字典接口, 用于表示游戏棋盘的格子集合, 以便通过位置快速访问格子
/// </summary>
public interface IGameBoardDictionary : IReadOnlyDictionary<Position, Cell>, INotifyPropertyChanged
{
    /// <summary>
    /// 获取当前已打开的格子数量
    /// </summary>
    int OpenedCount { get; }

    /// <summary>
    /// 获取当前已插旗的数量
    /// </summary>
    int FlagCount { get; }

    /// <summary>
    /// 获取当前已标记问号的数量
    /// </summary>
    int QuestionCount { get; }

    /// <summary>
    /// 获取所有非未打开格子的状态
    /// </summary>
    /// <returns>格子状态的字典</returns>
    IReadOnlyDictionary<Position, CellType> GetCellStates();
}
