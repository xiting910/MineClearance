using System.Text.Json;
using MineClearance.Utilities;
using MineClearance.Models.Configs;

namespace MineClearance.Services;

/// <summary>
/// 日志配置管理类
/// </summary>
internal static class LogConfigManager
{
    /// <summary>
    /// 当前日志配置
    /// </summary>
    public static LogConfig Config { get; private set; }

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static LogConfigManager()
    {
        Config = LoadConfig(out var needSave);
        Config.Changed += SaveConfig;
        if (needSave)
        {
            SaveConfig();
        }
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    public static void ResetToDefault()
    {
        Config.Changed -= SaveConfig;
        Config = new();
        Config.Changed += SaveConfig;
        SaveConfig();
    }

    /// <summary>
    /// 加载配置数据
    /// </summary>
    /// <param name="needSave">是否需要保存配置</param>
    /// <returns>返回加载的配置数据</returns>
    private static LogConfig LoadConfig(out bool needSave)
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);
            }

            // 如果配置文件不存在, 则返回默认配置
            if (!File.Exists(Constants.LogConfigFilePath))
            {
                needSave = true;
                return new();
            }

            // 读取配置文件内容
            var json = File.ReadAllText(Constants.LogConfigFilePath);

            // 如果内容为空, 则返回默认配置
            if (string.IsNullOrWhiteSpace(json))
            {
                needSave = true;
                return new();
            }

            // 尝试反序列化为 Config 对象
            var config = JsonSerializer.Deserialize<LogConfig>(json, Constants.JsonOptions);

            // 如果反序列化失败, 则返回默认配置
            if (config == null)
            {
                needSave = true;
                return new();
            }

            // 如果反序列化成功, 则不需要保存配置
            needSave = false;
            return config;
        }
        catch (Exception ex)
        {
            // 记录并显示错误信息
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"加载日志配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            needSave = true;
            return new();
        }
    }

    /// <summary>
    /// 保存当前配置数据
    /// </summary>
    private static void SaveConfig()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);
            }

            // 将配置对象序列化为 JSON 字符串
            var json = JsonSerializer.Serialize(Config, Constants.JsonOptions);

            // 将 JSON 字符串写入配置文件
            File.WriteAllText(Constants.LogConfigFilePath, json);
        }
        catch (Exception ex)
        {
            // 记录并显示错误信息
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"保存日志配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}