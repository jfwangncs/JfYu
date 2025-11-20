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
        /// Adds a value with the specified score to the sorted set stored at key.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="score">The score associated with the value.</param>
        /// <param name="when">The command condition.Default is  When.Always</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> SortedSetAddAsync<T>(string key, T value, double score, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Adds multiple values with their scores to the sorted set stored at key.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">A list of values and their scores.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements added to the sorted set.</returns>
        Task<long> SortedSetAddAllAsync<T>(string key, Dictionary<T, double> values, CommandFlags flag = CommandFlags.None) where T : notnull;

        /// <summary>
        /// Removes the specified values from the sorted set stored at key.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="values">The values to remove.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements removed from the sorted set.</returns>
        Task<long> SortedSetRemoveAsync<T>(string key, List<T> values, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Increments the score of the specified value in the sorted set stored at key.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value whose score to increment.</param>
        /// <param name="increment">The value to increment the score by.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The new score of the value after the increment.</returns>
        Task<double> SortedSetIncrementScoreAsync<T>(string key, T value, double increment, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Returns the rank (position) of the specified value in the sorted set.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value to get the rank for.</param>
        /// <param name="order">The order to use when ranking the value. Default is ascending.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The rank of the value, or null if the value does not exist.</returns>
        Task<long?> SortedSetRankAsync<T>(string key, T value, Order order = Order.Ascending, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Returns all the values of the sorted set stored at key, in the range [start, stop].
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="start">The start index of the range.</param>
        /// <param name="stop">The end index of the range.</param>
        /// <param name="order">The order to use when listing the values. Default is ascending.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>A list of values in the specified range.</returns>
        Task<List<RedisValue>> SortedSetRangeByRankAsync(string key, long start, long stop, Order order = Order.Ascending, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Returns all the values of the sorted set stored at key, with a score between min and max.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="min">The minimum score (inclusive).</param>
        /// <param name="max">The maximum score (inclusive).</param>
        /// <param name="exclude">Optionally exclude min and/or max from the range.</param>
        /// <param name="order">The order to use when listing the values. Default is ascending.</param>
        /// <param name="skip">The number of elements to skip before starting to return the elements.</param>
        /// <param name="take">The number of elements to return.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>A list of values in the specified score range.</returns>
        Task<List<RedisValue>> SortedSetRangeByScoreAsync(string key, double min, double max, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Returns the number of elements in the sorted set stored at key, with a score between min and max.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="min">The minimum score (inclusive).</param>
        /// <param name="max">The maximum score (inclusive).</param>
        /// <param name="exclude">Optionally exclude min and/or max from the range.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements in the specified score range.</returns>
        Task<long> SortedSetCountAsync(string key, double min, double max, Exclude exclude = Exclude.None, CommandFlags flag = CommandFlags.None);
    }
}