namespace MineClearance.Core.Interfaces;

/// <summary>
/// 提示提供者接口
/// </summary>
public interface IHintProvider
{
    /// <summary>
    /// 当前使用的提示策略类型
    /// </summary>
    Enums.HintStrategyType StrategyType { get; }

    /// <summary>
    /// 获取一个推荐的安全格子的位置
    /// </summary>
    /// <param name="board">当前游戏棋盘</param>
    /// <returns>一个推荐的安全格子的位置, 如果没有可推荐的安全格子则返回 <see langword="null"/></returns>
    Models.Records.Position? GetHint(IGameBoard board);
}
