using JfYu.Data.Constant;
using JfYu.Data.Context;
using JfYu.Data.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace JfYu.Data.Extension
{
    /// <summary>
    /// Provides extension methods for registering JfYu.Data services with read-write separation support.
    /// Enables master-slave database configuration with automatic random load balancing for read operations.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Adds and configures the JfYu DbContext service with read-write separation to the dependency injection container.
        /// Registers the master context, readonly contexts (if configured), and IService implementations.
        /// </summary>
        /// <typeparam name="T">The type of DbContext to configure (must inherit from DbContext).</typeparam>
        /// <param name="services">The IServiceCollection to add the DbContext service to.</param>
        /// <param name="setupAction">An action to configure the JfYuDatabaseConfig options including connection strings and readonly replicas.</param>
        /// <param name="extraConfigure">An optional action to further configure the DbContextOptionsBuilder (e.g., EnableDetailedErrors, EnableSensitiveDataLogging).</param>
        /// <returns>The modified IServiceCollection instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when setupAction is null, ConnectionString is null/whitespace, or ReadOnlyDatabases contains invalid connection strings.</exception>
        /// <remarks>
        /// The method automatically:
        /// - Registers the master DbContext for write operations
        /// - Registers keyed readonly contexts (named "JfYuReadOnly0", "JfYuReadOnly1", etc.) for each slave database
        /// - Registers ReadonlyDBContext that randomly selects a readonly replica (or falls back to master if none configured)
        /// - Registers IService{T, TContext} for CRUD operations with automatic read-write separation
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddJfYuDbContext&lt;MyDbContext&gt;(options =>
        /// {
        ///     options.ConnectionString = "master connection string";
        ///     options.ReadOnlyDatabases = [
        ///         new() { DatabaseType = DatabaseType.SqlServer, ConnectionString = "slave1" },
        ///         new() { DatabaseType = DatabaseType.MySql, ConnectionString = "slave2", Version = "8.0.36" }
        ///     ];
        /// }, db => db.EnableDetailedErrors());
        /// </code>
        /// </example>
        public static IServiceCollection AddJfYuDbContext<T>(this IServiceCollection services, Action<JfYuDatabaseConfig> setupAction, Action<DbContextOptionsBuilder>? extraConfigure = null) where T : DbContext
        {
            ArgumentNullException.ThrowIfNull(setupAction);

            var options = new JfYuDatabaseConfig();
            setupAction.Invoke(options);

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new ArgumentNullException(nameof(setupAction), "ConnectionString cannot be null or whitespace.");

            if (options.ReadOnlyDatabases.Any(db => string.IsNullOrWhiteSpace(db.ConnectionString)))
                throw new ArgumentNullException(nameof(setupAction), "ReadOnlyDatabases is null or dont have one valid connectionString.");

            services.AddDbContext<T>(q =>
            {
                GetDbContextOptions<T>(options, extraConfigure, q);
            });

            for (int i = 0; i < options.ReadOnlyDatabases.Count; i++)
            {
                var dbConfig = options.ReadOnlyDatabases[i];
                services.AddKeyedScoped($"{options.JfYuReadOnly}{i}", (provider, t) =>
                {
                    var dbContextOptions = GetDbContextOptions<T>(dbConfig, extraConfigure);
                    return (T)Activator.CreateInstance(typeof(T), dbContextOptions)!;
                });
            }
            services.AddScoped(provider =>
            {
                if (options.ReadOnlyDatabases.Count < 1)
                    return new ReadonlyDBContext<T>(provider.GetRequiredService<T>());
                var randomKey = Random.Shared.Next(options.ReadOnlyDatabases.Count);
                return new ReadonlyDBContext<T>(provider.GetRequiredKeyedService<T>($"{options.JfYuReadOnly}{randomKey}"));
            });

            services.AddScoped(typeof(ReadonlyDBContext<>));

            services.AddScoped(typeof(IService<,>), typeof(Service<,>));

            return services;
        }

        private static DbContextOptions GetDbContextOptions<T>(DatabaseConfig config, Action<DbContextOptionsBuilder>? extraConfigure = null, DbContextOptionsBuilder? opt = null) where T : DbContext
        {
            var optionsBuilder = opt ?? new DbContextOptionsBuilder<T>();

            ServerVersion? serverVersion = null;

            if (config.DatabaseType == DatabaseType.MySql || config.DatabaseType == DatabaseType.MariaDB)
            {
                if (!string.IsNullOrEmpty(config.Version))
                {
                    var version = new Version(config.Version);
                    serverVersion = config.DatabaseType == DatabaseType.MySql
                        ? new MySqlServerVersion(version)
                        : new MariaDbServerVersion(version);
                }
                else
                {
                    serverVersion = ServerVersion.AutoDetect(config.ConnectionString);
                }
            }

            switch (config.DatabaseType)
            {
                case DatabaseType.MySql:
                case DatabaseType.MariaDB:
                    optionsBuilder.UseMySql(config.ConnectionString, serverVersion);
                    break;

                case DatabaseType.Sqlite:
                    optionsBuilder.UseSqlite(config.ConnectionString);
                    break;

                case DatabaseType.Memory:
                    optionsBuilder.UseInMemoryDatabase(config.ConnectionString);
                    break;

                default:
                    optionsBuilder.UseSqlServer(config.ConnectionString).EnableDetailedErrors().EnableSensitiveDataLogging();
                    break;
            }
            extraConfigure?.Invoke(optionsBuilder);
            return optionsBuilder.Options;
        }
    }
}