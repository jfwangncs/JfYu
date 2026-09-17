using JfYu.Data.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JfYu.Data.SqlServer
{
    /// <summary>
    /// Extension methods for registering the JfYu.Data SQL Server provider.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the SQL Server database provider so that AddJfYuDbContext can configure SQL Server connections.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddJfYuSqlServer(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IJfYuDbProvider, SqlServerProvider>());
            return services;
        }
    }
}
