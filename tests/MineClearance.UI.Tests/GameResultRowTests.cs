using MineClearance.Core;
using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="GameResultRow"/> 的单元测试, 覆盖显示文本与棋盘尺寸的派生
/// </summary>
public sealed class GameResultRowTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    [Fact]
    public void 构造_内置难度_使用预设棋盘尺寸()
    {
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var row = new GameResultRow(result);

        Assert.Equal(9, row.Config.BoardHeight);
        Assert.Equal(9, row.Config.BoardWidth);
        Assert.Equal(10, row.Config.MineCount);
        Assert.NotEmpty(row.StartTimeText);
    }

    [Fact]
    public void StartTimeText_按指定格式输出()
    {
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));

        Assert.Equal("2026-08-12 18:00:00", new GameResultRow(result).StartTimeText);
    }

    [Fact]
    public void DifficultyText_输出难度描述()
    {
        var result = GameResult.CreateWin(42, GameDifficulty.Expert, StartTime, TimeSpan.FromMinutes(1));

        Assert.Equal(GameDifficulty.Expert.GetDescription(), new GameResultRow(result).DifficultyText);
    }

    [Fact]
    public void ResultText_胜利与失败_输出对应文本()
    {
        var win = GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var loss = GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), 0.5);

        Assert.Equal("胜利", new GameResultRow(win).ResultText);
        Assert.Equal("失败", new GameResultRow(loss).ResultText);
    }

    [Fact]
    public void CompletionForSort_胜利为1_失败为实际完成度()
    {
        var win = GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var loss = GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), 0.345);

        Assert.Equal(1.0, new GameResultRow(win).CompletionForSort);
        Assert.Equal(0.345, new GameResultRow(loss).CompletionForSort);
    }

    [Fact]
    public void CompletionText_胜利为100_失败为实际完成度()
    {
        var win = GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var loss = GameResult.CreateLoss(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), 0.345);

        Assert.Equal("100%", new GameResultRow(win).CompletionText);
        Assert.Equal("34.5%", new GameResultRow(loss).CompletionText);
    }

    [Fact]
    public void DurationText_按MM_SS_xx格式输出()
    {
        var result = GameResult.CreateWin(
            1, GameDifficulty.Beginner, StartTime,
            TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(23)).Add(TimeSpan.FromMilliseconds(450))
        );

        Assert.Equal("01:23.45", new GameResultRow(result).DurationText);
    }

    [Fact]
    public void 构造_自定义难度_使用结果中的实际棋盘尺寸()
    {
        var result = GameResult.CreateCustomWin(42, StartTime, TimeSpan.FromMinutes(1), 5, 7, 8);
        var row = new GameResultRow(result);

        Assert.Equal(5, row.Config.BoardHeight);
        Assert.Equal(7, row.Config.BoardWidth);
        Assert.Equal(8, row.Config.MineCount);
        Assert.Equal("5", row.HeightText);
        Assert.Equal("7", row.WidthText);
        Assert.Equal("8", row.MineCountText);
    }
}
