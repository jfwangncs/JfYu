using JfYu.Redis.Serializer;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
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
        /// Redis client
        /// </summary>
        IConnectionMultiplexer Client { get; }

        /// <summary>
        /// Redis IDatabase
        /// </summary>
        IDatabase Database { get; }

        /// <summary>
        /// Gets the instance of <see cref="ISerializer" />
        /// </summary>
        ISerializer Serializer { get; }

        /// <summary>
        /// Logs a message with the specified method name, key, and optional value.
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The optional value to log.</param>
        void Log(string methodName, string key, object? value = null);

        /// <summary>
        /// Verify that the specified key exists
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Removes the value of specified key
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///Removes all specified keys
        /// </summary>
        /// <param name="keys">The Redis keys.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The number of elements removed.</returns>
        Task<long> RemoveAllAsync(List<string> keys, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets the value of the specified key
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None..</param>
        /// <returns>The value associated with the key, or null if the key does not exist.</returns>
        Task<T?> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets the value of the specified key
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="expiresIn">The time span after which the key will expire.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The value associated with the key, or null if the key does not exist.</returns>
        Task<T?> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Adds a value with specified key to the Redis
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="expiresIn">The time span after which the key will expire.</param>
        /// <param name="when">The command condition.Default is  When.Always</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Adds a value with specified key to the Redis
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="when">The command condition.Default is  When.Always</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Expires specified key
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="expiresIn">The time span after which the key will expire.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> ExpireAsync(string key, TimeSpan expiresIn);

        /// <summary>
        /// Increments the numeric value of a key by the specified value,attention only support Serializer=NewtonsoftSerializer
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value by which to increment the key's value. Default is 1.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns> The new numeric value.</returns>
        Task<long> IncrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Increments the numeric value of a key by the specified value only support Serializer=NewtonsoftSerializer
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value by which to increment the key's value. Default is 1.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The new numeric value..</returns>
        Task<double> IncrementAsync(string key, double value, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Decrements the numeric value of a key by the specified value only support Serializer=NewtonsoftSerializer
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value by which to increment the key's value. Default is 1.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The new numeric value.</returns>
        Task<long> DecrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Decrements the numeric value of a key by the specified value only support Serializer=NewtonsoftSerializer
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value by which to increment the key's value. Default is 1.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The new numeric value.</returns>
        Task<double> DecrementAsync(string key, double value, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Attempts to acquire a distributed lock.
        /// </summary>
        /// <param name="key">The unique identifier for the lock.</param>
        /// <param name="expiresIn">The time span for which the lock will be held,default 1 minute</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> LockTakeAsync(string key, TimeSpan? expiresIn = null);

        /// <summary>
        /// Attempts to release a distributed lock.
        /// </summary>
        /// <param name="key">The unique identifier for the lock.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> LockReleaseAsync(string key);

        /// <summary>
        /// Gets multiple values for the specified keys.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="keys">The Redis keys.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>A dictionary of keys and their associated values.</returns>
        Task<Dictionary<string, T?>> GetBatchAsync<T>(List<string> keys, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Adds multiple key-value pairs to Redis.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="keyValues">Dictionary of keys and values to add.</param>
        /// <param name="expiresIn">Optional expiration time for all keys.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if all operations were successful, false otherwise.</returns>
        Task<bool> AddBatchAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiresIn = null, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Gets the remaining time to live for a key.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>The time to live, or null if the key does not exist or has no expiration.</returns>
        Task<TimeSpan?> GetTimeToLiveAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Removes the expiration from a key.
        /// </summary>
        /// <param name="key">The Redis key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> PersistAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        /// Pings the Redis server to check connectivity.
        /// </summary>
        /// <returns>The response time, or TimeSpan.Zero if failed.</returns>
        Task<TimeSpan> PingAsync();
    }
}