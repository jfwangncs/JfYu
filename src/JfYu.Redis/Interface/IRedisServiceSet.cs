using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JfYu.Redis.Interface
{
    /// <summary>
    /// The interface for the Redis service.
    /// </summary>
    public partial interface IRedisService
    {
        /// <summary>
        /// Adds an element to the set.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> SetAddAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Adds elements to the set.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="values">The values associated with the key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements added.</returns>
        Task<long> SetAddAllAsync<T>(string key, List<T> values, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Removes the specified value of set.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <returns>True if the operation was successful, false otherwise..</returns>
        Task<bool> SetRemoveAsync<T>(string key, T value);

        /// <summary>
        /// Removes the specified value of set.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="values">The values associated with the key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements removed.</returns>
        Task<long> SetRemoveAllAsync<T>(string key, List<T> values, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Checks value is exist or not in set.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the value was exist, false otherwise.</returns>
        Task<bool> SetContainsAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets all elements of set.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>Values</returns>
        Task<List<RedisValue>> SetMembersAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets the number of elements in the set.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>number of elements</returns>
        Task<long> SetLengthAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets a random member of the set.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The random value of the set.</returns>
        Task<RedisValue> SetRandomMemberAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets random members with count of the set.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="count">the count of members</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The random values of the set.</returns>
        Task<List<RedisValue>> SetRandomMembersAsync(string key, int count, CommandFlags flag = CommandFlags.None);
    }
}