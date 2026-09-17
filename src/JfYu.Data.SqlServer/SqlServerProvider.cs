using JfYu.Data.Constant;
using JfYu.Data.Extension;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Data.SqlServer
{
    /// <summary>
    /// JfYu.Data database provider for Microsoft SQL Server.
    /// </summary>
    public class SqlServerProvider : IJfYuDbProvider
    {
        /// <inheritdoc/>
        public DatabaseType DatabaseType => DatabaseType.SqlServer;

        /// <inheritdoc/>
        public void Configure(DatabaseConfig config, DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(config.ConnectionString).EnableDetailedErrors().EnableSensitiveDataLogging();
        }
    }
}
