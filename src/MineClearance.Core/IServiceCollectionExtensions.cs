using Microsoft.Extensions.DependencyInjection;

namespace MineClearance.Core;

/// <summary>
/// Core 层服务的 DI 注册扩展方法
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Core 层的所有服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        return services;
    }
}
