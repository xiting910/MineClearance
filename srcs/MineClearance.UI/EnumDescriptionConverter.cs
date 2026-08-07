using Avalonia.Data.Converters;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MineClearance.UI;

/// <summary>
/// 枚举描述文本转换器, 将枚举值转换为 <see cref="DescriptionAttribute"/> 描述文本
/// </summary>
public sealed class EnumDescriptionConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) { return value; }

        var name = enumValue.ToString();
        var field = enumValue.GetType().GetField(name);
        return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
