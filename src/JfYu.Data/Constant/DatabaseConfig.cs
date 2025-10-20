namespace JfYu.Data.Constant
{
    /// <summary>
    /// Database Configuration.
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// SqlServer/MySql/MariaDB/Sqlite/Memory default:SqlServer.
        /// </summary>
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;

        /// <summary>
        /// ConnectionString.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Version of the database, used for MySql and MariaDB.
        /// </summary>
        public string Version { get; set; } = string.Empty;
    }
}