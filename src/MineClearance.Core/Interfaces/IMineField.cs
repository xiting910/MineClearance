using System;
using System.Collections;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 内部地雷场接口, 负责地雷的放置和查询, 不包含与玩家交互的格子集合
/// </summary>
internal interface IMineField
{
    /// <summary>
    /// 创建地雷场, 该方法会根据指定的配置和首次点击位置生成地雷场
    /// </summary>
    /// <remarks>
    /// 该方法用于创建新游戏后首次点击时生成地雷场, 以确保首次点击的格子不是地雷
    /// </remarks>
    /// <param name="config">对局配置</param>
    /// <param name="firstClick">首次点击位置</param>
    /// <returns>按行优先顺序排列的周围地雷数量数组</returns>
    int[] Generate(Models.Records.GameConfig config, Models.Records.Position firstClick);

    /// <summary>
    /// 从指定的位图表示中创建地雷场, 该方法会根据指定的位图生成地雷场
    /// </summary>
    /// <remarks>
    /// 该方法用于从保存的游戏状态中恢复地雷场, 以确保恢复后的地雷场与保存时一致
    /// </remarks>
    /// <param name="mineMap">地雷场的位图表示, 其中每一位表示一个格子是否是地雷</param>
    /// <returns>按行优先顺序排列的周围地雷数量数组</returns>
    int[] Generate(BitArray mineMap);

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
    bool IsMine(Models.Records.Position position);

    /// <summary>
    /// 获取指定位置周围的地雷数量
    /// </summary>
    /// <param name="position">要查询的格子位置</param>
    /// <returns>指定位置周围的地雷数量</returns>
    /// <exception cref="InvalidOperationException">如果地雷场尚未生成, 则抛出该异常</exception>
    int GetAdjacentMineCount(Models.Records.Position position);
}
