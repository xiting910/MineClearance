using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 地雷生成器实现类, 负责生成地雷位置集合, 确保首次点击位置不是地雷
/// </summary>
/// <param name="solvabilityChecker">可解性检查器实例</param>
internal sealed class MineGenerator(ISolvabilityChecker solvabilityChecker) : IMineGenerator
{
    /// <summary>
    /// 最大重试次数, 超过后放弃可解性保证
    /// </summary>
    private const int MaxRetries = 1000;

    /// <summary>
    /// 可解性检查器实例
    /// </summary>
    private readonly ISolvabilityChecker _solvabilityChecker = solvabilityChecker;

    /// <inheritdoc/>
    public IEnumerable<Position> GenerateMines(GameConfig config, Position firstClick, int seed)
    {
        var allPositions = Position.GetAllPositions(config.BoardHeight, config.BoardWidth)
            .Where(pos => pos != firstClick)
            .ToArray();

        var neighbors = firstClick.GetAdjacentPositions(config.BoardHeight, config.BoardWidth);
        var excludeNeighbors = allPositions.Except(neighbors).ToArray();

        var available = excludeNeighbors.Length >= config.MineCount ? excludeNeighbors : allPositions;
        var shuffleEngine = new ShuffleEngine<Position>(available);

        IEnumerable<Position> mines;
        var retries = 0;
        do
        {
            shuffleEngine.Shuffle(seed + retries);
            mines = shuffleEngine.Result.Take(config.MineCount);
            if (retries++ >= MaxRetries) { break; }
        }
        while (!_solvabilityChecker.IsSolvable(config, firstClick, mines));
        return mines;
    }

    /// <summary>
    /// 洗牌引擎, 用于随机打乱数组元素的顺序
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="original">原始数组</param>
    private sealed class ShuffleEngine<T>(T[] original)
    {
        /// <summary>
        /// 原始数组
        /// </summary>
        private readonly T[] _original = original;

        /// <summary>
        /// 获取打乱后的结果
        /// </summary>
        public T[] Result { get; } = new T[original.Length];

        /// <summary>
        /// 使用指定的随机数种子打乱数组元素的顺序
        /// </summary>
        /// <param name="seed">随机数种子</param>
        public void Shuffle(int seed)
        {
            // 创建指定种子的随机数生成器
            var random = new Random(seed);

            // 将原始数组复制到缓冲数组中
            Array.Copy(_original, Result, _original.Length);

            // 使用随机数生成器打乱缓冲数组的顺序
            random.Shuffle(Result);
        }
    }
}
