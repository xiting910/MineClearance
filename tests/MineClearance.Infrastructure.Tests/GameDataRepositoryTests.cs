using Microsoft.Extensions.Logging.Abstractions;
using MineClearance.Core.Enums;
using MineClearance.Core.Models.Records;
using MineClearance.Infrastructure.Services;
using System.Collections;
using System.Text.Json;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// <see cref="GameDataRepository"/> 的单元测试, 覆盖存档与游戏结果记录的加载、保存、删除和清空
/// </summary>
public sealed class GameDataRepositoryTests
{
    /// <summary>
    /// 测试用的固定开始时间
    /// </summary>
    private static readonly DateTime StartTime = new(2026, 8, 12, 18, 0, 0);

    /// <summary>
    /// 每个测试开始前重置数据文件, 避免测试间互相干扰
    /// </summary>
    public GameDataRepositoryTests()
    {
        ResetPath(Constants.GameSaveDataFilePath);
        ResetPath(Constants.GameResultsFilePath);
        _ = Directory.CreateDirectory(Path.GetDirectoryName(Constants.GameSaveDataFilePath)!);
    }

    [Fact]
    public void 构造_数据文件不存在_存档为空且结果列表为空()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.Null(repo.SaveData);
        Assert.Empty(repo.GameResults);
    }

    [Fact]
    public void 构造_存档文件有效_加载存档数据()
    {
        File.WriteAllText(Constants.GameSaveDataFilePath, $$"""
        {
            "Seed": 42,
            "Difficulty": 0,
            "StartTime": "2026-08-12T18:00:00",
            "Duration": "00:01:00",
            "MineField": { "Length": 81, "Bytes": "{{Convert.ToBase64String(new byte[11])}}" },
            "CellStates": { "1,2": 2 },
            "BoardHeight": null,
            "BoardWidth": null,
            "MineCount": null
        }
        """);

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        var saveData = Assert.IsType<GameSaveData>(repo.SaveData);
        Assert.Equal(42, saveData.Seed);
        Assert.Equal(GameDifficulty.Beginner, saveData.Difficulty);
        Assert.Equal(StartTime, saveData.StartTime);
        Assert.Equal(TimeSpan.FromMinutes(1), saveData.Duration);
        Assert.Equal(81, saveData.MineField.Length);
        Assert.Equal(CellType.Number, saveData.CellStates[new(1, 2)]);
        Assert.True(saveData.IsValid());
    }

    [Fact]
    public void 构造_存档数据无效_忽略存档数据()
    {
        File.WriteAllText(Constants.GameSaveDataFilePath, $$"""
        {
            "Seed": 42,
            "Difficulty": 0,
            "StartTime": "2026-08-12T18:00:00",
            "Duration": "00:01:00",
            "MineField": { "Length": 80, "Bytes": "{{Convert.ToBase64String(new byte[10])}}" },
            "CellStates": { },
            "BoardHeight": null,
            "BoardWidth": null,
            "MineCount": null
        }
        """);

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.Null(repo.SaveData);
    }

    [Fact]
    public void 构造_存档文件损坏_不抛出异常且存档为空()
    {
        File.WriteAllText(Constants.GameSaveDataFilePath, "not-json");

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.Null(repo.SaveData);
    }

    [Fact]
    public void 构造_结果文件记录乱序_按开始时间降序排列()
    {
        var early = GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var late = GameResult.CreateWin(
            2, GameDifficulty.Beginner, StartTime.AddMinutes(5), TimeSpan.FromMinutes(1)
        );
        File.WriteAllText(
            Constants.GameResultsFilePath,
            JsonSerializer.Serialize(new List<GameResult> { early, late })
        );

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.Equal(2, repo.GameResults.Count);
        Assert.Equal(late, repo.GameResults[0]);
        Assert.Equal(early, repo.GameResults[1]);
    }

    [Fact]
    public void 构造_结果文件含无效记录_过滤无效记录()
    {
        var valid = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        var invalid = new GameResult(
            0, GameDifficulty.Beginner, StartTime, TimeSpan.Zero, true, 0.5, null, null, null
        );
        File.WriteAllText(
            Constants.GameResultsFilePath,
            JsonSerializer.Serialize(new List<GameResult> { invalid, valid })
        );

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.Equal(valid, Assert.Single(repo.GameResults));
    }

    [Fact]
    public void 构造_结果文件损坏_不抛出异常且结果列表为空()
    {
        File.WriteAllText(Constants.GameResultsFilePath, "not-json");

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.Empty(repo.GameResults);
    }

    [Fact]
    public async Task SaveGameSaveDataAsync_有效存档_保存文件并可完整读回()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);
        var data = CreateValidSaveData();

        Assert.True(await repo.SaveGameSaveDataAsync(data, TestContext.Current.CancellationToken));
        Assert.Same(data, repo.SaveData);
        Assert.True(File.Exists(Constants.GameSaveDataFilePath));

        var loaded = Assert.IsType<GameSaveData>(
            new GameDataRepository(NullLogger<GameDataRepository>.Instance).SaveData
        );
        Assert.Equal(data.Seed, loaded.Seed);
        Assert.Equal(data.Difficulty, loaded.Difficulty);
        Assert.Equal(data.StartTime, loaded.StartTime);
        Assert.Equal(data.Duration, loaded.Duration);
        Assert.Equal(data.BoardHeight, loaded.BoardHeight);
        Assert.Equal(data.BoardWidth, loaded.BoardWidth);
        Assert.Equal(data.MineCount, loaded.MineCount);
        AssertBitArrayEqual(data.MineField, loaded.MineField);
        Assert.Equal(data.CellStates.Count, loaded.CellStates.Count);
        foreach (var (position, cellType) in data.CellStates)
        {
            Assert.Equal(cellType, loaded.CellStates[position]);
        }
        Assert.True(loaded.IsValid());
    }

    [Fact]
    public async Task SaveGameSaveDataAsync_令牌已取消_返回false()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.False(await repo.SaveGameSaveDataAsync(CreateValidSaveData(), new(true)));
    }

    [Fact]
    public async Task SaveGameSaveDataAsync_目标路径被目录占用_返回false()
    {
        _ = Directory.CreateDirectory(Constants.GameSaveDataFilePath);

        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);

        Assert.False(await repo.SaveGameSaveDataAsync(
            CreateValidSaveData(), TestContext.Current.CancellationToken
        ));
    }

    [Fact]
    public async Task DeleteGameSaveDataAsync_删除存档_清空内存数据并写入空存档()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);
        _ = await repo.SaveGameSaveDataAsync(CreateValidSaveData(), TestContext.Current.CancellationToken);

        Assert.True(await repo.DeleteGameSaveDataAsync(TestContext.Current.CancellationToken));
        Assert.Null(repo.SaveData);
        Assert.Equal("null", File.ReadAllText(Constants.GameSaveDataFilePath));
    }

    [Fact]
    public async Task AddGameResultAsync_添加结果_插入列表头部并保存文件()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));

        Assert.True(await repo.AddGameResultAsync(result, TestContext.Current.CancellationToken));
        Assert.Same(result, repo.GameResults[0]);
        Assert.Equal(
            result,
            Assert.Single(new GameDataRepository(NullLogger<GameDataRepository>.Instance).GameResults)
        );
    }

    [Fact]
    public async Task DeleteGameResultAsync_结果存在_删除并返回true()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));
        _ = await repo.AddGameResultAsync(result, TestContext.Current.CancellationToken);

        Assert.True(await repo.DeleteGameResultAsync(result, TestContext.Current.CancellationToken));
        Assert.Empty(repo.GameResults);
        Assert.Equal("[]", File.ReadAllText(Constants.GameResultsFilePath));
    }

    [Fact]
    public async Task DeleteGameResultAsync_结果不存在_返回false()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);
        var result = GameResult.CreateWin(42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1));

        Assert.False(await repo.DeleteGameResultAsync(result, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ClearGameResultsAsync_清空结果_结果列表为空并保存文件()
    {
        var repo = new GameDataRepository(NullLogger<GameDataRepository>.Instance);
        _ = await repo.AddGameResultAsync(
            GameResult.CreateWin(1, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1)),
            TestContext.Current.CancellationToken
        );
        _ = await repo.AddGameResultAsync(
            GameResult.CreateWin(2, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(2)),
            TestContext.Current.CancellationToken
        );

        Assert.True(await repo.ClearGameResultsAsync(TestContext.Current.CancellationToken));
        Assert.Empty(repo.GameResults);
        Assert.Equal("[]", File.ReadAllText(Constants.GameResultsFilePath));
    }

    /// <summary>
    /// 创建合法的初级难度存档数据, 包含地雷位图与格子状态
    /// </summary>
    private static GameSaveData CreateValidSaveData()
    {
        var mineField = new BitArray(81);
        mineField.Set(0, true);
        mineField.Set(40, true);
        mineField.Set(80, true);
        IReadOnlyDictionary<Position, CellType> cellStates = new Dictionary<Position, CellType>
        {
            [new(0, 1)] = CellType.Number,
            [new(8, 8)] = CellType.Flagged
        };
        return GameSaveData.Create(
            42, GameDifficulty.Beginner, StartTime, TimeSpan.FromMinutes(1), mineField, cellStates
        );
    }

    /// <summary>
    /// 断言两个 <see cref="BitArray"/> 实例的内容相等
    /// </summary>
    private static void AssertBitArrayEqual(BitArray expected, BitArray actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }

    /// <summary>
    /// 重置指定路径, 兼容文件与目录两种占用形式
    /// </summary>
    private static void ResetPath(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
