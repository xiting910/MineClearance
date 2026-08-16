using MineClearance.Core.Models.Records;
using System.Collections;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 地雷求解器接口, 在玩家点击可能踩雷的格子时, 尝试重排雷位使该格子安全翻开, 从而实现无猜体验
/// </summary>
internal interface IMineSolver
{
    /// <summary>
    /// 尝试重排雷位, 使指定格子能够安全翻开
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="board">当前棋盘, 已揭示的数字格子提供约束</param>
    /// <param name="mineField">地雷场, 提供当前雷位与数字格计数</param>
    /// <param name="target">玩家要翻开的格子</param>
    /// <returns>重排后的雷位图, 其中 <paramref name="target"/> 不是地雷, 且所有已揭示数字格子的数字保持一致;
    /// 若不存在这样的重排, 返回 <see langword="null"/></returns>
    BitArray? TrySafeOpen(
        GameConfig config, IGameBoardDictionary board, IMineField mineField, Position target
    );
}
