using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using MineClearance.Infrastructure.Services;
using Moq;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// FileLoggerProvider 的单元测试, 覆盖日志级别过滤与日志内容写入
/// </summary>
public sealed class FileLoggerProviderTests
{
    /// <summary>
    /// 每个测试开始前重置最新日志文件并创建日志目录, 避免测试间互相干扰
    /// </summary>
    public FileLoggerProviderTests()
    {
        ResetPath(Constants.LatestLogFilePath);
        _ = Directory.CreateDirectory(Path.GetDirectoryName(Constants.LatestLogFilePath)!);
    }

    [Fact]
    public void CreateLogger_记录日志_内容写入最新日志文件()
    {
        using (var provider = CreateProvider())
        {
            provider.CreateLogger("测试类别").LogInformation("Hello world");
        }

        var content = File.ReadAllText(Constants.LatestLogFilePath);
        Assert.Contains("[Information]", content);
        Assert.Contains("测试类别", content);
        Assert.Contains("Hello world", content);
    }

    [Fact]
    public void CreateLogger_日志级别低于配置级别_不写入文件()
    {
        using (var provider = CreateProvider(LogLevel.Warning))
        {
            provider.CreateLogger("测试类别").LogInformation("Hello world");
        }

        Assert.Equal(string.Empty, File.ReadAllText(Constants.LatestLogFilePath));
    }

    [Theory]
    [InlineData(LogLevel.Information, LogLevel.Debug, false)]
    [InlineData(LogLevel.Information, LogLevel.Information, true)]
    [InlineData(LogLevel.Information, LogLevel.Error, true)]
    [InlineData(LogLevel.None, LogLevel.Information, false)]
    public void IsEnabled_按配置级别过滤(LogLevel configuredLevel, LogLevel logLevel, bool expected)
    {
        using var provider = CreateProvider(configuredLevel);
        var logger = provider.CreateLogger("测试类别");

        Assert.Equal(expected, logger.IsEnabled(logLevel));
    }

    [Fact]
    public void Log_带异常_异常信息写入文件()
    {
        using (var provider = CreateProvider())
        {
            provider.CreateLogger("测试类别").LogError(new InvalidOperationException("测试异常"), "发生错误");
        }

        var content = File.ReadAllText(Constants.LatestLogFilePath);
        Assert.Contains("发生错误", content);
        Assert.Contains("测试异常", content);
    }

    [Fact]
    public void Log_多次记录_日志按行追加()
    {
        using (var provider = CreateProvider())
        {
            var logger = provider.CreateLogger("测试类别");
            logger.LogInformation("第一条");
            logger.LogInformation("第二条");
        }

        Assert.Equal(2, File.ReadAllLines(Constants.LatestLogFilePath).Length);
    }

    /// <summary>
    /// 创建指定日志级别的文件日志提供程序
    /// </summary>
    private static FileLoggerProvider CreateProvider(LogLevel level = LogLevel.Information)
    {
        var section = new Mock<IConfigurationSection>();
        _ = section.SetupGet(s => s[nameof(FileLoggerOptions.Level)]).Returns(level.ToString());
        var configuration = new Mock<IConfiguration>();
        _ = configuration.Setup(c => c.GetSection(nameof(FileLoggerOptions))).Returns(section.Object);
        return new(new FileLoggerOptions(configuration.Object));
    }

    /// <summary>
    /// 重置指定路径, 兼容文件与目录两种占用形式
    /// </summary>
    private static void ResetPath(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
