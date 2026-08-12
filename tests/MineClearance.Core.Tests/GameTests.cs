using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.Core.Services;
using Moq;
using System.Collections;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="Game"/> 的单元测试, 覆盖状态机流转、格子操作、胜负判定、存档与释放
/// </summary>
public sealed class GameTests
{
    /// <summary>
    /// 棋盘行数
    /// </summary>
    private const int Rows = 5;

    /// <summary>
    /// 棋盘列数
    /// </summary>
    private const int Columns = 5;

    /// <summary>
    /// 测试用的自定义配置, 2 颗地雷
    /// </summary>
    private static readonly GameConfig Config = new(Rows, Columns, 2);

    /// <summary>
    /// 测试用的随机种子
    /// </summary>
    private const int Seed = 42;

    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    /// <summary>
    /// 真实棋盘字典工厂, 用于创建可实际操作的棋盘
    /// </summary>
    private static readonly GameBoardDictionaryFactory BoardFactory = new();

    /// <summary>
    /// 地雷布局: (1,1) 和 (1,3) 是地雷, 其余位置是周围地雷数量, 按行优先顺序排列
    /// </summary>
    private static readonly int[] AdjacentMineCounts =
    [
        1, 1, 2, 1, 1,
        1, -1, 2, -1, 1,
        1, 1, 2, 1, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
    ];

    /// <summary>
    /// 判断指定位置是否是地雷
    /// </summary>
    /// <param name="position">格子位置</param>
    /// <returns><see langword="true"/> 如果是地雷, 否则为 <see langword="false"/></returns>
    private static bool IsMine(Position position)
    {
        return position is { Row: 1, Col: 1 } or { Row: 1, Col: 3 };
    }

    /// <summary>
    /// 创建地雷场的位图表示
    /// </summary>
    /// <returns>地雷场的位图表示</returns>
    private static BitArray CreateMineMap()
    {
        var mineMap = new BitArray(Rows * Columns);
        mineMap[(1 * Columns) + 1] = true;
        mineMap[(1 * Columns) + 3] = true;
        return mineMap;
    }

    /// <summary>
    /// 创建地雷场模拟, 固定返回测试布局
    /// </summary>
    /// <returns>地雷场模拟</returns>
    private static Mock<IMineField> CreateMineFieldMock()
    {
        var mock = new Mock<IMineField>();
        _ = mock.Setup(m => m.Generate(It.IsAny<GameConfig>(), It.IsAny<Position>(), It.IsAny<int>()))
            .Returns(() => (int[])AdjacentMineCounts.Clone());
        _ = mock.Setup(m => m.Generate(It.IsAny<GameConfig>(), It.IsAny<BitArray>()))
            .Returns(() => (int[])AdjacentMineCounts.Clone());
        _ = mock.Setup(m => m.IsMine(It.IsAny<Position>()))
            .Returns((Position position) => IsMine(position));
        _ = mock.Setup(m => m.GetMineMap())
            .Returns(CreateMineMap);
        return mock;
    }

    /// <summary>
    /// 创建计时器模拟, 固定返回开始时间和已用时
    /// </summary>
    /// <returns>计时器模拟</returns>
    private static Mock<IGameTimer> CreateTimerMock()
    {
        var mock = new Mock<IGameTimer>();
        _ = mock.SetupGet(timer => timer.FirstStartTime).Returns(StartTime);
        _ = mock.SetupGet(timer => timer.Elapsed).Returns(TimeSpan.FromMinutes(1));
        return mock;
    }

    /// <summary>
    /// 创建可玩的游戏实例及配套模拟
    /// </summary>
    /// <returns>游戏实例和地雷场、计时器模拟</returns>
    private static (Game Game, Mock<IMineField> MineField, Mock<IGameTimer> Timer) CreatePlayableGame()
    {
        var mineField = CreateMineFieldMock();
        var timer = CreateTimerMock();
        var game = new Game(
            new Mock<IServiceScope>().Object,
            NullLogger<Game>.Instance,
            BoardFactory,
            mineField.Object,
            timer.Object,
            GameDifficulty.Custom,
            Config,
            Seed
        );
        return (game, mineField, timer);
    }

    /// <summary>
    /// 创建游戏实例, 可指定服务作用域模拟
    /// </summary>
    /// <param name="scopeMock">服务作用域模拟</param>
    /// <returns>游戏实例</returns>
    private static Game CreateGame(Mock<IServiceScope>? scopeMock = null)
    {
        return new(
            (scopeMock ?? new Mock<IServiceScope>()).Object,
            NullLogger<Game>.Instance,
            BoardFactory,
            CreateMineFieldMock().Object,
            CreateTimerMock().Object,
            GameDifficulty.Custom,
            Config,
            Seed
        );
    }

    [Fact]
    public void 构造_初始状态_等待开始且无棋盘()
    {
        var (game, _, _) = CreatePlayableGame();

        Assert.Equal(GameStatus.WaitingStarted, game.Status);
        Assert.Null(game.Board);
        Assert.Equal(0, game.Completion);
        Assert.Null(game.Result);
        Assert.Equal(GameDifficulty.Custom, game.Difficulty);
        Assert.Equal(Config, game.Config);
        Assert.Equal(Seed, game.Seed);
        Assert.True(game.IsPerformable);
        Assert.False(game.HasProgress);
    }

    [Fact]
    public void OpenCell_首次点击_创建棋盘并开始计时()
    {
        var (game, mineField, timer) = CreatePlayableGame();

        game.OpenCell(new(0, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.True(game.HasProgress);
        Assert.Equal(CellType.Number, game.Board[new(0, 0)].Type);
        Assert.Equal(1, game.Board.OpenedCount);

        mineField.Verify(m => m.Generate(Config, new(0, 0), Seed), Times.Once);
        timer.Verify(t => t.Start(), Times.Once);
    }

    [Fact]
    public void OpenCell_点击数字格_只打开该格()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(1, game.Board.OpenedCount);
        Assert.Equal(1.0 / 23, game.Completion);
    }

    [Fact]
    public void OpenCell_点击空白格_级联打开相邻区域()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(3, 3));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Empty, game.Board[new(3, 3)].Type);
        Assert.Equal(CellType.Empty, game.Board[new(4, 4)].Type);
        Assert.Equal(CellType.Number, game.Board[new(2, 2)].Type);
        Assert.Equal(CellType.Unopened, game.Board[new(0, 0)].Type);
        Assert.Equal(15, game.Board.OpenedCount);
        Assert.Equal(15.0 / 23, game.Completion);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Pause_游戏进行中_状态变为已暂停()
    {
        var (game, _, timer) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.Pause();

        Assert.Equal(GameStatus.Paused, game.Status);
        Assert.True(game.HasProgress);
        Assert.False(game.IsPerformable);

        timer.Verify(t => t.Pause(), Times.Once);
    }

    [Fact]
    public void Pause_尚未开始_状态变为已暂停()
    {
        var (game, _, timer) = CreatePlayableGame();

        game.Pause();

        Assert.Equal(GameStatus.Paused, game.Status);
        Assert.False(game.HasProgress);

        timer.Verify(t => t.Pause(), Times.Once);
    }

    [Fact]
    public void CancelPause_有棋盘_恢复进行中并继续计时()
    {
        var (game, _, timer) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.Pause();
        game.CancelPause();

        Assert.Equal(GameStatus.InProgress, game.Status);

        timer.Verify(t => t.Start(), Times.Exactly(2));
    }

    [Fact]
    public void CancelPause_无棋盘_回到等待开始且不启动计时器()
    {
        var (game, _, timer) = CreatePlayableGame();

        game.Pause();
        game.CancelPause();

        Assert.Equal(GameStatus.WaitingStarted, game.Status);

        timer.Verify(t => t.Start(), Times.Never);
    }

    [Fact]
    public void OpenCell_状态变化_触发属性变化事件()
    {
        var (game, _, _) = CreatePlayableGame();
        var statusChangedCount = 0;
        var boardChangedCount = 0;
        game.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IGame.Status)) { statusChangedCount++; }
            if (e.PropertyName is nameof(IGame.Board)) { boardChangedCount++; }
        };

        game.OpenCell(new(0, 0));

        Assert.Equal(1, statusChangedCount);
        Assert.Equal(1, boardChangedCount);
    }

    [Fact]
    public void FlagCell_插旗_格子类型变为旗子()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.FlagCell(new(1, 1));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Flagged, game.Board[new(1, 1)].Type);
        Assert.Equal(1, game.Board.FlagCount);
    }

    [Fact]
    public void QuestionCell_标记问号_格子类型变为问号()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.QuestionCell(new(1, 1));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Question, game.Board[new(1, 1)].Type);
        Assert.Equal(1, game.Board.QuestionCount);
    }

    [Fact]
    public void UnmarkCell_取消标记_格子恢复未打开()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.FlagCell(new(1, 1));
        game.UnmarkCell(new(1, 1));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Unopened, game.Board[new(1, 1)].Type);
        Assert.Equal(0, game.Board.FlagCount);
    }

    [Fact]
    public void FlagCell_周围旗数超过雷数_数字格变为警告数字()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 数字 1
        game.FlagCell(new(1, 1)); // 旗数 1, 与数字匹配, 不警告

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Number, game.Board[new(0, 0)].Type);

        game.FlagCell(new(0, 1));
        game.FlagCell(new(1, 0)); // 旗数 3, 超过数字 1

        Assert.Equal(CellType.WarningNumber, game.Board[new(0, 0)].Type);
    }

    [Fact]
    public void UnmarkCell_旗数恢复匹配_警告数字恢复为普通数字()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.FlagCell(new(1, 1));
        game.FlagCell(new(0, 1));
        game.FlagCell(new(1, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.WarningNumber, game.Board[new(0, 0)].Type);

        game.UnmarkCell(new(0, 1));
        game.UnmarkCell(new(1, 0));

        Assert.Equal(CellType.Number, game.Board[new(0, 0)].Type);
    }

    [Fact]
    public void OpenAdjacentCells_旗数匹配_打开周围格子()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 数字 1
        game.FlagCell(new(1, 1)); // 雷
        game.OpenAdjacentCells(new(0, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Number, game.Board[new(0, 1)].Type);
        Assert.Equal(CellType.Number, game.Board[new(1, 0)].Type);
        Assert.Equal(CellType.Flagged, game.Board[new(1, 1)].Type);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void OpenAdjacentCells_旗数不匹配_不打开任何格子()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 数字 1
        game.FlagCell(new(0, 1));
        game.FlagCell(new(1, 0)); // 旗数 2, 超过数字 1
        game.OpenAdjacentCells(new(0, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Flagged, game.Board[new(0, 1)].Type);
        Assert.Equal(CellType.Flagged, game.Board[new(1, 0)].Type);
        Assert.Equal(CellType.Unopened, game.Board[new(1, 1)].Type);
    }

    [Fact]
    public void OpenAdjacentCells_非数字格_不执行任何操作()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(3, 3)); // 空白格, 泛洪打开下方区域
        game.OpenAdjacentCells(new(3, 3));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Unopened, game.Board[new(0, 0)].Type);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void FlagAdjacentCells_未打开格数匹配_全部插旗()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 数字 1
        game.OpenCell(new(0, 1));
        game.OpenCell(new(1, 0));
        game.FlagAdjacentCells(new(0, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Flagged, game.Board[new(1, 1)].Type);
        Assert.Equal(CellType.Number, game.Board[new(0, 1)].Type);
    }

    [Fact]
    public void FlagAdjacentCells_未打开格数不匹配_不插旗()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 数字 1
        game.FlagAdjacentCells(new(0, 0));

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Unopened, game.Board[new(1, 1)].Type);
        Assert.Equal(CellType.Unopened, game.Board[new(0, 1)].Type);
    }

    [Fact]
    public void OpenCell_踩雷_游戏失败并暴露所有地雷()
    {
        var (game, _, timer) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 开始游戏
        game.OpenCell(new(1, 1)); // 踩雷

        Assert.NotNull(game.Board);
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(CellType.OpenedMine, game.Board[new(1, 1)].Type);
        Assert.Equal(CellType.Mine, game.Board[new(1, 3)].Type);
        Assert.NotNull(game.Result);
        Assert.False(game.Result.IsWin);
        Assert.Equal(GameDifficulty.Custom, game.Result.Difficulty);
        Assert.Equal(Config.BoardHeight, game.Result.BoardHeight);
        Assert.False(game.IsPerformable);

        timer.Verify(t => t.Pause(), Times.Once);
    }

    [Fact]
    public void OpenCell_踩雷_错误的旗子和问号被正确重置()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.FlagCell(new(0, 1));     // 非雷位置的旗子
        game.QuestionCell(new(2, 4)); // 非雷位置的问号
        game.QuestionCell(new(1, 3)); // 雷位置的问号
        game.OpenCell(new(1, 1)); // 踩雷

        Assert.NotNull(game.Board);
        Assert.Equal(CellType.ErrorFlag, game.Board[new(0, 1)].Type);
        Assert.Equal(CellType.Unopened, game.Board[new(2, 4)].Type);
        Assert.Equal(CellType.Mine, game.Board[new(1, 3)].Type);
    }

    [Fact]
    public void OpenCell_打开全部非雷格_游戏胜利并自动插旗()
    {
        var (game, _, timer) = CreatePlayableGame();

        game.OpenCell(new(0, 0)); // 开始游戏

        Assert.NotNull(game.Board);

        foreach (var position in Position.GetAllPositions(Rows, Columns))
        {
            if (game.Status is not GameStatus.InProgress) { break; }
            if (!IsMine(position) && game.Board[position].Type is CellType.Unopened)
            {
                game.OpenCell(position);
            }
        }

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(1.0, game.Completion);
        Assert.Equal(23, game.Board.OpenedCount);
        Assert.Equal(CellType.Flagged, game.Board[new(1, 1)].Type);
        Assert.Equal(CellType.Flagged, game.Board[new(1, 3)].Type);
        Assert.NotNull(game.Result);
        Assert.True(game.Result.IsWin);

        timer.Verify(t => t.Pause(), Times.Once);
    }

    [Fact]
    public void GetSaveData_游戏进行中_返回有效存档数据()
    {
        var (game, mineField, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        var saveData = game.GetSaveData();

        Assert.NotNull(saveData);
        Assert.True(saveData.IsValid());
        Assert.Equal(Seed, saveData.Seed);
        Assert.Equal(GameDifficulty.Custom, saveData.Difficulty);
        Assert.Equal(Config.BoardHeight, saveData.BoardHeight);
        Assert.Equal(Config.BoardWidth, saveData.BoardWidth);
        Assert.Equal(Config.MineCount, saveData.MineCount);
        Assert.Contains(new KeyValuePair<Position, CellType>(new(0, 0), CellType.Number), saveData.CellStates);
        mineField.Verify(m => m.GetMineMap(), Times.Once);
    }

    [Fact]
    public void GetSaveData_尚未开始_返回null()
    {
        var (game, _, _) = CreatePlayableGame();

        Assert.Null(game.GetSaveData());
    }

    [Fact]
    public void GetSaveData_游戏结束后_返回null()
    {
        var (game, _, _) = CreatePlayableGame();

        game.OpenCell(new(0, 0));
        game.OpenCell(new(1, 1)); // 踩雷结束游戏

        Assert.Null(game.GetSaveData());
    }

    [Fact]
    public void 从存档恢复_状态为暂停并恢复棋盘()
    {
        var mineField = CreateMineFieldMock();
        var timer = CreateTimerMock();
        IReadOnlyDictionary<Position, CellType> cellStates = new Dictionary<Position, CellType>
        {
            [new(0, 0)] = CellType.Number
        };
        var saveData = GameSaveData.CreateCustom(
            Seed, StartTime, TimeSpan.FromMinutes(1), CreateMineMap(),
            cellStates, Rows, Columns, Config.MineCount
        );
        var game = new Game(
            new Mock<IServiceScope>().Object,
            NullLogger<Game>.Instance,
            BoardFactory,
            mineField.Object,
            timer.Object,
            saveData
        );

        Assert.Equal(GameStatus.Paused, game.Status);
        Assert.NotNull(game.Board);
        Assert.Equal(CellType.Number, game.Board[new(0, 0)].Type);
        Assert.Equal(CellType.Unopened, game.Board[new(3, 3)].Type);
        Assert.Equal(GameDifficulty.Custom, game.Difficulty);
        Assert.Equal(Config, game.Config);
        Assert.Equal(Seed, game.Seed);
        Assert.Equal(1.0 / 23, game.Completion);
        mineField.Verify(m => m.Generate(Config, saveData.MineField), Times.Once);
        timer.Verify(t => t.Initial(StartTime, TimeSpan.FromMinutes(1)), Times.Once);
    }

    [Fact]
    public void Dispose_释放服务作用域_重复释放无副作用()
    {
        var scope = new Mock<IServiceScope>();
        var game = CreateGame(scope);

        game.Dispose();
        game.Dispose();

        scope.Verify(s => s.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_之后访问属性和方法_抛出ObjectDisposedException()
    {
        var game = CreateGame();
        game.Dispose();

        _ = Assert.Throws<ObjectDisposedException>(() => game.Status);
        _ = Assert.Throws<ObjectDisposedException>(() => game.OpenCell(new(0, 0)));
    }
}
