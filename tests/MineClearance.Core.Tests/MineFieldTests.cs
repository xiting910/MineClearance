using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.Core.Services;
using Moq;
using System.Collections;

namespace MineClearance.Core.Tests;

/// <summary>
/// <see cref="MineField"/> 的单元测试, 覆盖地雷布局生成、相邻雷数计算和地雷查询
/// </summary>
public sealed class MineFieldTests
{
    /// <summary>
    /// 测试用的 9x9 棋盘配置
    /// </summary>
    private static readonly GameConfig Config = new(9, 9, 10);

    /// <summary>
    /// 创建返回指定地雷布局的地雷生成器模拟
    /// </summary>
    /// <param name="mines">地雷位置集合</param>
    /// <returns>地雷生成器模拟</returns>
    private static Mock<IMineGenerator> CreateGeneratorMock(IEnumerable<Position> mines)
    {
        var mock = new Mock<IMineGenerator>();
        _ = mock.Setup(
            generator => generator.GenerateMines(It.IsAny<GameConfig>(), It.IsAny<Position>(), It.IsAny<int>())
        ).Returns(mines);
        return mock;
    }

    [Fact]
    public void Generate_已知地雷布局_正确标记地雷和相邻雷数()
    {
        var mineField = new MineField(CreateGeneratorMock([new(0, 0), new(4, 4)]).Object);

        var result = mineField.Generate(Config, new(3, 3), 42);

        Assert.Equal(Constants.MineValue, result[(0 * 9) + 0]); // (0,0) 是地雷
        Assert.Equal(Constants.MineValue, result[(4 * 9) + 4]); // (4,4) 是地雷
        Assert.Equal(1, result[(0 * 9) + 1]);                   // (0,1) 与 (0,0) 相邻
        Assert.Equal(1, result[(1 * 9) + 0]);                   // (1,0) 与 (0,0) 相邻
        Assert.Equal(1, result[(1 * 9) + 1]);                   // (1,1) 与 (0,0) 相邻
        Assert.Equal(1, result[(3 * 9) + 3]);                   // (3,3) 与 (4,4) 相邻
        Assert.Equal(1, result[(5 * 9) + 5]);                   // (5,5) 与 (4,4) 相邻
        Assert.Equal(0, result[(0 * 9) + 8]);                   // (0,8) 与任何地雷都不相邻
    }

    [Fact]
    public void Generate_地雷位于角落_邻居雷数只统计棋盘内()
    {
        var mineField = new MineField(CreateGeneratorMock([new(0, 0)]).Object);

        var result = mineField.Generate(Config, new(3, 3), 42);

        Assert.Equal(Constants.MineValue, result[0]);
        Assert.Equal(1, result[1]);  // (0,1) 在棋盘内, 与 (0,0) 相邻
        Assert.Equal(1, result[9]);  // (1,0) 在棋盘内, 与 (0,0) 相邻
        Assert.Equal(1, result[10]); // (1,1) 在棋盘内, 与 (0,0) 相邻
        Assert.Equal(0, result[2]);  // (0,2) 不与 (0,0) 相邻
        Assert.Equal(0, result[18]); // (2,0) 不与 (0,0) 相邻
    }

    [Fact]
    public void Generate_两雷相邻_相邻雷数正确累计且地雷标记不被覆盖()
    {
        var mineField = new MineField(CreateGeneratorMock([new(3, 3), new(3, 4)]).Object);

        var result = mineField.Generate(Config, new(0, 0), 42);

        Assert.Equal(Constants.MineValue, result[(3 * 9) + 3]);
        Assert.Equal(Constants.MineValue, result[(3 * 9) + 4]);
        Assert.Equal(2, result[(2 * 9) + 4]); // (2,4) 同时与两雷相邻
        Assert.Equal(2, result[(4 * 9) + 4]); // (4,4) 同时与两雷相邻
        Assert.Equal(2, result[(2 * 9) + 3]); // (2,3) 同时与两雷相邻
        Assert.Equal(1, result[(3 * 9) + 5]); // (3,5) 只与 (3,4) 相邻
    }

    [Fact]
    public void Generate_传入位图_正确还原地雷布局()
    {
        var mineMap = new BitArray(81);
        mineMap[0] = true;  // (0,0)
        mineMap[40] = true; // (4,4)
        var mineField = new MineField(new Mock<IMineGenerator>().Object);

        var result = mineField.Generate(Config, mineMap);

        Assert.Equal(Constants.MineValue, result[0]);
        Assert.Equal(Constants.MineValue, result[40]);
        Assert.Equal(1, result[1]);   // (0,1) 与 (0,0) 相邻
        Assert.Equal(1, result[9]);   // (1,0) 与 (0,0) 相邻
        Assert.Equal(1, result[10]);  // (1,1) 与 (0,0) 相邻
        Assert.Equal(1, result[31]);  // (3,4) 与 (4,4) 相邻
        Assert.Equal(1, result[49]);  // (5,4) 与 (4,4) 相邻
        Assert.Equal(0, result[55]);  // (6,1) 与任何地雷都不相邻
    }

    [Fact]
    public void GetMineMap_生成后_返回与地雷布局一致的位置图()
    {
        var mineField = new MineField(CreateGeneratorMock([new(0, 0), new(4, 4)]).Object);
        _ = mineField.Generate(Config, new(3, 3), 42);

        var mineMap = mineField.GetMineMap();

        Assert.Equal(81, mineMap.Length);
        Assert.True(mineMap[0]);
        Assert.True(mineMap[40]);
        Assert.Equal(79, mineMap.Cast<bool>().Count(value => !value));
    }

    [Fact]
    public void IsMine_地雷位置_返回true()
    {
        var mineField = new MineField(CreateGeneratorMock([new(4, 4)]).Object);
        _ = mineField.Generate(Config, new(0, 0), 42);

        Assert.True(mineField.IsMine(new(4, 4)));
        Assert.False(mineField.IsMine(new(0, 0)));
        Assert.False(mineField.IsMine(new(4, 5)));
    }

    [Fact]
    public void Generate_再次调用_重新生成地雷场并覆盖旧数据()
    {
        var mock = new Mock<IMineGenerator>();
        _ = mock.SetupSequence(
            generator => generator.GenerateMines(It.IsAny<GameConfig>(), It.IsAny<Position>(), It.IsAny<int>())
        ).Returns([new(0, 0)]).Returns([new(8, 8)]);
        var mineField = new MineField(mock.Object);

        _ = mineField.Generate(Config, new(3, 3), 42);
        var secondResult = mineField.Generate(Config, new(3, 3), 43);

        Assert.Equal(Constants.MineValue, secondResult[(8 * 9) + 8]);
        Assert.Equal(0, secondResult[0]);
    }

    [Fact]
    public void GetMineMap_尚未生成_抛出InvalidOperationException()
    {
        var mineField = new MineField(new Mock<IMineGenerator>().Object);

        _ = Assert.Throws<InvalidOperationException>(mineField.GetMineMap);
    }

    [Fact]
    public void IsMine_尚未生成_抛出InvalidOperationException()
    {
        var mineField = new MineField(new Mock<IMineGenerator>().Object);

        _ = Assert.Throws<InvalidOperationException>(() => mineField.IsMine(new Position(0, 0)));
    }
}
