using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core.Interfaces;
using MineClearance.Infrastructure.Services;

namespace MineClearance.Infrastructure;

/// <summary>
/// <see cref="Infrastructure"/> 层服务的 DI 注册扩展方法
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> 类的扩展
    /// </summary>
    /// <param name="services">服务集合</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册 <see cref="Infrastructure"/> 层的所有服务
        /// </summary>
        /// <returns>服务集合</returns>
        public IServiceCollection AddInfrastructure()
        {
            return services.AddSingleton<FileLoggerOptions>()
                .AddSingleton<IGameDataRepository, GameDataRepository>();
        }
    }
}
