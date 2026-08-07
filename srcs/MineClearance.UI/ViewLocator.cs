using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;

namespace MineClearance.UI;

/// <summary>
/// 视图定位器类
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    /// <inheritdoc/>
    public Control? Build(object? data)
    {
        if (data is null) { return null; }

        var viewTypeName = data.GetType().FullName?.Replace(
            Constants.ViewModelSuffix,
            Constants.ViewSuffix,
            StringComparison.Ordinal
        );

        if (viewTypeName is not null)
        {
            var viewType = Type.GetType(viewTypeName);
            if (viewType is not null && typeof(Control).IsAssignableFrom(viewType))
            {
                return (Control?)Activator.CreateInstance(viewType);
            }
        }

        return new TextBlock { Text = $"未找到视图: {data.GetType().FullName}" };
    }

    /// <inheritdoc/>
    public bool Match(object? data)
    {
        if (data is null) { return false; }

        // 匹配 ViewModel 结尾的类型的对象
        return data.GetType().Name.EndsWith(Constants.ViewModelSuffix, StringComparison.Ordinal);
    }
}
