using System.Collections.Generic;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 地雷生成器接口, 只在 Core 层使用, 用于首次点击时生成地雷位置集合
/// </summary>
internal interface IMineGenerator
{
    /// <summary>
    /// 生成地雷位置集合
    /// </summary>
    /// <param name="config">对局配置</param>
    /// <param name="firstClick">首次点击位置</param>
    /// <returns>所有地雷位置的集合</returns>
    IReadOnlyCollection<Models.Records.Position> GenerateMines(Models.Records.GameConfig config, Models.Records.Position firstClick);
}
