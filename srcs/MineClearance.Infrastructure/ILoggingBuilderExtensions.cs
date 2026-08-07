using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Services;

namespace MineClearance.Infrastructure;

/// <summary>
/// <see cref="ILoggingBuilder"/> 的扩展类
/// </summary>
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// <see cref="ILoggingBuilder"/> 类的扩展
    /// </summary>
    /// <param name="loggingBuilder">日志构建器</param>
    extension(ILoggingBuilder loggingBuilder)
    {
        /// <summary>
        /// 注册文件日志记录器
        /// </summary>
        public void AddFileLogger()
        {
            loggingBuilder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>()
            );
        }
    }
}
