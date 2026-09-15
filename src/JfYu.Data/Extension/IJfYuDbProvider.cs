using JfYu.Data.Constant;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Data.Extension
{
    /// <summary>
    /// Defines a database provider that configures a <see cref="DbContextOptionsBuilder"/> for a specific database type.
    /// Implementations are supplied by the individual JfYu.Data provider packages (e.g. JfYu.Data.SqlServer).
    /// </summary>
    public interface IJfYuDbProvider
    {
        /// <summary>
        /// The database type this provider supports.
        /// </summary>
        DatabaseType DatabaseType { get; }

        /// <summary>
        /// Configures the options builder using the provider-specific UseXxx method.
        /// </summary>
        /// <param name="config">The database configuration containing the connection string and optional version.</param>
        /// <param name="builder">The options builder to configure.</param>
        void Configure(DatabaseConfig config, DbContextOptionsBuilder builder);
    }
}
