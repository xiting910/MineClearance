using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="GameConfig"/> 的单元测试, 覆盖有效性校验、难度映射和格子总数计算
/// </summary>
public sealed class GameConfigTests
{
    [Fact]
    public void TotalCellsToOpen_初级配置_返回总格子数减地雷数()
    {
        Assert.Equal(71, Constants.BeginnerConfig.TotalCellsToOpen);
    }

    [Fact]
    public void TotalCellsToOpen_自定义配置_返回总格子数减地雷数()
    {
        Assert.Equal(75, new GameConfig(10, 10, 25).TotalCellsToOpen);
    }

    [Theory]
    [InlineData(9, 9, 10, true)]
    [InlineData(30, 50, 309, true)]
    [InlineData(9, 9, 80, true)]
    [InlineData(0, 9, 10, false)]
    [InlineData(-1, 9, 10, false)]
    [InlineData(31, 9, 10, false)]
    [InlineData(9, 0, 10, false)]
    [InlineData(9, -1, 10, false)]
    [InlineData(9, 51, 10, false)]
    [InlineData(9, 9, 0, false)]
    [InlineData(9, 9, -1, false)]
    [InlineData(9, 9, 81, false)]
    public void IsValid_给定行列和雷数_判断配置是否有效(int height, int width, int mineCount, bool expected)
    {
        Assert.Equal(expected, GameConfig.IsValid(height, width, mineCount));
    }

    [Fact]
    public void IsValid_实例方法_与静态方法结果一致()
    {
        Assert.True(new GameConfig(9, 9, 10).IsValid());
        Assert.False(new GameConfig(0, 9, 10).IsValid());
    }

    [Theory]
    [InlineData(GameDifficulty.Beginner, 9, 9, 10)]
    [InlineData(GameDifficulty.Intermediate, 16, 16, 40)]
    [InlineData(GameDifficulty.Expert, 16, 30, 99)]
    [InlineData(GameDifficulty.Master, 30, 50, 309)]
    public void FromDifficulty_内置难度_返回对应的内置配置(GameDifficulty difficulty, int height, int width, int mineCount)
    {
        Assert.Equal(new GameConfig(height, width, mineCount), GameConfig.FromDifficulty(difficulty));
    }

    [Fact]
    public void FromDifficulty_自定义难度_抛出ArgumentException()
    {
        _ = Assert.Throws<ArgumentException>(() => GameConfig.FromDifficulty(GameDifficulty.Custom));
    }
}
