using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MineClearance.UI;

/// <summary>
/// 应用元数据类
/// </summary>
public static class AppMetadata
{
    /// <summary>
    /// 应用元数据
    /// </summary>
    private static readonly IEnumerable<AssemblyMetadataAttribute> _metadata = Assembly.GetExecutingAssembly()
        .GetCustomAttributes<AssemblyMetadataAttribute>();

    /// <summary>
    /// 获取应用元数据
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>值</returns>
    /// <exception cref="InvalidOperationException">当指定键不存在或对应值为 null 时抛出</exception>
    public static string Get(string key)
    {
        return _metadata.First(x => x.Key == key).Value
            ?? throw new InvalidOperationException($"Metadata value for key '{key}' is null.");
    }
}
