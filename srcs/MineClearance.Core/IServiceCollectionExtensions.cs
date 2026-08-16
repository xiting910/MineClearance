using Microsoft.Extensions.DependencyInjection;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Services;

namespace MineClearance.Core;

/// <summary>
/// <see cref="Core"/> 层服务的 DI 注册扩展方法
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
        /// 注册 <see cref="Core"/> 层的所有服务
        /// </summary>
        /// <returns>服务集合</returns>
        public IServiceCollection AddCore()
        {
            return services
                .AddSingleton<IGameBoardDictionaryFactory, GameBoardDictionaryFactory>()
                .AddSingleton<IGameFactory, GameFactory>()
                .AddSingleton<IGameManager, GameManager>()
                .AddScoped<IMineField, MineField>()
                .AddScoped<IMineSolver, MineSolver>()
                .AddScoped<IGameTimer, GameTimer>()
                .AddTransient<IMineGenerator, MineGenerator>();
        }
    }
}
