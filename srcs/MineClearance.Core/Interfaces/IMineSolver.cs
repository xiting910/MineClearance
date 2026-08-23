using MineClearance.Core.Models.Records;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 地雷求解器接口, 在玩家点击可能踩雷的格子时, 尝试重排雷位使该格子安全翻开, 从而实现无猜体验
/// </summary>
internal interface IMineSolver
{
    /// <summary>
    /// 尝试重排雷位, 使指定格子能够安全翻开
    /// </summary>
    /// <param name="target">玩家要翻开的格子</param>
    /// <param name="config">游戏配置</param>
    /// <param name="mineField">地雷场, 提供当前雷位与数字格计数</param>
    /// <param name="board">当前棋盘, 已揭示的数字格子提供约束</param>
    /// <param name="rearrangedMines">重排后的满足约束的雷位图</param>
    /// <param name="guaranteedSafePositions">可以推定为必定安全的格子集合</param>
    /// <returns><see langword="true"/> 如果重排成功, 否则为 <see langword="false"/></returns>
    bool TrySafeOpen(
        Position target,
        GameConfig config,
        IMineField mineField,
        IGameBoardDictionary board,
        [NotNullWhen(true)] out BitArray? rearrangedMines,
        [NotNullWhen(false)] out HashSet<Position>? guaranteedSafePositions
    );
}
