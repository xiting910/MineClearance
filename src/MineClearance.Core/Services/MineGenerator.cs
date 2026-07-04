using System;
using System.Collections.Generic;

namespace MineClearance.Core.Services;

/// <summary>
/// 地雷生成器实现类, 负责生成地雷位置集合, 确保首次点击位置不是地雷
/// </summary>
internal sealed class MineGenerator : Interfaces.IMineGenerator
{
    // TODO: 实现该方法
    /// <inheritdoc/>
    public IReadOnlyCollection<Models.Records.Position> GenerateMines(Models.Records.GameConfig config, Models.Records.Position firstClick, int seed)
    {
        throw new NotImplementedException();
    }
}
