using StackExchange.Redis;
using System.Threading.Tasks;

namespace JfYu.Redis.Interface
{
    /// <summary>
    /// The interface for the Redis service.
    /// </summary>
    public partial interface IRedisService
    {
        /// <summary>
        /// Adds a value with specified key and hashKey to the Redis
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="hashKey">The key of hash.</param>
        /// <param name="value">The value associated with the hashKey.</param>
        /// <param name="when">The command condition.Default is  When.Always</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> HashSetAsync<T>(string key, string hashKey, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets a value with specified key and hashKey from the Redis
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="hashKey">The key of hash.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The value associated with the key, or null if the key does not exist.</returns>
        Task<T?> HashGetAsync<T>(string key, string hashKey, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets all values with specified key from the Redis
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The values associated with the key, or null if the key does not exist.</returns>
        Task<HashEntry[]> HashGetAllAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Deletes a field from a Redis hash.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="hashKey">The key of hash.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise..</returns>
        Task<bool> HashDeleteAsync(string key, string hashKey, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Checks if a field exists in a Redis hash.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="hashKey">The key of hash.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> HashExistsAsync(string key, string hashKey, CommandFlags flag = CommandFlags.None);
    }
}