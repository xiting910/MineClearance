using Microsoft.Extensions.Logging;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.Infrastructure.Services;

/// <summary>
/// 游戏数据仓储实现类, 用于管理游戏存档和游戏结果记录
/// </summary>
internal sealed partial class GameDataRepository : IGameDataRepository
{
    /// <summary>
    /// Json 序列化选项, 包含自定义转换器
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(Constants.JsonSerializerOptions)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new BitArrayConverter(), new PositionConverter() }
    };

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<GameDataRepository> _logger;

    /// <summary>
    /// 游戏结果记录列表
    /// </summary>
    private readonly List<GameResult> _results = [];

    /// <inheritdoc/>
    public GameSaveData? SaveData { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<GameResult> GameResults => _results;

    /// <summary>
    /// 初始化 <see cref="GameDataRepository"/> 类的新实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public GameDataRepository(ILogger<GameDataRepository> logger)
    {
        // 初始化日志记录器
        _logger = logger;

        try
        {
            // 尝试从文件中加载游戏存档数据
            var saveData = JsonSerializer.Deserialize<GameSaveData>(
                File.ReadAllText(Constants.GameSaveDataFilePath), _jsonOptions
            );

            // 如果存档数据有效, 则将其赋值给 SaveData 属性
            if (saveData?.IsValid() == true) { SaveData = saveData; }
        }
        catch (Exception ex)
        {
            // 如果加载数据时发生异常, 则记录错误日志
            LogLoadSaveDataException(ex);
        }

        try
        {
            // 尝试从文件中加载游戏结果记录数据
            var results = JsonSerializer.Deserialize<List<GameResult>>(
                File.ReadAllText(Constants.GameResultsFilePath), _jsonOptions
            );

            // 如果结果不为空, 则将有效的结果按开始时间降序排序后添加到结果列表中
            if (results is not null)
            {
                _results.AddRange(results.Where(r => r.IsValid()).OrderByDescending(r => r.StartTime));
            }
        }
        catch (Exception ex)
        {
            // 如果加载数据时发生异常, 则记录错误日志
            LogLoadGameResultsException(ex);
        }
    }

    /// <inheritdoc/>
    public Task<bool> SaveGameSaveDataAsync(GameSaveData data, CancellationToken ct = default)
    {
        SaveData = data;
        return SaveGameSaveDataToFileAsync(ct);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteGameSaveDataAsync(CancellationToken ct = default)
    {
        SaveData = null;
        return SaveGameSaveDataToFileAsync(ct);
    }

    /// <inheritdoc/>
    public Task<bool> AddGameResultAsync(GameResult result, CancellationToken ct = default)
    {
        _results.Insert(0, result);
        return SaveGameResultsToFileAsync(ct);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteGameResultAsync(GameResult result, CancellationToken ct = default)
    {
        return _results.Remove(result) ? SaveGameResultsToFileAsync(ct) : Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<bool> ClearGameResultsAsync(CancellationToken ct = default)
    {
        _results.Clear();
        return SaveGameResultsToFileAsync(ct);
    }

    /// <summary>
    /// 将游戏存档数据保存到文件
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns><see langword="true"/> 如果保存成功, 否则为 <see langword="false"/></returns>
    private async Task<bool> SaveGameSaveDataToFileAsync(CancellationToken ct)
    {
        try
        {
            await using var stream = File.Create(Constants.GameSaveDataFilePath);
            await JsonSerializer.SerializeAsync(stream, SaveData, _jsonOptions, ct).ConfigureAwait(false);
            LogGameSaveDataSaved();
            return true;
        }
        catch (Exception ex)
        {
            LogSaveGameSaveDataException(ex);
            return false;
        }
    }

    /// <summary>
    /// 将游戏结果保存到文件
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns><see langword="true"/> 如果保存成功, 否则为 <see langword="false"/></returns>
    private async Task<bool> SaveGameResultsToFileAsync(CancellationToken ct)
    {
        try
        {
            await using var stream = File.Create(Constants.GameResultsFilePath);
            await JsonSerializer.SerializeAsync(stream, _results, _jsonOptions, ct).ConfigureAwait(false);
            LogGameResultsSaved();
            return true;
        }
        catch (Exception ex)
        {
            LogSaveGameResultsException(ex);
            return false;
        }
    }
}
