using Microsoft.Win32;

namespace MineClearance.Services;

/// <summary>
/// 自动启动帮助类
/// </summary>
internal static class AutoStartHelper
{
    /// <summary>
    /// RunKey 注册表路径
    /// </summary>
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// 要设置的启动项名字
    /// </summary>
    private const string AutoStartName = "MineClearance";

    /// <summary>
    /// 打开注册表 RunKey 项
    /// </summary>
    /// <returns>RunKey 的 RegistryKey 对象</returns>
    /// <exception cref="InvalidOperationException">无法打开注册表 RunKey 项时抛出</exception>
    private static RegistryKey OpenRunKey() => Registry.CurrentUser.OpenSubKey(RunKey, true) ?? throw new InvalidOperationException("无法打开注册表 RunKey 项");

    /// <summary>
    /// 确保注册表的某一个键下指定名字的字符串值是正确的(如果不存在则不做任何操作)
    /// </summary>
    /// <param name="key">要检查的注册表键</param>
    /// <param name="name">要检查的值名称</param>
    /// <param name="value">期望的值</param>
    private static void EnsureCorrectStringValue(RegistryKey key, string name, string value)
    {
        var currentValue = key.GetValue(name);
        if (currentValue == null)
        {
            return;
        }

        if (currentValue is not string strValue || strValue != value)
        {
            key.SetValue(name, value);
            FileLogger.LogInfo($"更新注册表项 {key.Name}\\{name} 的值为 {value} (原值为 {currentValue})");
        }
    }

    /// <summary>
    /// 确保注册表中不存在无效启动项
    /// </summary>
    public static void EnsureValidRegistryEntries()
    {
        // 打开注册表 RunKey 项
        using var runKey = OpenRunKey();

        // 确保 MineClearance 项的值是正确的
        EnsureCorrectStringValue(runKey, AutoStartName, $"\"{Application.ExecutablePath}\"");

        // 遍历所有值, 检查文件是否存在
        foreach (var name in runKey.GetValueNames())
        {
            // 获取值
            var value = runKey.GetValue(name) as string;

            // 解析值
            if (!string.IsNullOrEmpty(value))
            {
                // 原始值
                var originalValue = value;

                // 如果值以 " 开头, 提取 "" 中的路径
                if (value.StartsWith('"'))
                {
                    var endQuoteIndex = value.IndexOf('"', 1);
                    if (endQuoteIndex > 1)
                    {
                        value = value[1..endQuoteIndex];
                    }
                }

                // 检查文件是否存在
                if (!File.Exists(value))
                {
                    runKey.DeleteValue(name, false);
                    FileLogger.LogWarning($"删除判断为无效的注册表项 {runKey.Name}\\{name}, 原值为 {originalValue}");
                }
            }
        }
    }

    /// <summary>
    /// 检查是否启用自动启动, 并确保路径正确
    /// </summary>
    /// <returns>是否启用自动启动</returns>
    public static bool IsAutoStartEnabled()
    {
        using var runKey = OpenRunKey();
        if (runKey.GetValue(AutoStartName) == null)
        {
            return false;
        }

        EnsureCorrectStringValue(runKey, AutoStartName, $"\"{Application.ExecutablePath}\"");
        return true;
    }

    /// <summary>
    /// 启用自动启动
    /// </summary>
    public static void EnableAutoStart()
    {
        using var key = OpenRunKey();
        key.SetValue(AutoStartName, $"\"{Application.ExecutablePath}\"");
        FileLogger.LogInfo($"添加注册表项 {key.Name}\\{AutoStartName}, 值为 \"{Application.ExecutablePath}\"");
    }

    /// <summary>
    /// 禁用自动启动
    /// </summary>
    public static void DisableAutoStart()
    {
        using var key = OpenRunKey();
        key.DeleteValue(AutoStartName, false);
        FileLogger.LogInfo($"删除注册表项 {key.Name}\\{AutoStartName}");
    }
}