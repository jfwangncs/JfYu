using JfYu.Redis.Interface;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace JfYu.Redis.Implementation
{
    /// <summary>
    /// The implementation of the Redis service.
    /// </summary>
    public partial class RedisService : IRedisService
    {
        /// <inheritdoc/>
        public async Task<bool> HashSetAsync<T>(string key, string hashKey, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashKey);
            ArgumentNullException.ThrowIfNull(value);
            Log(nameof(HashSetAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.HashSetAsync(key, hashKey, entryBytes, when, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<T?> HashGetAsync<T>(string key, string hashKey, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashKey);
            Log(nameof(HashGetAsync), key);
            var redisValue = await _database.HashGetAsync(key, hashKey, flag).ConfigureAwait(false);
            return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue!) : default;
        }

        /// <inheritdoc/>
        public async Task<HashEntry[]> HashGetAllAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(HashGetAllAsync), key);
            return await _database.HashGetAllAsync(key, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> HashDeleteAsync(string key, string hashKey, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashKey);
            Log(nameof(HashDeleteAsync), key);
            return await _database.HashDeleteAsync(key, hashKey, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> HashExistsAsync(string key, string hashKey, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashKey);
            Log(nameof(HashExistsAsync), key);
            return await _database.HashExistsAsync(key, hashKey, flag).ConfigureAwait(false);
        }
    }
}