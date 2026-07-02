using System;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏核心接口
/// </summary>
public interface IGame
{
    /// <summary>
    /// 游戏状态变更事件
    /// </summary>
    event EventHandler<Models.Args.GameStatusChangedEventArgs> StatusChanged;

    /// <summary>
    /// 获取当前游戏棋盘
    /// </summary>
    IGameBoard Board { get; }

    /// <summary>
    /// 获取当前游戏地雷场
    /// </summary>
    IMineField Minefield { get; }

    /// <summary>
    /// 获取当前游戏计时器
    /// </summary>
    IGameTimer Timer { get; }

    /// <summary>
    /// 获取当前游戏提示提供者
    /// </summary>
    IHintProvider HintProvider { get; }

    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    Enums.GameStatus Status { get; }

    /// <summary>
    /// 获取当前游戏难度
    /// </summary>
    Enums.GameDifficulty Difficulty { get; }

    /// <summary>
    /// 获取当前游戏随机种子
    /// </summary>
    int Seed { get; }

    /// <summary>
    /// 获取地雷总数
    /// </summary>
    int TotalMines { get; }

    /// <summary>
    /// 获取当前游戏完成度, 范围为 0.0 到 <see cref="Constants.MaxCompletion"/>
    /// </summary>
    double Completion { get; }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    void Pause();

    /// <summary>
    /// 取消暂停游戏
    /// </summary>
    void CancelPause();

    /// <summary>
    /// 打开指定位置的格子
    /// </summary>
    /// <param name="position">要打开的格子的位置</param>
    void OpenCell(Models.Records.Position position);

    /// <summary>
    /// 在指定位置插旗
    /// </summary>
    /// <param name="position">要插旗的格子的位置</param>
    void FlagCell(Models.Records.Position position);

    /// <summary>
    /// 在指定位置标记问号
    /// </summary>
    /// <param name="position">要标记问号的格子的位置</param>
    void QuestionCell(Models.Records.Position position);

    /// <summary>
    /// 取消指定位置的插旗或者问号标记, 将格子恢复为未打开状态
    /// </summary>
    /// <param name="position">要取消标记的格子的位置</param>
    void UnmarkCell(Models.Records.Position position);
}
