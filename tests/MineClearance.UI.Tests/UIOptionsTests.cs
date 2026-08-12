using Avalonia.Input;
using Microsoft.Extensions.Configuration;
using MineClearance.UI.Models;
using System.Text.Json.Nodes;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="UIOptions"/> 的单元测试, 覆盖配置读取, 默认值钳制与属性变化自动保存
/// </summary>
public sealed class UIOptionsTests
{
    /// <summary>
    /// 创建设置文件所在目录, 保存到文件依赖目录已存在 (真实应用由其他组件创建)
    /// </summary>
    private static void EnsureSettingsDirectory()
    {
        _ = Directory.CreateDirectory(Path.GetDirectoryName(Constants.UIOptionsSettingsFilePath)!);
    }

    /// <summary>
    /// 创建空配置的 UI 配置
    /// </summary>
    /// <returns>空配置的 UI 配置</returns>
    private static UIOptions CreateEmpty()
    {
        return new(new ConfigurationBuilder().Build());
    }

    /// <summary>
    /// 创建带指定键值的 UI 配置
    /// </summary>
    /// <param name="sectionValues"><see cref="UIOptions"/> 节下的键值对</param>
    /// <returns>带指定键值的 UI 配置</returns>
    private static UIOptions CreateWith(params (string Key, string Value)[] sectionValues)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                sectionValues.ToDictionary(
                    pair => $"{nameof(UIOptions)}:{pair.Key}", pair => (string?)pair.Value
                )
            )
            .Build();
        return new(config);
    }

    [Fact]
    public void 构造_无配置节_使用默认值()
    {
        var options = CreateEmpty();

        Assert.Equal(ThemeMode.System, options.Theme);
        Assert.Equal(Constants.DefaultToastDurationSeconds, options.ToastDurationSeconds);
        Assert.Equal(Constants.DefaultMaxToastCount, options.MaxToastCount);
        Assert.True(options.ShowDownloadBall);
        Assert.True(options.ShowFirstLaunchTip);
        Assert.False(options.CopyIndexOnFirstClick);
        Assert.Equal(Key.None, options.ShowIndexHotKey);
    }

    [Fact]
    public void 构造_配置有效_按配置解析()
    {
        var options = CreateWith(
            (nameof(UIOptions.Theme), ThemeMode.Dark.ToString()),
            (nameof(UIOptions.ToastDurationSeconds), "3"),
            (nameof(UIOptions.MaxToastCount), "4"),
            (nameof(UIOptions.ShowDownloadBall), "false"),
            (nameof(UIOptions.ShowFirstLaunchTip), "false"),
            (nameof(UIOptions.CopyIndexOnFirstClick), "true"),
            (nameof(UIOptions.ShowIndexHotKey), Key.F7.ToString())
        );

        Assert.Equal(ThemeMode.Dark, options.Theme);
        Assert.Equal(3, options.ToastDurationSeconds);
        Assert.Equal(4, options.MaxToastCount);
        Assert.False(options.ShowDownloadBall);
        Assert.False(options.ShowFirstLaunchTip);
        Assert.True(options.CopyIndexOnFirstClick);
        Assert.Equal(Key.F7, options.ShowIndexHotKey);
    }

    [Fact]
    public void 构造_Toast时长超上限_钳制到最大值()
    {
        var options = CreateWith((nameof(UIOptions.ToastDurationSeconds), "99"));

        Assert.Equal(Constants.MaxToastDurationSeconds, options.ToastDurationSeconds);
    }

    [Fact]
    public void 构造_Toast时长低于下限_钳制到最小值()
    {
        var options = CreateWith((nameof(UIOptions.ToastDurationSeconds), "-5"));

        Assert.Equal(Constants.MinToastDurationSeconds, options.ToastDurationSeconds);
    }

    [Fact]
    public void 构造_Toast条数超上限_钳制到最大值()
    {
        var options = CreateWith((nameof(UIOptions.MaxToastCount), "99"));

        Assert.Equal(Constants.MaxMaxToastCount, options.MaxToastCount);
    }

    [Fact]
    public void 构造_主题枚举无效_回退到System()
    {
        var options = CreateWith((nameof(UIOptions.Theme), "NotATheme"));

        Assert.Equal(ThemeMode.System, options.Theme);
    }

    [Fact]
    public void 构造_热键为系统保留键_回退到None()
    {
        var options = CreateWith((nameof(UIOptions.ShowIndexHotKey), Key.Escape.ToString()));

        Assert.Equal(Key.None, options.ShowIndexHotKey);
    }

    [Fact]
    public void 修改属性_保存到设置文件()
    {
        EnsureSettingsDirectory();
        var options = CreateEmpty();
        options.Theme = ThemeMode.Dark;
        options.ToastDurationSeconds = 7;
        options.MaxToastCount = 3;
        options.ShowDownloadBall = false;
        options.CopyIndexOnFirstClick = true;
        options.ShowIndexHotKey = Key.F9;

        Assert.True(File.Exists(Constants.UIOptionsSettingsFilePath));
        var node = JsonNode.Parse(File.ReadAllText(Constants.UIOptionsSettingsFilePath))![nameof(UIOptions)]!;
        Assert.Equal(ThemeMode.Dark.ToString(), node[nameof(options.Theme)]!.GetValue<string>());
        Assert.Equal(7, node[nameof(options.ToastDurationSeconds)]!.GetValue<double>());
        Assert.Equal(3, node[nameof(options.MaxToastCount)]!.GetValue<int>());
        Assert.False(node[nameof(options.ShowDownloadBall)]!.GetValue<bool>());
        Assert.True(node[nameof(options.CopyIndexOnFirstClick)]!.GetValue<bool>());
        Assert.Equal(Key.F9.ToString(), node[nameof(options.ShowIndexHotKey)]!.GetValue<string>());
    }

    [Fact]
    public void 修改属性_写入的文件可被重新加载读回()
    {
        EnsureSettingsDirectory();
        var options = CreateEmpty();
        options.Theme = ThemeMode.Light;
        options.MaxToastCount = 5;

        // 与真实应用一致: 从设置文件加载配置再构造
        var config = new ConfigurationBuilder()
            .AddJsonFile(Constants.UIOptionsSettingsFilePath)
            .Build();
        var loaded = new UIOptions(config);

        Assert.Equal(ThemeMode.Light, loaded.Theme);
        Assert.Equal(5, loaded.MaxToastCount);
    }
}
