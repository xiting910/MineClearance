using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MineClearance.Core.Services;

/// <summary>
/// 地雷求解器实现类, 在玩家点击可能踩雷的格子时, 尝试重排雷位使该格子安全翻开, 从而实现无猜体验
/// </summary>
internal sealed class MineSolver : IMineSolver
{
    /// <summary>
    /// 每次求解的搜索节点上限, 超过后保守判定为必死格, 防止极端局面导致卡顿
    /// </summary>
    private const long MaxSearchNodes = 1000000;

    /// <summary>
    /// 边界变量的赋值状态
    /// </summary>
    private enum AssignmentState : byte
    {
        /// <summary>
        /// 未定
        /// </summary>
        Undecided,

        /// <summary>
        /// 安全
        /// </summary>
        Safe,

        /// <summary>
        /// 地雷
        /// </summary>
        Mine
    }

    /// <inheritdoc/>
    public bool TrySafeOpen(
        Position target,
        GameConfig config,
        IMineField mineField,
        IGameBoardDictionary board,
        [NotNullWhen(true)] out BitArray? rearrangedMines,
        [NotNullWhen(false)] out HashSet<Position>? guaranteedSafePositions)
    {
        rearrangedMines = default;
        guaranteedSafePositions = default;

        var (height, width, mineCount) = config;
        var constraints = new List<(Position Position, int Count)>();
        var frontier = new List<Position>();
        var frontierSet = new HashSet<Position>();

        foreach (var (position, cell) in board)
        {
            if (cell.Type is not (CellType.Number or CellType.WarningNumber)) { continue; }

            constraints.Add((position, mineField.GetAdjacentMineCount(position)));
            foreach (var adjacent in position.GetAdjacentPositions(height, width))
            {
                if (board[adjacent].Type is CellType.Unopened or CellType.Flagged or CellType.Question
                    && frontierSet.Add(adjacent))
                {
                    frontier.Add(adjacent);
                }
            }
        }

        var freeCells = new List<Position>();
        foreach (var (position, cell) in board)
        {
            if (cell.Type is CellType.Unopened or CellType.Flagged or CellType.Question
                && !frontierSet.Contains(position))
            {
                freeCells.Add(position);
            }
        }
        freeCells.Sort((a, b) => CompareByTarget(a, b, target));
        frontier.Sort((a, b) => CompareByTarget(a, b, target));

        var variableCount = frontier.Count;
        var constraintCount = constraints.Count;
        var variableIndex = new Dictionary<Position, int>(variableCount);
        for (var i = 0; i < variableCount; i++)
        {
            variableIndex[frontier[i]] = i;
        }

        var targets = new int[constraintCount];
        var neighbors = new List<int>[constraintCount];
        var variableConstraints = new List<int>[variableCount];
        for (var i = 0; i < variableCount; i++)
        {
            variableConstraints[i] = [];
        }
        for (var c = 0; c < constraintCount; c++)
        {
            targets[c] = constraints[c].Count;
            neighbors[c] = [..
                constraints[c].Position
                .GetAdjacentPositions(height, width)
                .Where(variableIndex.ContainsKey)
                .Select(pos => variableIndex[pos])
            ];
            foreach (var v in neighbors[c])
            {
                variableConstraints[v].Add(c);
            }
        }

        var freeCapacity = freeCells.Count - (frontierSet.Contains(target) ? 0 : 1);
        if (freeCapacity < 0)
        {
            guaranteedSafePositions = [];
            return false;
        }

        var baseRem = new int[constraintCount];
        for (var c = 0; c < constraintCount; c++)
        {
            baseRem[c] = neighbors[c].Count;
        }

        var checkNodes = 0L;
        var safePositions = new HashSet<Position>();
        for (var i = 0; i < variableCount; i++)
        {
            if (!CanBeMine(i))
            {
                _ = safePositions.Add(frontier[i]);
            }
        }
        if (safePositions.Count > 0)
        {
            guaranteedSafePositions = safePositions;
            return false;
        }

        if (!frontierSet.Contains(target))
        {
            // freeCells 已按距离排序, 取第一个可用的自由格交换雷位
            foreach (var position in freeCells)
            {
                if (position == target || mineField.IsMine(position)) { continue; }

                var newMap = mineField.GetMineMap();
                newMap[target.ToIndex(width)] = false;
                newMap[position.ToIndex(width)] = true;
                rearrangedMines = newMap;
                return true;
            }
        }

        var assignment = new AssignmentState[variableCount];
        var cur = new int[constraintCount];
        var rem = (int[])baseRem.Clone();
        var minesPlaced = 0;
        var undecided = variableCount;
        var searchNodes = 0L;

        var originalMines = new bool[variableCount];
        for (var i = 0; i < variableCount; i++)
        {
            originalMines[i] = mineField.IsMine(frontier[i]);
        }

        if (variableIndex.TryGetValue(target, out var targetIndex))
        {
            assignment[targetIndex] = AssignmentState.Safe;
            undecided--;
            foreach (var c in variableConstraints[targetIndex])
            {
                rem[c]--;
            }
        }

        bool TrySafe(int v)
        {
            assignment[v] = AssignmentState.Safe;
            undecided--;
            var pruned = false;
            foreach (var c in variableConstraints[v])
            {
                rem[c]--;
                if (cur[c] > targets[c] || cur[c] + rem[c] < targets[c]) { pruned = true; }
            }
            if (!pruned && Search()) { return true; }
            foreach (var c in variableConstraints[v])
            {
                rem[c]++;
            }
            assignment[v] = AssignmentState.Undecided;
            undecided++;
            return false;
        }

        bool TryMine(int v)
        {
            assignment[v] = AssignmentState.Mine;
            minesPlaced++;
            undecided--;
            var pruned = minesPlaced > mineCount || minesPlaced + undecided < mineCount - freeCapacity;
            foreach (var c in variableConstraints[v])
            {
                cur[c]++;
                rem[c]--;
                if (cur[c] > targets[c] || cur[c] + rem[c] < targets[c]) { pruned = true; }
            }
            if (!pruned && Search()) { return true; }
            foreach (var c in variableConstraints[v])
            {
                cur[c]--;
                rem[c]++;
            }
            assignment[v] = AssignmentState.Undecided;
            minesPlaced--;
            undecided++;
            return false;
        }

        bool Search()
        {
            if (++searchNodes > MaxSearchNodes) { return false; }

            var v = -1;
            for (var i = 0; i < variableCount; i++)
            {
                if (assignment[i] is AssignmentState.Undecided) { v = i; break; }
            }
            if (v < 0)
            {
                return minesPlaced <= mineCount && mineCount - minesPlaced <= freeCapacity;
            }

            // 优先尝试原布局的雷位, 以减少雷位移动
            return originalMines[v] ? TryMine(v) || TrySafe(v) : TrySafe(v) || TryMine(v);
        }

        if (!Search())
        {
            guaranteedSafePositions = [];
            return false;
        }

        var result = new BitArray(mineField.GetMineMap());
        for (var i = 0; i < variableCount; i++)
        {
            result[frontier[i].ToIndex(width)] = assignment[i] is AssignmentState.Mine;
        }
        if (!frontierSet.Contains(target))
        {
            result[target.ToIndex(width)] = false;
        }
        var frontierMines = 0;
        for (var i = 0; i < variableCount; i++)
        {
            if (assignment[i] is AssignmentState.Mine) { frontierMines++; }
        }
        var neededFreeMines = mineCount - frontierMines;
        var currentFreeMines = freeCells.Count(position => result[position.ToIndex(width)]);
        if (currentFreeMines > neededFreeMines)
        {
            foreach (var position in freeCells)
            {
                if (currentFreeMines == neededFreeMines) { break; }
                if (result[position.ToIndex(width)])
                {
                    result[position.ToIndex(width)] = false;
                    currentFreeMines--;
                }
            }
        }
        else if (currentFreeMines < neededFreeMines)
        {
            foreach (var position in freeCells)
            {
                if (currentFreeMines == neededFreeMines) { break; }
                if (!result[position.ToIndex(width)])
                {
                    result[position.ToIndex(width)] = true;
                    currentFreeMines++;
                }
            }
        }
        rearrangedMines = result;
        return true;

        bool CanBeMine(int fixedIndex)
        {
            var subAssignment = new AssignmentState[variableCount];
            var subCur = new int[constraintCount];
            var subRem = (int[])baseRem.Clone();
            var subMinesPlaced = 1;
            var subUndecided = variableCount - 1;
            subAssignment[fixedIndex] = AssignmentState.Mine;
            foreach (var c in variableConstraints[fixedIndex])
            {
                subCur[c]++;
                subRem[c]--;
            }

            bool SubSearch()
            {
                if (++checkNodes > MaxSearchNodes) { return true; }

                var v = -1;
                for (var i = 0; i < variableCount; i++)
                {
                    if (subAssignment[i] is AssignmentState.Undecided) { v = i; break; }
                }
                if (v < 0)
                {
                    return subMinesPlaced <= mineCount && mineCount - subMinesPlaced <= freeCapacity;
                }

                subAssignment[v] = AssignmentState.Safe;
                subUndecided--;
                var pruned = false;
                foreach (var c in variableConstraints[v])
                {
                    subRem[c]--;
                    if (subCur[c] > targets[c] || subCur[c] + subRem[c] < targets[c]) { pruned = true; }
                }
                if (!pruned && SubSearch()) { return true; }
                foreach (var c in variableConstraints[v])
                {
                    subRem[c]++;
                }
                subAssignment[v] = AssignmentState.Undecided;
                subUndecided++;

                subAssignment[v] = AssignmentState.Mine;
                subMinesPlaced++;
                subUndecided--;
                if (subMinesPlaced <= mineCount && subMinesPlaced + subUndecided >= mineCount - freeCapacity)
                {
                    pruned = false;
                    foreach (var c in variableConstraints[v])
                    {
                        subCur[c]++;
                        subRem[c]--;
                        if (subCur[c] > targets[c] || subCur[c] + subRem[c] < targets[c]) { pruned = true; }
                    }
                    if (!pruned && SubSearch()) { return true; }
                    foreach (var c in variableConstraints[v])
                    {
                        subCur[c]--;
                        subRem[c]++;
                    }
                }
                subAssignment[v] = AssignmentState.Undecided;
                subMinesPlaced--;
                subUndecided++;

                return false;
            }

            return SubSearch();
        }
    }

    /// <summary>
    /// 按与目标格的距离比较两个位置: 切比雪夫距离升序, 同距离按从正上方开始的顺时针方向序,
    /// 距离大于 1 的格按行列序兜底, 避免雷位变动偏向特定行列
    /// </summary>
    /// <param name="a">第一个位置</param>
    /// <param name="b">第二个位置</param>
    /// <param name="target">目标格</param>
    /// <returns>比较结果</returns>
    private static int CompareByTarget(Position a, Position b, Position target)
    {
        var distanceA = Math.Max(Math.Abs(a.Row - target.Row), Math.Abs(a.Col - target.Col));
        var distanceB = Math.Max(Math.Abs(b.Row - target.Row), Math.Abs(b.Col - target.Col));
        if (distanceA != distanceB) { return distanceA.CompareTo(distanceB); }

        var directionA = DirectionIndex(a, target);
        var directionB = DirectionIndex(b, target);
        return directionA != directionB
            ? directionA.CompareTo(directionB)
            : a.Row != b.Row
                ? a.Row.CompareTo(b.Row)
                : a.Col.CompareTo(b.Col);
    }

    /// <summary>
    /// 获取位置相对于目标格的方向索引, 按从正上方开始的顺时针方向排列
    /// </summary>
    /// <param name="position">要计算方向的位置</param>
    /// <param name="target">目标格</param>
    /// <returns>方向索引</returns>
    private static int DirectionIndex(Position position, Position target)
    {
        var offset = position - target;
        for (var i = 0; i < Position.DirectionOffsets.Length; i++)
        {
            if (offset == Position.DirectionOffsets[i]) { return i; }
        }
        return Position.DirectionOffsets.Length;
    }
}
