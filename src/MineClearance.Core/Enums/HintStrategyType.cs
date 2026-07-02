namespace MineClearance.Core.Enums;

/// <summary>
/// 提示策略枚举
/// </summary>
public enum HintStrategyType
{
    /// <summary>
    /// 获取任意安全格子, 相当于开启作弊
    /// </summary>
    AnySafe,

    /// <summary>
    /// 获取可以 100% 推理得到的安全格子
    /// </summary>
    LogicalDeduction,

    /// <summary>
    /// 获取概率最高的安全格子, 可能会有误判
    /// </summary>
    ProbabilityBased
}
