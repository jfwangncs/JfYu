using JfYu.Data.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JfYu.Data.PostgreSQL
{
    /// <summary>
    /// Extension methods for registering the JfYu.Data PostgreSQL provider.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the PostgreSQL database provider so that AddJfYuDbContext can configure PostgreSQL connections.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddJfYuPostgreSql(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IJfYuDbProvider, PostgreSqlProvider>());
            return services;
        }
    }
}
