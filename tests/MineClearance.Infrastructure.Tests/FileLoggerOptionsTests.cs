using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using Moq;
using System.Text.Json.Nodes;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// <see cref="FileLoggerOptions"/> 的单元测试, 覆盖日志级别配置解析与级别设置的持久化
/// </summary>
public sealed class FileLoggerOptionsTests
{
    /// <summary>
    /// 测试用的日志目录
    /// </summary>
    private readonly string? _testLogDirectory;

    /// <summary>
    /// 每个测试开始前删除设置目录, 避免测试间互相干扰
    /// </summary>
    public FileLoggerOptionsTests()
    {
        _testLogDirectory = Path.GetDirectoryName(Constants.LogSettingsFilePath);
        if (Directory.Exists(_testLogDirectory))
        {
            Directory.Delete(_testLogDirectory, recursive: true);
        }
    }

    [Fact]
    public void 构造_配置无日志级别_默认使用Information()
    {
        var options = new FileLoggerOptions(CreateConfiguration(null));

        Assert.Equal(LogLevel.Information, options.Level);
    }

    [Theory]
    [InlineData("Debug", LogLevel.Debug)]
    [InlineData("Warning", LogLevel.Warning)]
    [InlineData("None", LogLevel.None)]
    public void 构造_配置有效日志级别_使用配置级别(string level, LogLevel expected)
    {
        var options = new FileLoggerOptions(CreateConfiguration(level));

        Assert.Equal(expected, options.Level);
    }

    [Fact]
    public void 构造_配置无效日志级别_回退使用Information()
    {
        var options = new FileLoggerOptions(CreateConfiguration("Invalid"));

        Assert.Equal(LogLevel.Information, options.Level);
    }

    [Fact]
    public void Level_设置新级别_写入日志设置文件()
    {
        Assert.NotNull(_testLogDirectory);

        _ = Directory.CreateDirectory(_testLogDirectory);
        var options = new FileLoggerOptions(CreateConfiguration(null))
        {
            Level = LogLevel.Error
        };

        var root = JsonNode.Parse(File.ReadAllText(Constants.LogSettingsFilePath));
        Assert.Equal("Error", root?[nameof(FileLoggerOptions)]?[nameof(FileLoggerOptions.Level)]?.GetValue<string>());
    }

    [Fact]
    public void Level_设置新级别但设置目录不存在_不抛出异常()
    {
        _ = new FileLoggerOptions(CreateConfiguration(null))
        {
            Level = LogLevel.Error
        };

        Assert.False(File.Exists(Constants.LogSettingsFilePath));
    }

    /// <summary>
    /// 创建指定日志级别的配置对象
    /// </summary>
    private static IConfiguration CreateConfiguration(string? level)
    {
        var section = new Mock<IConfigurationSection>();
        _ = section.SetupGet(s => s[nameof(FileLoggerOptions.Level)]).Returns(level);
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c.GetSection(nameof(FileLoggerOptions))).Returns(section.Object);
        return configuration.Object;
    }
}
