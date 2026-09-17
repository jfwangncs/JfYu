using JfYu.Data.Constant;
using JfYu.Data.Extension;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Data.MySql
{
    /// <summary>
    /// JfYu.Data database provider for MySQL.
    /// </summary>
    public class MySqlProvider : IJfYuDbProvider
    {
        /// <inheritdoc/>
        public DatabaseType DatabaseType => DatabaseType.MySql;

        /// <inheritdoc/>
        public void Configure(DatabaseConfig config, DbContextOptionsBuilder builder)
        {
            builder.UseMySql(config.ConnectionString, MySqlVersionResolver.Resolve(config));
        }
    }

    /// <summary>
    /// JfYu.Data database provider for MariaDB.
    /// </summary>
    public class MariaDbProvider : IJfYuDbProvider
    {
        /// <inheritdoc/>
        public DatabaseType DatabaseType => DatabaseType.MariaDB;

        /// <inheritdoc/>
        public void Configure(DatabaseConfig config, DbContextOptionsBuilder builder)
        {
            builder.UseMySql(config.ConnectionString, MySqlVersionResolver.Resolve(config));
        }
    }
}
