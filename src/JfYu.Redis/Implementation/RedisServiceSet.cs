using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JfYu.Redis.Implementation
{
    /// <summary>
    /// The implementation of the Redis service.
    /// </summary>
    public partial class RedisService : IRedisService
    {
        /// <summary>
        /// Adds an element to the set.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="key">The Redis key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="flag">Optional command flags. Default is CommandFlags.None.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public async Task<bool> SetAddAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            Log(nameof(SetAddAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.SetAddAsync(key, entryBytes, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<long> SetAddAllAsync<T>(string key, List<T> values, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            values.ThrowListIfNullOrEmpty();
            Log(nameof(SetAddAllAsync), key);
            return await _database.SetAddAsync(key, values
                    .Select(item => Serializer.Serialize(item))
                    .Select(x => (RedisValue)x)
                    .ToArray(), flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> SetRemoveAsync<T>(string key, T value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            Log(nameof(SetRemoveAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.SetRemoveAsync(key, entryBytes).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<long> SetRemoveAllAsync<T>(string key, List<T> values, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            values.ThrowListIfNullOrEmpty();
            values.ForEach(value => ArgumentNullException.ThrowIfNull(value));
            Log(nameof(SetRemoveAllAsync), key);
            return await _database.SetRemoveAsync(key, values
                   .Select(item => Serializer.Serialize(item))
                   .Select(x => (RedisValue)x)
                   .ToArray(), flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> SetContainsAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            Log(nameof(SetContainsAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.SetContainsAsync(key, entryBytes).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<List<RedisValue>> SetMembersAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(SetMembersAsync), key);
            return [.. await _database.SetMembersAsync(key, flag).ConfigureAwait(false)];
        }

        /// <inheritdoc/>
        public async Task<long> SetLengthAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(SetLengthAsync), key);
            return await _database.SetLengthAsync(key, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<RedisValue> SetRandomMemberAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(SetRandomMemberAsync), key);
            return await _database.SetRandomMemberAsync(key, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<List<RedisValue>> SetRandomMembersAsync(string key, int count, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(SetRandomMembersAsync), key);
            return [.. await _database.SetRandomMembersAsync(key, count, flag).ConfigureAwait(false)];
        }
    }
}