using System.Text.Json;
using MineClearance.Models;
using MineClearance.Utilities;

namespace MineClearance.Services;

/// <summary>
/// 数据类, 记录和控制所有历史游戏数据
/// </summary>
internal static class Datas
{
    /// <summary>
    /// 当前数据版本号
    /// </summary>
    private const int CurrentDataVersion = 4;

    /// <summary>
    /// 所有游戏结果列表
    /// </summary>
    private static readonly List<GameResult> _gameResults = [];

    /// <summary>
    /// 所有游戏结果的只读列表
    /// </summary>
    public static IReadOnlyList<GameResult> GameResults => _gameResults.AsReadOnly();

    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    public static async Task Initialize()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);

                // 创建一个空的历史记录文件
                await using (File.Create(Constants.HistoryFilePath)) { }
                return;
            }

            // 如果历史记录文件不存在, 则创建一个空的文件
            if (!File.Exists(Constants.HistoryFilePath))
            {
                await using (File.Create(Constants.HistoryFilePath)) { }
                return;
            }

            // 获取临时文件名, 避免与现有文件冲突
            var tempPath = Path.Combine(Constants.DataPath, Guid.NewGuid().ToString() + ".tmp");
            while (File.Exists(tempPath))
            {
                tempPath = Path.Combine(Constants.DataPath, Guid.NewGuid().ToString() + ".tmp");
            }

            // 先重命名为临时文件名, 再重命名为目标大小写
            File.Move(Constants.HistoryFilePath, tempPath);
            File.Move(tempPath, Constants.HistoryFilePath);

            // 读取历史记录文件内容
            await using var stream = File.OpenRead(Constants.HistoryFilePath);
            if (stream.Length > 0)
            {
                try
                {
                    // 尝试异步反序列化为 GameData 对象
                    var gameData = await JsonSerializer.DeserializeAsync<GameData>(stream, Constants.JsonOptions);
                    if (gameData != null)
                    {
                        // 更新游戏结果列表
                        _gameResults.AddRange(gameData.GameResults);

                        // 记录数据加载信息
                        FileLogger.LogInfo($"成功加载游戏数据: {gameData.GameResults.Count} 条记录");

                        // 如果数据版本低于当前版本, 则进行数据升级
                        if (gameData.Version < CurrentDataVersion)
                        {
                            // 将游戏结果按时间倒序排列
                            _gameResults.Sort(new GameResultComparer(nameof(GameResult.StartTime), SortOrder.Descending));

                            // 保存更新后的数据
                            await SaveGameResultsAsync();

                            // 记录数据升级信息
                            FileLogger.LogInfo($"游戏数据已从版本 {gameData.Version} 升级到 {CurrentDataVersion}");
                        }
                    }
                }
                catch (JsonException)
                {
                    // 如果反序列化为 GameData 对象失败, 可能是旧版本数据格式
                    stream.Position = 0;
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();

                    // 将文件出现的所有"Difficulty": 3替换为"Difficulty": 4
                    var updatedJson = json.Replace("\"Difficulty\": 3", "\"Difficulty\": 4");

                    // 尝试反序列化为 List<GameResult> 对象
                    var oldGameResults = JsonSerializer.Deserialize<List<GameResult>>(updatedJson, Constants.JsonOptions);

                    // 如果存在旧数据
                    if (oldGameResults != null)
                    {
                        // 将旧数据添加到游戏结果列表
                        _gameResults.AddRange(oldGameResults);

                        // 按时间倒序排列
                        _gameResults.Sort(new GameResultComparer(nameof(GameResult.StartTime), SortOrder.Descending));

                        // 记录数据加载信息
                        FileLogger.LogInfo($"成功加载游戏数据: {oldGameResults.Count} 条记录");
                    }

                    // 保存更新后的数据
                    await SaveGameResultsAsync();

                    // 记录数据升级信息
                    FileLogger.LogInfo($"游戏数据已从旧版本升级到新版本");
                }
            }
        }
        catch (Exception ex)
        {
            // 记录并显示错误信息
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"初始化游戏数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 添加游戏结果
    /// </summary>
    /// <param name="result">游戏结果</param>
    public static async Task AddGameResultAsync(GameResult result)
    {
        // 插入到列表开头
        _gameResults.Insert(0, result);
        await SaveGameResultsAsync();
        FileLogger.LogInfo($"添加游戏结果: {result}");
    }

    /// <summary>
    /// 删除指定的游戏结果
    /// </summary>
    /// <param name="result">要删除的游戏结果</param>
    public static async Task RemoveGameResultAsync(GameResult result)
    {
        if (_gameResults.Remove(result))
        {
            await SaveGameResultsAsync();
            FileLogger.LogInfo($"删除游戏结果: {result}");
        }
    }

    /// <summary>
    /// 清空游戏结果
    /// </summary>
    public static async Task ClearGameResultsAsync()
    {
        _gameResults.Clear();
        await SaveGameResultsAsync();
        FileLogger.LogWarning($"清空游戏结果");
    }

    /// <summary>
    /// 保存游戏结果到数据文件
    /// </summary>
    private static async Task SaveGameResultsAsync()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);
            }

            // 要保存的数据
            var data = new GameData
            {
                Version = CurrentDataVersion,
                LastUpdate = DateTime.Now,
                GameResults = _gameResults
            };

            // 异步序列化 data 到游戏历史记录文件
            await using var stream = File.Create(Constants.HistoryFilePath);
            await JsonSerializer.SerializeAsync(stream, data, Constants.JsonOptions);
        }
        catch (Exception ex)
        {
            // 记录并显示错误信息
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"保存游戏结果失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}