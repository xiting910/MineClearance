using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using MineClearance.Utilities;
using MineClearance.Models.Configs;

namespace MineClearance.Services;

/// <summary>
/// 窗口配置管理类
/// </summary>
internal static class FormConfigManager
{
    /// <summary>
    /// 配置信息字段
    /// </summary>
    private static FormConfig _config = LoadConfig();

    /// <summary>
    /// 配置信息, 设置后会自动保存, 设置为 null 时会重置为默认值
    /// </summary>
    [AllowNull]
    public static FormConfig Config
    {
        get => _config;
        set => SaveConfig(value ?? FormConfig.Invalid);
    }

    /// <summary>
    /// 加载配置数据
    /// </summary>
    /// <returns>返回加载的配置数据</returns>
    private static FormConfig LoadConfig()
    {
        try
        {
            // 检查数据存储路径是否存在
            if (!Directory.Exists(Constants.DataPath))
            {
                // 如果不存在, 创建数据存储路径
                _ = Directory.CreateDirectory(Constants.DataPath);
            }

            // 如果配置文件不存在, 则返回无效配置
            if (!File.Exists(Constants.FormConfigFilePath))
            {
                return FormConfig.Invalid;
            }

            // 读取配置文件内容
            var json = File.ReadAllText(Constants.FormConfigFilePath);

            // 如果内容为空, 则返回无效配置
            if (string.IsNullOrWhiteSpace(json))
            {
                return FormConfig.Invalid;
            }

            // 尝试反序列化为 Config 对象
            var config = JsonSerializer.Deserialize<FormConfig>(json, Constants.JsonOptions);
            return config ?? FormConfig.Invalid;
        }
        catch (Exception ex)
        {
            // 记录并显示错误信息
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"加载窗口配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return FormConfig.Invalid;
        }
    }

    /// <summary>
    /// 保存配置数据
    /// </summary>
    /// <param name="config">要保存的配置数据</param>
    private static void SaveConfig(FormConfig config)
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
            var json = JsonSerializer.Serialize(config, Constants.JsonOptions);

            // 将 JSON 字符串写入配置文件
            File.WriteAllText(Constants.FormConfigFilePath, json);

            // 更新当前配置
            _config = config;
        }
        catch (Exception ex)
        {
            // 记录并显示错误信息
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"保存窗口配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}