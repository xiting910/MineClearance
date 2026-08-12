using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using System.Collections;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="GameSaveData"/> 的单元测试, 覆盖各工厂方法的参数校验和 <see cref="GameSaveData.IsValid"/> 有效性校验
/// </summary>
public sealed class GameSaveDataTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    /// <summary>
    /// 初级难度 (9x9) 对应的地雷分布位图
    /// </summary>
    private static readonly BitArray BeginnerMineField = new(81);

    /// <summary>
    /// 空的格子状态字典
    /// </summary>
    private static readonly IReadOnlyDictionary<Position, CellType> EmptyCellStates = new Dictionary<Position, CellType>();

    [Fact]
    public void Create_内置难度_返回合法的存档数据()
    {
        var data = GameSaveData.Create(
            42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), BeginnerMineField, EmptyCellStates
        );

        Assert.Equal(42, data.Seed);
        Assert.Equal(GameDifficulty.Beginner, data.Difficulty);
        Assert.Equal(StartTime, data.StartTime);
        Assert.Equal(TimeSpan.FromMinutes(1), data.Duration);
        Assert.Same(BeginnerMineField, data.MineField);
        Assert.Null(data.BoardHeight);
        Assert.Null(data.BoardWidth);
        Assert.Null(data.MineCount);
        Assert.True(data.IsValid());
    }

    [Fact]
    public void Create_自定义难度_抛出ArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(
            () => GameSaveData.Create(
                42, GameDifficulty.Custom, StartTime, TimeSpan.Zero, BeginnerMineField, EmptyCellStates
            )
        );
    }

    [Fact]
    public void CreateCustom_有效参数_返回合法的自定义难度存档数据()
    {
        var data = GameSaveData.CreateCustom(
            42, StartTime, TimeSpan.FromMinutes(1), BeginnerMineField, EmptyCellStates, 9, 9, 10
        );

        Assert.Equal(GameDifficulty.Custom, data.Difficulty);
        Assert.Equal(9, data.BoardHeight);
        Assert.Equal(9, data.BoardWidth);
        Assert.Equal(10, data.MineCount);
        Assert.True(data.IsValid());
    }

    [Theory]
    [InlineData(0, 9, 10)]
    [InlineData(-1, 9, 10)]
    [InlineData(31, 9, 10)]
    [InlineData(9, 0, 10)]
    [InlineData(9, 51, 10)]
    [InlineData(9, 9, 0)]
    [InlineData(9, 9, -1)]
    [InlineData(9, 9, 81)]
    public void CreateCustom_棋盘参数无效_抛出ArgumentOutOfRangeException(int height, int width, int mineCount)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => GameSaveData.CreateCustom(42, StartTime, TimeSpan.Zero, new(height * width), EmptyCellStates, height, width, mineCount)
        );
    }

    [Fact]
    public void IsValid_非自定义难度携带棋盘信息_返回false()
    {
        var data = new GameSaveData(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, BeginnerMineField,
            EmptyCellStates, 9, null, null
        );
        Assert.False(data.IsValid());
    }

    [Fact]
    public void IsValid_自定义难度缺少地雷数_返回false()
    {
        var data = new GameSaveData(
            0, GameDifficulty.Custom, StartTime, TimeSpan.Zero, BeginnerMineField, EmptyCellStates, 9, 9, null
        );
        Assert.False(data.IsValid());
    }

    [Fact]
    public void IsValid_自定义难度棋盘参数非法_返回false()
    {
        var data = new GameSaveData(
            0, GameDifficulty.Custom, StartTime, TimeSpan.Zero, BeginnerMineField, EmptyCellStates, 31, 9, 10
        );
        Assert.False(data.IsValid());
    }

    [Fact]
    public void IsValid_地雷分布长度与棋盘不符_返回false()
    {
        var data = new GameSaveData(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, new(80), EmptyCellStates, null, null, null
        );
        Assert.False(data.IsValid());
    }

    [Fact]
    public void IsValid_格子状态键超出棋盘范围_返回false()
    {
        IReadOnlyDictionary<Position, CellType> cellStates = new Dictionary<Position, CellType>
        {
            [new(9, 0)] = CellType.Empty
        };
        var data = new GameSaveData(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero,
            BeginnerMineField, cellStates, null, null, null
        );
        Assert.False(data.IsValid());
    }

    [Fact]
    public void IsValid_自定义难度完整有效_返回true()
    {
        var data = new GameSaveData(
            0, GameDifficulty.Custom, StartTime, TimeSpan.Zero, BeginnerMineField, EmptyCellStates, 9, 9, 10
        );
        Assert.True(data.IsValid());
    }

    [Fact]
    public void IsValid_非自定义难度无棋盘信息且字段合法_返回true()
    {
        var data = new GameSaveData(
            0, GameDifficulty.Intermediate, StartTime, TimeSpan.Zero, new(256),
            EmptyCellStates, null, null, null
        );
        Assert.True(data.IsValid());
    }
}
