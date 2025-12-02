using JfYu.Redis.Extensions;
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
        private readonly string _lockToken;

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
        /// Initializes a new instance of the <see cref="RedisService"/> class.
        /// </summary>
        /// <param name="redisConfiguration">The Redis configuration options.</param>
        /// <param name="client">The Redis connection multiplexer.</param>
        /// <param name="serializer">The serializer for Redis values.</param>
        /// <param name="logger">Optional logger for Redis operations.</param>
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
            _lockToken = $"{Environment.MachineName}_{Environment.CurrentManagedThreadId}_{Guid.NewGuid()}";
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Redis {Method} - Key: {Key}, Value: {Value}")]
        static partial void LogRedis(ILogger logger, string method, string key, string value);

        /// <inheritdoc/>
        public void Log(string methodName, string key, object? value = null)
        {
            if (_configuration.EnableLogs && _logger != null)
            {
                var valueStr = value?.ToString() ?? string.Empty;
                var filteredValue = _configuration.ValueFilter?.Invoke(valueStr) ?? valueStr;
                LogRedis(_logger, methodName, key, filteredValue);
            }
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            Log(nameof(ExistsAsync), key);
            return _database.KeyExistsAsync(key, flag);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            Log(nameof(RemoveAsync), key);
            return await _database.KeyDeleteAsync(key, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<long> RemoveAllAsync(List<string> keys, CommandFlags flag = CommandFlags.None)
        {
            ArgumentNullExceptionExtension.ThrowIfNullOrEmpty(keys);
            Log(nameof(RemoveAllAsync), string.Join(", ", keys));

            var redisKeys = new RedisKey[keys.Count];
            for (int i = 0; i < keys.Count; i++)
            {
                redisKeys[i] = keys[i];
            }

            return _database.KeyDeleteAsync(redisKeys, flag);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif           
            var valueBytes = await _database.StringGetAsync(key, flag).ConfigureAwait(false);
            var result = !valueBytes.HasValue ? default : Serializer.Deserialize<T>(valueBytes!);
            Log(nameof(GetAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            var result = await GetAsync<T>(key, flag).ConfigureAwait(false);
            if (!EqualityComparer<T?>.Default.Equals(result, default))
                await _database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);
            Log(nameof(GetAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullExceptionExtension.ThrowIfNull(value);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
#endif
            Log(nameof(AddAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.StringSetAsync(key, entryBytes, expiresIn, when, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullExceptionExtension.ThrowIfNull(value);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
#endif
            Log(nameof(AddAsync), key);
            var entryBytes = _serializer.Serialize(value);
            return await _database.StringSetAsync(key, entryBytes, null, when, flag).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiresIn)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            Log(nameof(ExpireAsync), key);
            return await _database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<long> IncrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif 
            var result = await _database.StringIncrementAsync(key, value, flag).ConfigureAwait(false);
            Log(nameof(IncrementAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<double> IncrementAsync(string key, double value = 1.0, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif 
            var result = await _database.StringIncrementAsync(key, value, flag).ConfigureAwait(false);
            Log(nameof(IncrementAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<long> DecrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif 
            var result = await _database.StringDecrementAsync(key, value, flag).ConfigureAwait(false);
            Log(nameof(DecrementAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<double> DecrementAsync(string key, double value = 1.0, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif 
            var result = await _database.StringDecrementAsync(key, value, flag).ConfigureAwait(false);
            Log(nameof(DecrementAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> LockTakeAsync(string key, TimeSpan? expiresIn = null)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            Log(nameof(LockTakeAsync), key);
            return await _database.LockTakeAsync(key, _lockToken, expiresIn ?? TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> LockReleaseAsync(string key)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            Log(nameof(LockReleaseAsync), key);
            return await _database.LockReleaseAsync(key, _lockToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, T?>> GetBatchAsync<T>(List<string> keys, CommandFlags flag = CommandFlags.None)
        {
            if (keys == null || keys.Count == 0)
                throw new ArgumentException("The parameter 'keys' cannot be null or empty.", nameof(keys));

            var redisKeys = new RedisKey[keys.Count];
            for (int i = 0; i < keys.Count; i++)
            {
                redisKeys[i] = keys[i];
            }

            var values = await _database.StringGetAsync(redisKeys, flag).ConfigureAwait(false);

            var result = new Dictionary<string, T?>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i].HasValue ? Serializer.Deserialize<T>(values[i]!) : default;
            }

            Log(nameof(GetBatchAsync), string.Join(", ", keys), result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> AddBatchAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiresIn = null, CommandFlags flag = CommandFlags.None)
        {
            if (keyValues == null || keyValues.Count == 0)
                throw new ArgumentException("The parameter 'keyValues' cannot be null or empty.", nameof(keyValues));

            Log(nameof(AddBatchAsync), string.Join(", ", keyValues.Keys));

            var tasks = new Task<bool>[keyValues.Count];
            int index = 0;
            foreach (var kv in keyValues)
            {
                var serializedValue = _serializer.Serialize(kv.Value);
                tasks[index++] = _database.StringSetAsync(kv.Key, serializedValue, expiresIn, When.Always, flag);
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return Array.TrueForAll(results, r => r);
        }

        /// <inheritdoc/>
        public async Task<TimeSpan?> GetTimeToLiveAsync(string key, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
            var result = await _database.KeyTimeToLiveAsync(key, flag).ConfigureAwait(false);
            Log(nameof(GetTimeToLiveAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> PersistAsync(string key, CommandFlags flag = CommandFlags.None)
        {
#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(key);
#else
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif            
            var result = await _database.KeyPersistAsync(key, flag).ConfigureAwait(false);
            Log(nameof(PersistAsync), key, result);
            return result;
        }

        /// <inheritdoc/>
        public async Task<TimeSpan> PingAsync()
        {
            var result = await _database.PingAsync().ConfigureAwait(false);
            Log(nameof(PingAsync), "server", result);
            return result;
        }
    }
}