using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;

namespace MineClearance.Core.Tests;

/// <summary>
/// GameResult 的单元测试, 覆盖各工厂方法的参数校验和 IsValid 有效性校验
/// </summary>
public sealed class GameResultTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    [Fact]
    public void CreateWin_内置难度_返回合法的获胜结果()
    {
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));

        Assert.True(result.IsWin);
        Assert.Equal(42, result.Seed);
        Assert.Equal(GameDifficulty.Beginner, result.Difficulty);
        Assert.Equal(StartTime, result.StartTime);
        Assert.Equal(TimeSpan.FromMinutes(1), result.Duration);
        Assert.Null(result.Completion);
        Assert.Null(result.BoardHeight);
        Assert.Null(result.BoardWidth);
        Assert.Null(result.MineCount);
        Assert.True(result.IsValid());
    }

    [Fact]
    public void CreateWin_自定义难度_抛出ArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(
            () => GameResult.CreateWin(42, GameDifficulty.Custom, StartTime, TimeSpan.Zero)
        );
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void CreateLoss_完成度在合法范围内_返回合法的失败结果(double completion)
    {
        var result = GameResult.CreateLoss(
            42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), completion
        );

        Assert.False(result.IsWin);
        Assert.Equal(completion, result.Completion);
        Assert.True(result.IsValid());
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void CreateLoss_完成度超出范围_抛出ArgumentOutOfRangeException(double completion)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => GameResult.CreateLoss(42, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, completion)
        );
    }

    [Fact]
    public void CreateLoss_自定义难度_抛出ArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(
            () => GameResult.CreateLoss(42, GameDifficulty.Custom, StartTime, TimeSpan.Zero, 0.5)
        );
    }

    [Fact]
    public void CreateCustomWin_有效参数_返回合法的自定义难度获胜结果()
    {
        var result = GameResult.CreateCustomWin(42, StartTime, TimeSpan.FromMinutes(1), 9, 9, 10);

        Assert.True(result.IsWin);
        Assert.Equal(GameDifficulty.Custom, result.Difficulty);
        Assert.Equal(9, result.BoardHeight);
        Assert.Equal(9, result.BoardWidth);
        Assert.Equal(10, result.MineCount);
        Assert.Null(result.Completion);
        Assert.True(result.IsValid());
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
    public void CreateCustomWin_棋盘参数无效_抛出ArgumentOutOfRangeException(int height, int width, int mineCount)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => GameResult.CreateCustomWin(42, StartTime, TimeSpan.Zero, height, width, mineCount)
        );
    }

    [Fact]
    public void CreateCustomLoss_有效参数_返回合法的自定义难度失败结果()
    {
        var result = GameResult.CreateCustomLoss(42, StartTime, TimeSpan.FromMinutes(1), 0.5, 9, 9, 10);

        Assert.False(result.IsWin);
        Assert.Equal(GameDifficulty.Custom, result.Difficulty);
        Assert.Equal(0.5, result.Completion);
        Assert.Equal(9, result.BoardHeight);
        Assert.Equal(9, result.BoardWidth);
        Assert.Equal(10, result.MineCount);
        Assert.True(result.IsValid());
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void CreateCustomLoss_完成度超出范围_抛出ArgumentOutOfRangeException(double completion)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => GameResult.CreateCustomLoss(42, StartTime, TimeSpan.Zero, completion, 9, 9, 10)
        );
    }

    [Theory]
    [InlineData(0, 9, 10)]
    [InlineData(9, 9, 81)]
    public void CreateCustomLoss_棋盘参数无效_抛出ArgumentOutOfRangeException(int height, int width, int mineCount)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(
            () => GameResult.CreateCustomLoss(42, StartTime, TimeSpan.Zero, 0.5, height, width, mineCount)
        );
    }

    [Fact]
    public void IsValid_自定义难度缺少棋盘信息_返回false()
    {
        var result = new GameResult(
            0, GameDifficulty.Custom, StartTime, TimeSpan.Zero, true, null, null, null, null
        );
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValid_非自定义难度携带棋盘信息_返回false()
    {
        var result = new GameResult(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, true, null, 9, 9, 10
        );
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValid_获胜但携带完成度_返回false()
    {
        var result = new GameResult(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, true, 0.5, null, null, null
        );
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValid_失败但未携带完成度_返回false()
    {
        var result = new GameResult(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, false, null, null, null, null
        );
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValid_失败且完成度超出范围_返回false()
    {
        var result = new GameResult(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, false, 1.5, null, null, null
        );
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValid_失败且完成度为负_返回false()
    {
        var result = new GameResult(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, false, -0.1, null, null, null
        );
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValid_自定义难度获胜且棋盘信息合法_返回true()
    {
        var result = new GameResult(0, GameDifficulty.Custom, StartTime, TimeSpan.Zero, true, null, 9, 9, 10);
        Assert.True(result.IsValid());
    }

    [Fact]
    public void IsValid_非自定义难度失败且完成度合法_返回true()
    {
        var result = new GameResult(
            0, GameDifficulty.Intermediate, StartTime, TimeSpan.Zero, false, 0.25, null, null, null
        );
        Assert.True(result.IsValid());
    }
}
