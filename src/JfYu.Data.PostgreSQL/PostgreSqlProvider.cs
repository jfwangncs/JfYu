using JfYu.Data.Constant;
using JfYu.Data.Extension;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Data.PostgreSQL
{
    /// <summary>
    /// JfYu.Data database provider for PostgreSQL.
    /// </summary>
    public class PostgreSqlProvider : IJfYuDbProvider
    {
        /// <inheritdoc/>
        public DatabaseType DatabaseType => DatabaseType.PostgreSQL;

        /// <inheritdoc/>
        public void Configure(DatabaseConfig config, DbContextOptionsBuilder builder)
        {
            builder.UseNpgsql(config.ConnectionString);
        }
    }
}
