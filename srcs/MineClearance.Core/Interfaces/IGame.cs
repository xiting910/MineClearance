using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using System;
using System.ComponentModel;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏核心接口
/// </summary>
public interface IGame : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 获取当前游戏棋盘字典, 在地雷生成之前为 <see langword="null"/>
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    IGameBoardDictionary? Board { get; }

    /// <summary>
    /// 获取当前游戏计时器
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    IGameTimer Timer { get; }

    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    GameStatus Status { get; }

    /// <summary>
    /// 获取当前游戏难度
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    GameDifficulty Difficulty { get; }

    /// <summary>
    /// 获取当前游戏配置
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    GameConfig Config { get; }

    /// <summary>
    /// 获取当前游戏随机种子
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    int Seed { get; }

    /// <summary>
    /// 获取当前游戏完成度, 范围为 0.0 到 <see cref="Constants.MaxCompletion"/>
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    double Completion { get; }

    /// <summary>
    /// 获取当前游戏的游戏结果, 在游戏结束之前为 <see langword="null"/>
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    GameResult? Result { get; }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void Pause();

    /// <summary>
    /// 取消暂停游戏
    /// </summary>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void CancelPause();

    /// <summary>
    /// 打开指定位置的格子
    /// </summary>
    /// <param name="position">要打开的格子的位置</param>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void OpenCell(Position position);

    /// <summary>
    /// 在指定位置插旗
    /// </summary>
    /// <param name="position">要插旗的格子的位置</param>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void FlagCell(Position position);

    /// <summary>
    /// 在指定位置标记问号
    /// </summary>
    /// <param name="position">要标记问号的格子的位置</param>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void QuestionCell(Position position);

    /// <summary>
    /// 取消指定位置的插旗或者问号标记, 将格子恢复为未打开状态
    /// </summary>
    /// <param name="position">要取消标记的格子的位置</param>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void UnmarkCell(Position position);

    /// <summary>
    /// 打开某个数字格子周围的所有未打开格子
    /// </summary>
    /// <param name="position">要打开的数字格子的位置</param>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void OpenAdjacentCells(Position position);

    /// <summary>
    /// 标记某个数字格子周围的所有未打开格子为旗子
    /// </summary>
    /// <param name="position">要标记的数字格子的位置</param>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    void FlagAdjacentCells(Position position);

    /// <summary>
    /// 获取当前游戏的存档数据, 用于保存游戏进度
    /// </summary>
    /// <returns>游戏存档数据</returns>
    /// <exception cref="ObjectDisposedException">如果当前实例已被释放, 则抛出该异常</exception>
    GameSaveData? GetSaveData();
}
