using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="StatsRowBuilder"/> 的单元测试, 覆盖游戏结果的统计累积与统计行生成
/// </summary>
public sealed class StatsRowBuilderTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    [Fact]
    public void ToRow_无任何结果_显示空统计文本()
    {
        var row = new StatsRowBuilder().ToRow(null);

        Assert.Null(row.Difficulty);
        Assert.Equal("全部", row.DifficultyText);
        Assert.Equal(0, row.Games);
        Assert.Equal(0, row.Wins);
        Assert.Equal("--", row.WinRateText);
        Assert.Equal(-1, row.WinRate);
        Assert.Equal("--", row.AvgWinDurationText);
        Assert.Null(row.AvgWinDuration);
        Assert.Equal("--", row.MinWinDurationText);
        Assert.Null(row.MinWinDuration);
        Assert.Equal("--", row.AvgCompletionText);
        Assert.Equal(-1, row.AvgCompletion);
    }

    [Fact]
    public void ToRow_全部胜利_胜率100且平均最短用时相同()
    {
        StatsRowBuilder builder = new();
        builder.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        builder.Add(GameResult.CreateWin(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2)));

        var row = builder.ToRow(GameDifficulty.Beginner);

        Assert.Equal(GameDifficulty.Beginner, row.Difficulty);
        Assert.Equal(2, row.Games);
        Assert.Equal(2, row.Wins);
        Assert.Equal("100%", row.WinRateText);
        Assert.Equal(100, row.WinRate);
        Assert.Equal("01:30", row.AvgWinDurationText);
        Assert.Equal(TimeSpan.FromMinutes(1.5), row.AvgWinDuration);
        Assert.Equal("01:00", row.MinWinDurationText);
        Assert.Equal(TimeSpan.FromMinutes(1), row.MinWinDuration);
        Assert.Equal("100%", row.AvgCompletionText);
        Assert.Equal(1.0, row.AvgCompletion);
    }

    [Fact]
    public void ToRow_胜负混合_统计胜负与用时()
    {
        StatsRowBuilder builder = new();
        builder.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        builder.Add(GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2), 0.5));

        var row = builder.ToRow(GameDifficulty.Beginner);

        Assert.Equal(GameDifficulty.Beginner, row.Difficulty);
        Assert.Equal(2, row.Games);
        Assert.Equal(1, row.Wins);
        Assert.Equal("50%", row.WinRateText);
        Assert.Equal(50, row.WinRate);
        Assert.Equal("01:00", row.AvgWinDurationText);
        Assert.Equal("75%", row.AvgCompletionText);
        Assert.Equal(0.75, row.AvgCompletion);
    }

    [Fact]
    public void ToRow_全部失败_无胜利用时但平均完成度有效()
    {
        StatsRowBuilder builder = new();
        builder.Add(
            GameResult.CreateLoss(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), 0.25)
        );
        builder.Add(
            GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2), 0.75)
        );

        var row = builder.ToRow(GameDifficulty.Beginner);

        Assert.Equal(GameDifficulty.Beginner, row.Difficulty);
        Assert.Equal(2, row.Games);
        Assert.Equal(0, row.Wins);
        Assert.Equal("0%", row.WinRateText);
        Assert.Equal("--", row.AvgWinDurationText);
        Assert.Null(row.AvgWinDuration);
        Assert.Equal("--", row.MinWinDurationText);
        Assert.Null(row.MinWinDuration);
        Assert.Equal("50%", row.AvgCompletionText);
        Assert.Equal(0.5, row.AvgCompletion);
    }

    [Fact]
    public void ToRow_胜率非整除_按两位小数格式化()
    {
        StatsRowBuilder builder = new();
        builder.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)));
        builder.Add(GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2), 0.5));

        // 1/3 = 33.33%
        builder.Add(GameResult.CreateLoss(3, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(3), 0.5));

        Assert.Equal("33.33%", builder.ToRow(GameDifficulty.Beginner).WinRateText);
    }

    [Fact]
    public void ToRow_用时跨分钟_按MM_SS格式化()
    {
        StatsRowBuilder builder = new();
        builder.Add(GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(12)));
        builder.Add(GameResult.CreateWin(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(3)));

        Assert.Equal("07:30", builder.ToRow(GameDifficulty.Beginner).AvgWinDurationText);
        Assert.Equal("03:00", builder.ToRow(GameDifficulty.Beginner).MinWinDurationText);
    }
}
