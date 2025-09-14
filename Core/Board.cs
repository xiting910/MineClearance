using MineClearance.Models;
using MineClearance.Models.Enums;

namespace MineClearance.Core;

/// <summary>
/// 表示扫雷游戏的场地
/// </summary>
internal sealed class Board
{
    /// <summary>
    /// 格子状态改变事件, 参数为格子位置
    /// </summary>
    public event Action<Position>? GridChanged;

    /// <summary>
    /// 当剩余地雷数量改变时触发, 参数为改变的数量(正数表示增加, 负数表示减少)
    /// </summary>
    public event Action<int>? RemainingMinesChanged;

    /// <summary>
    /// 当剩余未处理的格子数量改变时触发, 参数为改变的数量(正数表示增加, 负数表示减少)
    /// </summary>
    public event Action<int>? UnopenedCountChanged;

    /// <summary>
    /// 当未打开的安全格子数量改变时触发, 参数为变化后的未打开的安全格子数量
    /// </summary>
    public event Action<int>? UnopenedSafeCountChanged;

    /// <summary>
    /// 当游戏胜利时触发
    /// </summary>
    public event Action? Won;

    /// <summary>
    /// 当打开地雷时触发(游戏失败)
    /// </summary>
    public event Action? HitMine;

    /// <summary>
    /// 首次点击时触发
    /// </summary>
    public event Action? FirstClick;

    /// <summary>
    /// 游戏的宽度
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 游戏的高度
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// 游戏上的格子数组
    /// </summary>
    public Grid[,] Grids { get; }

    /// <summary>
    /// 未打开的非地雷格子数量
    /// </summary>
    public int UnopenedSafeCount
    {
        get => _unopenedSafeCount;
        private set
        {
            _unopenedSafeCount = value;
            UnopenedSafeCountChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// 管理地雷的集合
    /// </summary>
    private readonly Mines _mines;

    /// <summary>
    /// 是否是第一次点击
    /// </summary>
    private bool _isFirstClick;

    /// <summary>
    /// 未打开的安全格子数量字段
    /// </summary>
    private int _unopenedSafeCount;

    /// <summary>
    /// 构造函数, 创建一个新的游戏场地实例
    /// </summary>
    /// <param name="height">游戏高度</param>
    /// <param name="width">游戏宽度</param>
    /// <param name="mineCount">地雷数量</param>
    public Board(int width, int height, int mineCount)
    {
        Width = width;
        Height = height;
        Grids = new Grid[height, width];
        for (var row = 0; row < height; ++row)
        {
            for (var column = 0; column < width; ++column)
            {
                Grids[row, column] = new();
            }
        }

        _unopenedSafeCount = (height * width) - mineCount;
        _mines = new(width, height, mineCount);
        _isFirstClick = true;
    }

    /// <summary>
    /// 判断某个位置是不是地雷
    /// </summary>
    /// <param name="row">行索引</param>
    /// <param name="col">列索引</param>
    /// <returns>如果是地雷返回 true, 否则返回 false</returns>
    public bool IsMine(int row, int col)
    {
        return _mines.MineGrid[row, col] == -1;
    }

    /// <summary>
    /// 左键点击格子
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    public void OnLeftClick(Position position)
    {
        // 检查点击位置是否在游戏范围内
        if (position.Row < 0 || position.Row >= Height || position.Col < 0 || position.Col >= Width)
        {
            return;
        }

        // 如果是第一次点击
        if (_isFirstClick)
        {
            _mines.GenerateMines(position);
            _isFirstClick = false;
            FirstClick?.Invoke();
        }

        // 如果点击的格子为未打开状态, 则打开格子
        if (Grids[position.Row, position.Col].Type == GridType.Unopened)
        {
            OpenGrid(position);
            return;
        }

        // 如果点击的格子为数字格子, 且周围已经插旗的数量等于数字格子显示的数字, 则打开周围未插旗的格子
        if (Grids[position.Row, position.Col].Type == GridType.Number)
        {
            var flaggedCount = GetSurroundingGridCount(position, GridType.Flagged);

            if (flaggedCount == Grids[position.Row, position.Col].SurroundingMines)
            {
                for (var r = position.Row - 1; r <= position.Row + 1; ++r)
                {
                    for (var c = position.Col - 1; c <= position.Col + 1; ++c)
                    {
                        if (r >= 0 && r < Height && c >= 0 && c < Width)
                        {
                            OpenGrid(new(r, c));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 右键点击格子
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    public void OnRightClick(Position position)
    {
        // 检查点击位置是否在游戏范围内
        if (position.Row < 0 || position.Row >= Height || position.Col < 0 || position.Col >= Width)
        {
            return;
        }

        // 如果是第一次点击, 则不处理右键点击
        if (_isFirstClick)
        {
            return;
        }

        // 如果点击的格子是空白格子, 则不处理
        if (Grids[position.Row, position.Col].Type == GridType.Empty)
        {
            return;
        }

        if (Grids[position.Row, position.Col].Type == GridType.Unopened)
        {
            // 插旗
            Grids[position.Row, position.Col].Type = GridType.Flagged;
            GridChanged?.Invoke(position);
            CheckSurroundingNumberGrids(position);
            RemainingMinesChanged?.Invoke(-1);
            UnopenedCountChanged?.Invoke(-1);
        }
        else if (Grids[position.Row, position.Col].Type == GridType.Flagged)
        {
            // 取消插旗
            Grids[position.Row, position.Col].Type = GridType.Unopened;
            GridChanged?.Invoke(position);
            CheckSurroundingNumberGrids(position);
            RemainingMinesChanged?.Invoke(1);
            UnopenedCountChanged?.Invoke(1);
        }
        else if (Grids[position.Row, position.Col].Type == GridType.Number)
        {
            // 如果点击的是数字格子, 则检查周围插旗和未打开的格子数量
            var count = GetSurroundingGridCount(position, GridType.Flagged, GridType.Unopened);

            // 如果周围插旗数量和未打开数量加起来等于数字格子显示的数字, 则插旗所有未打开的格子
            if (count == Grids[position.Row, position.Col].SurroundingMines)
            {
                for (var r = position.Row - 1; r <= position.Row + 1; ++r)
                {
                    for (var c = position.Col - 1; c <= position.Col + 1; ++c)
                    {
                        if (r >= 0 && r < Height && c >= 0 && c < Width)
                        {
                            if (Grids[r, c].Type == GridType.Unopened)
                            {
                                var pos = new Position(r, c);
                                Grids[r, c].Type = GridType.Flagged;
                                GridChanged?.Invoke(pos);
                                CheckSurroundingNumberGrids(pos);
                                RemainingMinesChanged?.Invoke(-1);
                                UnopenedCountChanged?.Invoke(-1);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 插旗或取消插旗后, 检测并更新其周围的数字格子状态
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    private void CheckSurroundingNumberGrids(Position position)
    {
        for (var r = position.Row - 1; r <= position.Row + 1; ++r)
        {
            for (var c = position.Col - 1; c <= position.Col + 1; ++c)
            {
                if (r >= 0 && r < Height && c >= 0 && c < Width)
                {
                    if (Grids[r, c].Type is GridType.Number or GridType.WarningNumber)
                    {
                        CheckNumberGrid(new(r, c));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获得某个格子及其周围指定类型的格子数量, 支持指定多种类型
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    /// <param name="types">要统计的格子类型</param>
    /// <returns>周围指定类型的格子数量</returns>
    private int GetSurroundingGridCount(Position position, params GridType[] types)
    {
        var count = 0;
        for (var r = position.Row - 1; r <= position.Row + 1; ++r)
        {
            for (var c = position.Col - 1; c <= position.Col + 1; ++c)
            {
                if (r >= 0 && r < Height && c >= 0 && c < Width)
                {
                    if (types.Contains(Grids[r, c].Type))
                    {
                        ++count;
                    }
                }
            }
        }
        return count;
    }

    /// <summary>
    /// 检测数字格子
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    /// <exception cref="ArgumentOutOfRangeException">如果位置不在游戏范围内则抛出</exception>
    /// <exception cref="ArgumentException">如果格子不是数字格子则抛出</exception>
    private void CheckNumberGrid(Position position)
    {
        // 检查位置是否在游戏范围内
        if (position.Row < 0 || position.Row >= Height || position.Col < 0 || position.Col >= Width)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "位置不在游戏范围内");
        }

        // 检查格子是否为数字格子
        if (Grids[position.Row, position.Col].Type is not GridType.Number and not GridType.WarningNumber)
        {
            throw new ArgumentException("格子不是数字格子", nameof(position));
        }

        // 检查周围的格子
        var flaggedCount = GetSurroundingGridCount(position, GridType.Flagged);

        // 如果周围插旗的数量超过了实际地雷数量, 则将格子类型改为警告数字格子
        if (flaggedCount > Grids[position.Row, position.Col].SurroundingMines)
        {
            Grids[position.Row, position.Col].Type = GridType.WarningNumber;
            GridChanged?.Invoke(position);
        }
        // 否则将格子类型改为数字格子
        else
        {
            Grids[position.Row, position.Col].Type = GridType.Number;
            GridChanged?.Invoke(position);
        }
    }

    /// <summary>
    /// 打开格子, 如果为地雷则触发地雷事件, 如果为空白则打开周围格子(自动检测是否胜利)
    /// </summary>
    /// <param name="position">格子的行列索引</param>
    private void OpenGrid(Position position)
    {
        // 如果点击的格子不是未打开状态, 则不处理
        if (Grids[position.Row, position.Col].Type != GridType.Unopened)
        {
            return;
        }

        // 如果点击的格子是地雷
        if (_mines.MineGrid[position.Row, position.Col] == -1)
        {
            Grids[position.Row, position.Col].Type = GridType.Mine;
            HitMine?.Invoke();
            return;
        }

        // BFS 队列替代递归
        var queue = new Queue<Position>();
        queue.Enqueue(position);

        // 队列不为空时循环处理
        while (queue.Count > 0)
        {
            // 取出队列中的位置
            var pos = queue.Dequeue();

            // 跳过已处理的格子
            if (Grids[pos.Row, pos.Col].Type != GridType.Unopened)
            {
                continue;
            }

            // 更新未打开的安全格子数量
            --UnopenedSafeCount;
            UnopenedCountChanged?.Invoke(-1);

            // 更新格子周围地雷数量
            var surroundingMines = _mines.MineGrid[pos.Row, pos.Col];
            Grids[pos.Row, pos.Col].SurroundingMines = surroundingMines;

            if (surroundingMines == 0)
            {
                // 如果是空白格子, 则将其类型改为 Empty并触发格子改变事件
                Grids[pos.Row, pos.Col].Type = GridType.Empty;
                GridChanged?.Invoke(pos);

                // 将周围未打开的格子加入队列
                for (var r = pos.Row - 1; r <= pos.Row + 1; ++r)
                {
                    for (var c = pos.Col - 1; c <= pos.Col + 1; ++c)
                    {
                        if (r >= 0 && r < Height && c >= 0 && c < Width)
                        {
                            if (Grids[r, c].Type == GridType.Unopened)
                            {
                                queue.Enqueue(new(r, c));
                            }
                        }
                    }
                }
            }
            else
            {
                // 如果是数字格子, 则将其类型改为 Number, 并检查周围格子
                Grids[pos.Row, pos.Col].Type = GridType.Number;
                CheckNumberGrid(pos);

                // 如果检测后依然是数字格子, 则触发格子改变事件
                if (Grids[pos.Row, pos.Col].Type == GridType.Number)
                {
                    GridChanged?.Invoke(pos);
                }
            }
        }

        // 如果未打开的安全格子数量等于0
        if (_unopenedSafeCount == 0)
        {
            // 触发游戏胜利事件
            Won?.Invoke();
        }
    }
}