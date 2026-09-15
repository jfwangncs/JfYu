using JfYu.Data.Constant;
using Microsoft.EntityFrameworkCore;
using System;

namespace JfYu.Data.MySql
{
    /// <summary>
    /// Resolves the <see cref="ServerVersion"/> for MySQL/MariaDB connections.
    /// </summary>
    internal static class MySqlVersionResolver
    {
        /// <summary>
        /// Resolves the server version from the configuration, auto-detecting when no explicit version is provided.
        /// </summary>
        /// <param name="config">The database configuration.</param>
        /// <returns>The resolved server version, or null when auto-detection applies.</returns>
        internal static ServerVersion? Resolve(DatabaseConfig config)
        {
            if (!string.IsNullOrEmpty(config.Version))
            {
                var version = new Version(config.Version);
                return config.DatabaseType == DatabaseType.MySql
                    ? new MySqlServerVersion(version)
                    : new MariaDbServerVersion(version);
            }

            return ServerVersion.AutoDetect(config.ConnectionString);
        }
    }
}
