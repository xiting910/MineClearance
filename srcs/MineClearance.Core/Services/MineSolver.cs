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

    /// <summary>
    /// 无猜求解的回溯搜索状态, 封装搜索过程所需的中间变量与回溯逻辑
    /// </summary>
    private sealed class SearchState
    {
        /// <summary>
        /// 变量(前沿格)总数
        /// </summary>
        private readonly int _variableCount;

        /// <summary>
        /// 每个约束要求的相邻地雷数
        /// </summary>
        private readonly int[] _targets;

        /// <summary>
        /// 每个变量的相邻约束索引列表
        /// </summary>
        private readonly List<int>[] _variableConstraints;

        /// <summary>
        /// 变量在原布局中是否为地雷, 搜索时优先尝试原布局以少动雷位
        /// </summary>
        private readonly bool[] _originalMines;

        /// <summary>
        /// 自由格可容纳的地雷数
        /// </summary>
        private readonly int _freeCapacity;

        /// <summary>
        /// 总地雷数
        /// </summary>
        private readonly int _mineCount;

        /// <summary>
        /// 每个变量的当前赋值
        /// </summary>
        private readonly AssignmentState[] _assignment;

        /// <summary>
        /// 每个约束当前已分配的地雷数
        /// </summary>
        private readonly int[] _cur;

        /// <summary>
        /// 每个约束当前剩余的未决变量数
        /// </summary>
        private readonly int[] _rem;

        /// <summary>
        /// 已放置的地雷数
        /// </summary>
        private int _minesPlaced;

        /// <summary>
        /// 未决变量数
        /// </summary>
        private int _undecided;

        /// <summary>
        /// 已搜索的节点数, 超过上限时停止搜索
        /// </summary>
        private long _searchNodes;

        /// <summary>
        /// 初始化搜索状态, 并将目标格(若在前沿中)预先赋值为安全
        /// </summary>
        /// <param name="variableCount">变量总数</param>
        /// <param name="constraintCount">约束总数</param>
        /// <param name="targets">每个约束要求的相邻地雷数</param>
        /// <param name="baseRem">每个约束的初始剩余变量数</param>
        /// <param name="variableConstraints">每个变量的相邻约束索引列表</param>
        /// <param name="originalMines">变量在原布局中是否为地雷</param>
        /// <param name="freeCapacity">自由格可容纳的地雷数</param>
        /// <param name="mineCount">总地雷数</param>
        /// <param name="targetIndex">目标格对应的变量索引, 不在前沿中时为 -1</param>
        public SearchState(
            int variableCount,
            int constraintCount,
            int[] targets,
            int[] baseRem,
            List<int>[] variableConstraints,
            bool[] originalMines,
            int freeCapacity,
            int mineCount,
            int targetIndex)
        {
            _variableCount = variableCount;
            _targets = targets;
            _variableConstraints = variableConstraints;
            _originalMines = originalMines;
            _freeCapacity = freeCapacity;
            _mineCount = mineCount;
            _assignment = new AssignmentState[variableCount];
            _cur = new int[constraintCount];
            _rem = (int[])baseRem.Clone();
            _minesPlaced = 0;
            _undecided = variableCount;

            if (targetIndex >= 0)
            {
                _assignment[targetIndex] = AssignmentState.Safe;
                _undecided--;
                foreach (var c in _variableConstraints[targetIndex])
                {
                    _rem[c]--;
                }
            }
        }

        /// <summary>
        /// 深度优先搜索是否存在满足所有约束的完整布局
        /// </summary>
        /// <returns>存在可行布局时返回 <see langword="true"/></returns>
        public bool Search()
        {
            if (++_searchNodes > MaxSearchNodes) { return false; }

            var v = -1;
            for (var i = 0; i < _variableCount; i++)
            {
                if (_assignment[i] is AssignmentState.Undecided) { v = i; break; }
            }
            if (v < 0)
            {
                return _minesPlaced <= _mineCount && _mineCount - _minesPlaced <= _freeCapacity;
            }

            // 优先尝试原布局的雷位, 以减少雷位移动
            return _originalMines[v] ? TryMine(v) || TrySafe(v) : TrySafe(v) || TryMine(v);
        }

        /// <summary>
        /// 获取变量是否被赋值为地雷
        /// </summary>
        /// <param name="index">变量索引</param>
        /// <returns>是地雷时返回 <see langword="true"/></returns>
        public bool IsMine(int index)
        {
            return _assignment[index] is AssignmentState.Mine;
        }

        /// <summary>
        /// 尝试将变量赋值为安全并继续搜索, 失败时回滚赋值
        /// </summary>
        /// <param name="v">变量索引</param>
        /// <returns>搜索成功时返回 <see langword="true"/></returns>
        private bool TrySafe(int v)
        {
            _assignment[v] = AssignmentState.Safe;
            _undecided--;
            var pruned = false;
            foreach (var c in _variableConstraints[v])
            {
                _rem[c]--;
                if (_cur[c] > _targets[c] || _cur[c] + _rem[c] < _targets[c]) { pruned = true; }
            }
            if (!pruned && Search()) { return true; }
            foreach (var c in _variableConstraints[v])
            {
                _rem[c]++;
            }
            _assignment[v] = AssignmentState.Undecided;
            _undecided++;
            return false;
        }

        /// <summary>
        /// 尝试将变量赋值为地雷并继续搜索, 失败时回滚赋值
        /// </summary>
        /// <param name="v">变量索引</param>
        /// <returns>搜索成功时返回 <see langword="true"/></returns>
        private bool TryMine(int v)
        {
            _assignment[v] = AssignmentState.Mine;
            _minesPlaced++;
            _undecided--;
            var pruned = _minesPlaced > _mineCount || _minesPlaced + _undecided < _mineCount - _freeCapacity;
            foreach (var c in _variableConstraints[v])
            {
                _cur[c]++;
                _rem[c]--;
                if (_cur[c] > _targets[c] || _cur[c] + _rem[c] < _targets[c]) { pruned = true; }
            }
            if (!pruned && Search()) { return true; }
            foreach (var c in _variableConstraints[v])
            {
                _cur[c]--;
                _rem[c]++;
            }
            _assignment[v] = AssignmentState.Undecided;
            _minesPlaced--;
            _undecided++;
            return false;
        }
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
        var (constraints, frontier, frontierSet) = CollectConstraints(mineField, board, height, width);
        var freeCells = CollectFreeCells(board, frontierSet);
        freeCells.Sort((a, b) => CompareByTarget(a, b, target));
        frontier.Sort((a, b) => CompareByTarget(a, b, target));

        var variableCount = frontier.Count;
        var constraintCount = constraints.Count;
        var (targets, baseRem, variableConstraints, variableIndex) = BuildSolverIndex(constraints, frontier, height, width);

        var freeCapacity = freeCells.Count - (frontierSet.Contains(target) ? 0 : 1);
        if (freeCapacity < 0)
        {
            guaranteedSafePositions = [];
            return false;
        }

        var safePositions = new HashSet<Position>();
        for (var i = 0; i < variableCount; i++)
        {
            if (!CanBeMine(i, variableCount, constraintCount, targets, baseRem, variableConstraints, freeCapacity, mineCount))
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

        var originalMines = new bool[variableCount];
        for (var i = 0; i < variableCount; i++)
        {
            originalMines[i] = mineField.IsMine(frontier[i]);
        }

        var search = new SearchState(
            variableCount,
            constraintCount,
            targets,
            baseRem,
            variableConstraints,
            originalMines,
            freeCapacity,
            mineCount,
            variableIndex.TryGetValue(target, out var targetIndex) ? targetIndex : -1
        );
        if (!search.Search())
        {
            guaranteedSafePositions = [];
            return false;
        }

        var result = new BitArray(mineField.GetMineMap());
        for (var i = 0; i < variableCount; i++)
        {
            result[frontier[i].ToIndex(width)] = search.IsMine(i);
        }
        if (!frontierSet.Contains(target))
        {
            result[target.ToIndex(width)] = false;
        }
        var frontierMines = 0;
        for (var i = 0; i < variableCount; i++)
        {
            if (search.IsMine(i)) { frontierMines++; }
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
    }

    /// <summary>
    /// 收集所有已翻开数字格的约束, 以及与其相邻的未翻开格(搜索变量)
    /// </summary>
    /// <param name="mineField"><see cref="IMineField"/> 雷场</param>
    /// <param name="board"><see cref="IGameBoardDictionary"/> 棋盘字典</param>
    /// <param name="height">棋盘高度</param>
    /// <param name="width">棋盘宽度</param>
    /// <returns>约束列表, 前沿格列表及其集合</returns>
    private static (List<(Position Position, int Count)> Constraints, List<Position> Frontier,
    HashSet<Position> FrontierSet) CollectConstraints(
        IMineField mineField,
        IGameBoardDictionary board,
        int height,
        int width)
    {
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

        return (constraints, frontier, frontierSet);
    }

    /// <summary>
    /// 收集不与任何已翻开数字格相邻的未翻开格(自由格)
    /// </summary>
    /// <param name="board"><see cref="IGameBoardDictionary"/> 棋盘字典</param>
    /// <param name="frontierSet">前沿格集合</param>
    /// <returns>自由格列表</returns>
    private static List<Position> CollectFreeCells(IGameBoardDictionary board, HashSet<Position> frontierSet)
    {
        var freeCells = new List<Position>();
        foreach (var (position, cell) in board)
        {
            if (cell.Type is CellType.Unopened or CellType.Flagged or CellType.Question
                && !frontierSet.Contains(position))
            {
                freeCells.Add(position);
            }
        }
        return freeCells;
    }

    /// <summary>
    /// 为搜索构建变量索引与约束邻接结构
    /// </summary>
    /// <param name="constraints">约束列表</param>
    /// <param name="frontier">前沿格列表</param>
    /// <param name="height">棋盘高度</param>
    /// <param name="width">棋盘宽度</param>
    /// <returns>约束目标地雷数, 约束初始剩余变量数, 变量的相邻约束索引列表, 位置到变量索引的映射</returns>
    private static (int[] Targets, int[] BaseRem, List<int>[] VariableConstraints,
    Dictionary<Position, int> VariableIndex) BuildSolverIndex(
        List<(Position Position, int Count)> constraints,
        List<Position> frontier,
        int height,
        int width)
    {
        var variableCount = frontier.Count;
        var constraintCount = constraints.Count;
        var variableIndex = new Dictionary<Position, int>(variableCount);
        for (var i = 0; i < variableCount; i++)
        {
            variableIndex[frontier[i]] = i;
        }

        var targets = new int[constraintCount];
        var baseRem = new int[constraintCount];
        var variableConstraints = new List<int>[variableCount];
        for (var i = 0; i < variableCount; i++)
        {
            variableConstraints[i] = [];
        }
        for (var c = 0; c < constraintCount; c++)
        {
            targets[c] = constraints[c].Count;
            var neighborCount = 0;
            foreach (var v in constraints[c].Position
                .GetAdjacentPositions(height, width)
                .Where(variableIndex.ContainsKey)
                .Select(pos => variableIndex[pos]))
            {
                neighborCount++;
                variableConstraints[v].Add(c);
            }
            baseRem[c] = neighborCount;
        }

        return (targets, baseRem, variableConstraints, variableIndex);
    }

    /// <summary>
    /// 检查将指定变量固定为地雷时是否存在可行布局, 不存在时说明该变量对应的格必安全
    /// </summary>
    /// <param name="fixedIndex">固定为地雷的变量索引</param>
    /// <param name="variableCount">变量总数</param>
    /// <param name="constraintCount">约束总数</param>
    /// <param name="targets">每个约束的相邻地雷目标数</param>
    /// <param name="baseRem">每个约束的初始剩余变量数</param>
    /// <param name="variableConstraints">每个变量的相邻约束索引列表</param>
    /// <param name="freeCapacity">自由格可容纳的地雷数</param>
    /// <param name="mineCount">总地雷数</param>
    /// <returns>存在可行布局时返回 <see langword="true"/>, 不存在时该格必安全</returns>
    private static bool CanBeMine(
        int fixedIndex,
        int variableCount,
        int constraintCount,
        int[] targets,
        int[] baseRem,
        List<int>[] variableConstraints,
        int freeCapacity,
        int mineCount)
    {
        var checkNodes = 0L;
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
        return SubSearch(ref subMinesPlaced, ref subUndecided);

        bool SubSearch(ref int minesPlaced, ref int undecided)
        {
            if (++checkNodes > MaxSearchNodes) { return true; }

            var v = -1;
            for (var i = 0; i < variableCount; i++)
            {
                if (subAssignment[i] is AssignmentState.Undecided) { v = i; break; }
            }
            if (v < 0)
            {
                return minesPlaced <= mineCount && mineCount - minesPlaced <= freeCapacity;
            }

            subAssignment[v] = AssignmentState.Safe;
            undecided--;
            var pruned = false;
            foreach (var c in variableConstraints[v])
            {
                subRem[c]--;
                if (subCur[c] > targets[c] || subCur[c] + subRem[c] < targets[c]) { pruned = true; }
            }
            if (!pruned && SubSearch(ref minesPlaced, ref undecided)) { return true; }
            foreach (var c in variableConstraints[v])
            {
                subRem[c]++;
            }
            subAssignment[v] = AssignmentState.Undecided;
            undecided++;

            subAssignment[v] = AssignmentState.Mine;
            minesPlaced++;
            undecided--;
            if (minesPlaced <= mineCount && minesPlaced + undecided >= mineCount - freeCapacity)
            {
                pruned = false;
                foreach (var c in variableConstraints[v])
                {
                    subCur[c]++;
                    subRem[c]--;
                    if (subCur[c] > targets[c] || subCur[c] + subRem[c] < targets[c]) { pruned = true; }
                }
                if (!pruned && SubSearch(ref minesPlaced, ref undecided)) { return true; }
                foreach (var c in variableConstraints[v])
                {
                    subCur[c]--;
                    subRem[c]++;
                }
            }
            subAssignment[v] = AssignmentState.Undecided;
            minesPlaced--;
            undecided++;

            return false;
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
