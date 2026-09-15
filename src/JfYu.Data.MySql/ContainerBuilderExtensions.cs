using JfYu.Data.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JfYu.Data.MySql
{
    /// <summary>
    /// Extension methods for registering the JfYu.Data MySQL and MariaDB providers.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the MySQL and MariaDB database providers so that AddJfYuDbContext can configure their connections.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddJfYuMySql(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IJfYuDbProvider, MySqlProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IJfYuDbProvider, MariaDbProvider>());
            return services;
        }
    }
}
