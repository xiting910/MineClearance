using MineClearance.Core.Models.Records;
using System;
using System.Collections;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 内部地雷场接口, 负责地雷的放置和查询, 不包含与玩家交互的格子集合
/// </summary>
internal interface IMineField
{
    /// <summary>
    /// 生成地雷场, 确保首次点击位置不是地雷, 并尽量保证在首次点击位置周围的格子中没有地雷
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="firstClick">首次点击位置</param>
    /// <param name="seed">随机种子, 用于生成固定的地雷布局</param>
    void Generate(GameConfig config, Position firstClick, int seed);

    /// <summary>
    /// 应用给定的地雷场位图
    /// </summary>
    /// <param name="config">游戏配置</param>
    /// <param name="mineMap">地雷场的位图表示, 其中每一位表示一个格子是否是地雷</param>
    void Apply(GameConfig config, BitArray mineMap);

    /// <summary>
    /// 获取地雷场的位图表示, 其中每一位表示一个格子是否是地雷
    /// </summary>
    /// <returns>地雷场的位图表示</returns>
    /// <exception cref="InvalidOperationException">如果地雷场尚未生成, 则抛出该异常</exception>
    BitArray GetMineMap();

    /// <summary>
    /// 判断指定位置是否是地雷
    /// </summary>
    /// <param name="position">要判断的格子位置</param>
    /// <returns><see langword="true"/> 如果指定位置是地雷, 否则返回 <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException">如果地雷场尚未生成, 则抛出该异常</exception>
    bool IsMine(Position position);

    /// <summary>
    /// 获取指定位置周围的地雷数量
    /// </summary>
    /// <param name="position">要查询的格子位置</param>
    /// <returns>指定位置周围的地雷数量</returns>
    /// <exception cref="InvalidOperationException">如果地雷场尚未生成或该位置是地雷, 则抛出该异常</exception>
    int GetAdjacentMineCount(Position position);
}
