using JfYu.Redis.Interface;
using JfYu.Redis.Options;
using JfYu.Redis.Serializer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
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
        private readonly IConnectionMultiplexer _client;
        private readonly IDatabase _database;
        private readonly ISerializer _serializer;
        private readonly ILogger<RedisService>? _logger;
        private readonly RedisOptions _configuration;

        /// <summary>
        /// Redis client
        /// </summary>
        public IConnectionMultiplexer Client => _client;

        /// <summary>
        /// Redis IDatabase
        /// </summary>
        public IDatabase Database => _database;

        /// <summary>
        /// Gets the instance of <see cref="ISerializer" />
        /// </summary>
        public ISerializer Serializer => _serializer;

        /// <summary>
        ///
        /// </summary>
        /// <param name="redisConfiguration"></param>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public RedisService(IOptions<RedisOptions> redisConfiguration, IConnectionMultiplexer client, ISerializer serializer, ILogger<RedisService>? logger = null)
        {
            _configuration = redisConfiguration.Value;
            _logger = logger;
            _client = client;
            if (string.IsNullOrEmpty(redisConfiguration.Value.Prefix))
                _database = Client.GetDatabase(redisConfiguration.Value.DbIndex);
            else
                _database = Client.GetDatabase(redisConfiguration.Value.DbIndex).WithKeyPrefix(redisConfiguration.Value.Prefix);
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public void Log(string methodName, string key, LogLevel logLevel = LogLevel.Trace)
        {
            if (_configuration.EnableLogs)
                _logger?.Log(logLevel, "Redis {Method} - Key: {Key}", methodName, key);
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(ExistsAsync), key);
            return _database.KeyExistsAsync(key, flag);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(RemoveAsync), key);
            return await _database.KeyDeleteAsync(key, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<long> RemoveAllAsync(List<string> keys, CommandFlags flag = CommandFlags.None)
        {
            if (keys == null || keys.Count == 0)
                throw new ArgumentException("The parameter 'keys' cannot be null or empty.", nameof(keys));
            Log(nameof(RemoveAllAsync), string.Join(", ", keys));
            var redisKeys = keys.Select(q => (RedisKey)q);
            return _database.KeyDeleteAsync([.. redisKeys], flag);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(GetAsync), key);
            var valueBytes = await _database.StringGetAsync(key, flag).ConfigureAwait(false);
            return !valueBytes.HasValue ? default : Serializer.Deserialize<T>(valueBytes!);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            var result = await GetAsync<T>(key, flag).ConfigureAwait(false);
            Log(nameof(GetAsync), key);
            if (!EqualityComparer<T?>.Default.Equals(result, default))
                await _database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            Log(nameof(AddAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.StringSetAsync(key, entryBytes, expiresIn, when, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
            Log(nameof(AddAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.StringSetAsync(key, entryBytes, null, when, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiresIn)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(ExpireAsync), key);
            return await _database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<long> IncrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(IncrementAsync), key);
            return await _database.StringIncrementAsync(key, value, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<double> IncrementAsync(string key, double value, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(IncrementAsync), key);
            return await _database.StringIncrementAsync(key, value, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<long> DecrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(DecrementAsync), key);
            return await _database.StringDecrementAsync(key, value, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<double> DecrementAsync(string key, double value, CommandFlags flag = CommandFlags.None)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(DecrementAsync), key);
            return await _database.StringDecrementAsync(key, value, flag).ConfigureAwait(false);
        }

        private static readonly RedisValue LockToken = Environment.MachineName;

        /// <inheritdoc/>
        public async Task<bool> LockTakeAsync(string key, TimeSpan? expiresIn = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(LockTakeAsync), key);
            return await _database.LockTakeAsync(key, LockToken, expiresIn ?? TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        }

        /// <inheritdoc/>

        public async Task<bool> LockReleaseAsync(string key)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            Log(nameof(LockReleaseAsync), key);
            return await _database.LockReleaseAsync(key, LockToken).ConfigureAwait(false);
        }
    }
}