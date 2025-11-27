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
        /// Adds an element to the tail of the list.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="when">The command condition.Default is  When.Always</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements.</returns>
        Task<long> ListAddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Add an element to the head of the list.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="when">The command condition.Default is  When.Always</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements.</returns>
        Task<long> ListAddToLeftAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Pop an element from the tail of the list.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The last element of the list, or null if the key does not exist.</returns>
        Task<T?> ListPopFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Pop an element from the head of the list.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The head element of the list, or null if the key does not exist.</returns>
        Task<T?> ListPopFromLeftAsync<T>(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Get the length of list
        /// </summary>
        /// <param name="key">The key of the list.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements.</returns>
        Task<long> ListLengthAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Remove the specified number of values.
        ///  <list type="bullet">
        ///     <value>count &gt; 0: Remove elements equal to value moving from head to tail.</value>
        ///     <value>count &lt; 0: Remove elements equal to value moving from tail to head.</value>
        ///     <value>count = 0: Remove all elements equal to value.</value>
        /// </list>
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="count">The count behavior</param>
        /// <returns>The number of removed elements.</returns>
        Task<long> ListRemoveAsync<T>(string key, T value, int count);

        /// <summary>
        ///  Returns the specified elements of the list stored at key.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="start">start index</param>
        /// <param name="stop">stop index</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The all element of the list.</returns>
        Task<List<RedisValue>> ListGetRangeAsync(string key, int start, int stop = -1, CommandFlags flag = CommandFlags.None);
    }
}