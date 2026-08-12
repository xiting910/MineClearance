using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineClearance.Core.Interfaces;
using MineClearance.Infrastructure.Models;
using MineClearance.Infrastructure.Services;
using Moq;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// 服务注册扩展方法的单元测试, 覆盖 AddInfrastructure 与 AddFileLogger 的注册行为
/// </summary>
public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddInfrastructure_注册数据仓储服务()
    {
        var services = new Mock<IServiceCollection>();

        _ = services.Object.AddInfrastructure();

        services.Verify(s => s.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IGameDataRepository) &&
            d.Lifetime == ServiceLifetime.Singleton
        )), Times.Once);
    }

    [Fact]
    public void AddInfrastructure_注册更新服务()
    {
        var services = new Mock<IServiceCollection>();

        _ = services.Object.AddInfrastructure();

        services.Verify(s => s.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IUpdateService) &&
            d.Lifetime == ServiceLifetime.Singleton
        )), Times.Once);
    }

    [Fact]
    public void AddInfrastructure_注册日志级别选项()
    {
        var services = new Mock<IServiceCollection>();

        _ = services.Object.AddInfrastructure();

        services.Verify(s => s.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(FileLoggerOptions) &&
            d.Lifetime == ServiceLifetime.Singleton
        )), Times.Once);
        services.Verify(s => s.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(3));
    }

    [Fact]
    public void AddFileLogger_注册文件日志提供程序()
    {
        var services = new Mock<IServiceCollection>();
        var loggingBuilder = new Mock<ILoggingBuilder>();
        _ = loggingBuilder.SetupGet(b => b.Services).Returns(services.Object);

        loggingBuilder.Object.AddFileLogger();

        services.Verify(s => s.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(ILoggerProvider) &&
            d.Lifetime == ServiceLifetime.Singleton &&
            d.ImplementationType == typeof(FileLoggerProvider)
        )), Times.Once);
    }
}
