#if NET8_0_OR_GREATER
using Bogus;
using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using JfYu.UnitTests.Models; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using static JfYu.UnitTests.Redis.RedisBaseTests;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class RedisStringTests
    {
        private readonly IRedisService _redisService;

        public RedisStringTests()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .Build();

            var services = new ServiceCollection();
            var serviceProvider = services.AddRedisService(options =>
            {
                configuration.GetSection("Redis").Bind(options);
                options.UsingNewtonsoft(options =>
                {
                    options.MaxDepth = 12;
                });
            }).BuildServiceProvider();
            _redisService = serviceProvider.GetRequiredService<IRedisService>();
        }

        #region ExpireAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task ExpireAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.ExpireAsync(key, TimeSpan.FromSeconds(3)));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task ExpireAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var key = nameof(ExpireAsync_KeyDoesNotExist_ReturnsFalse); 
            var result = await _redisService.ExpireAsync(key, TimeSpan.FromSeconds(3));
            Assert.False(result);
        }

        [Fact]
        public async Task ExpireAsync_KeyExists_ReturnsTrue()
        {
            var key = nameof(ExpireAsync_KeyExists_ReturnsTrue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.ExpireAsync(key, TimeSpan.FromSeconds(3));
            Assert.True(result);
            var value1= await _redisService.GetAsync<string>(key);
            Assert.Equal(value, value1);
            await Task.Delay(4000);
            value = await _redisService.GetAsync<string>(key);
            Assert.Null(value);
        }

        [Fact]
        public async Task ExpireAsync_WithTimeSpanZero_ReturnsTrue()
        {
            var key = nameof(ExpireAsync_WithTimeSpanZero_ReturnsTrue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.ExpireAsync(key, TimeSpan.Zero);
            Assert.True(result);
            var value1 = await _redisService.GetAsync<string>(key);
            Assert.Null(value1);
        }

        #endregion ExpireAsync

        #region ExistsAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task ExistsAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.ExistsAsync(key));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task ExistsAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var key = nameof(ExistsAsync_KeyDoesNotExist_ReturnsFalse);
            var result = await _redisService.ExistsAsync(key);
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_KeyExists_ReturnsTrue()
        {
            var key = nameof(ExistsAsync_KeyExists_ReturnsTrue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.ExistsAsync(key);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion ExistsAsync

        #region RemoveAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task RemoveAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.RemoveAsync(key));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task RemoveAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var key = nameof(RemoveAsync_KeyDoesNotExist_ReturnsFalse); 
            var result = await _redisService.RemoveAsync(key);
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_KeyExists_ReturnsTrue()
        {
            var key = nameof(RemoveAsync_KeyExists_ReturnsTrue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.RemoveAsync(key);
            Assert.True(result);
        }

        #endregion RemoveAsync

        #region RemoveAllAsync

        [Theory]
        [ClassData(typeof(NullKeysExpectData))]
        public async Task RemoveAllAsync_KeyIsNull_ThrowsException(string[] keys)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.RemoveAllAsync([.. keys]));
            Assert.IsType<ArgumentException>(ex, false); 
        }

        [Fact]
        public async Task RemoveAllAsync_AllKeysExist_ReturnsNumberOfKeysRemoved()
        {
            await _redisService.AddAsync("{user:123}:key1", "value1");
            await _redisService.AddAsync("{user:123}:key2", "value2");
            await _redisService.AddAsync("{user:123}:key3", "value3");

            var result = await _redisService.RemoveAllAsync(["{user:123}:key1", "{user:123}:key2", "{user:123}:key3"]);
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task RemoveAllAsync_AllKeysNotExist_ReturnsNumberOfKeysRemoved()
        {
            var result = await _redisService.RemoveAllAsync(["{user:123}:key1", "{user:123}:key2", "{user:123}:key3"]);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task RemoveAllAsync_AllKeysHalfExist_ReturnsNumberOfKeysRemoved()
        {
            await _redisService.AddAsync("{user:123}:key1", "value1");
            await _redisService.AddAsync("{user:123}:key2", "value2");

            var result = await _redisService.RemoveAllAsync(["{user:123}:key1", "{user:123}:key2", "{user:123}:key3"]);
            Assert.Equal(2, result);
        }

        #endregion RemoveAllAsync

        #region GetAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task GetAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.GetAsync<string>(key));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task GetAsync_KeyDoesNotExist_ReturnsNull()
        {
            var result = await _redisService.GetAsync<string>("nonexistent_key");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_KeyExists_ReturnsValue()
        {
            var key = nameof(GetAsync_KeyExists_ReturnsValue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<string>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueInt_ReturnsValue()
        {
            var key = nameof(GetAsync_ValueInt_ReturnsValue);
            var value = 1;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<int>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueLong_ReturnsValue()
        {
            var key = nameof(GetAsync_ValueLong_ReturnsValue);
            long value = 121132321321;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<long>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueDouble_ReturnsValue()
        {
            var key = nameof(GetAsync_ValueDouble_ReturnsValue);
            double value = 121132321321.3123131414;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<double>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueDecimal_ReturnsValue()
        {
            var key = nameof(GetAsync_ValueDecimal_ReturnsValue);
            decimal value = 12113232132131.3114142131M;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<decimal>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueDateTime_ReturnsValue()
        {
            var key = nameof(GetAsync_ValueDateTime_ReturnsValue);
            DateTime value = DateTime.UtcNow;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<DateTime>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueModel_ReturnsValue()
        {
            var key = nameof(GetAsync_ValueModel_ReturnsValue); 
            var value = new TestModelFaker().GenerateBetween(1, 10);
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<List<TestModel>>(key);
            Assert.Equal(value, result, new TestModelComparer());
            await _redisService.RemoveAsync(key);
        }

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task GetAsync_WithExpiry_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.GetAsync<string>(key, TimeSpan.FromSeconds(10)));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task GetAsync_WithExpiry_KeyDoesNotExist_ReturnsNull()
        {
            var result = await _redisService.GetAsync<string>("nonexistent_key", TimeSpan.FromSeconds(10));
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WithExpiry_KeyExist_ReturnsNull()
        {
            var key = nameof(GetAsync_WithExpiry_KeyExist_ReturnsNull);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<string>(key, TimeSpan.FromSeconds(3));
            Assert.Equal(value, result);
            await Task.Delay(5000);
            result = await _redisService.GetAsync<string>(key);
            Assert.Null(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_WithExpiry_KeyExist_ReturnsValue()
        {
            var key = nameof(GetAsync_WithExpiry_KeyExist_ReturnsValue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<string>(key, TimeSpan.FromSeconds(5));
            Assert.Equal(value, result);
            await Task.Delay(3000);
            result = await _redisService.GetAsync<string>(key);
            Assert.Equal(value, result);
            await Task.Delay(3000);
            result = await _redisService.GetAsync<string>(key);
            Assert.Null(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_WithExpiry_KeyExists_ReturnsValueAndUpdatesExpiry()
        {
            var key = nameof(GetAsync_WithExpiry_KeyExists_ReturnsValueAndUpdatesExpiry);
            var value = "dada";
            var expiry = TimeSpan.FromSeconds(10);
            await _redisService.AddAsync(key, value, expiry);
            var result = await _redisService.GetAsync<string>(key, expiry);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion GetAsync

        #region AddAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task AddAsync_KeyIsNull_ThrowsException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.AddAsync(key, value));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task AddAsync_KeyAlreadyExists_ReturnsNewValue()
        {
            var key = nameof(AddAsync_KeyAlreadyExists_ReturnsNewValue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.AddAsync(key, value);
            Assert.True(result);
            var value1 = await _redisService.GetAsync<string>(key);
            Assert.Equal(value, value1);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task AddAsync_KeyDoesNotExist_ReturnsTrue()
        {
            var key = nameof(AddAsync_KeyDoesNotExist_ReturnsTrue);
            var value = "dada";
            var result = await _redisService.AddAsync(key, value);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task AddAsync_WithExpiry_KeyDoesNotExist_ReturnsTrue()
        {
            var key = nameof(AddAsync_WithExpiry_KeyDoesNotExist_ReturnsTrue);
            var value = "dada";
            var expiry = TimeSpan.FromSeconds(3);
            var result = await _redisService.AddAsync(key, value, expiry);
            Assert.True(result);
            await Task.Delay(5000);
            var value1 = await _redisService.GetAsync<string>(key);
            Assert.Null(value1);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task AddAsync_WithExpiry_KeyExist_ReturnsNewValue()
        {
            var key = nameof(AddAsync_WithExpiry_KeyExist_ReturnsNewValue);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var expiry = TimeSpan.FromSeconds(10);
            var result = await _redisService.AddAsync(key, value, expiry);
            Assert.True(result);
            var value1 = await _redisService.GetAsync<string>(key);
            Assert.Equal(value, value1);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task AddAsync_WithCondition_KeyDoesNotExist_ReturnsTrue()
        {
            var key = nameof(AddAsync_WithCondition_KeyDoesNotExist_ReturnsTrue);
            var value = "dada";
            var result = await _redisService.AddAsync(key, value, When.NotExists);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task AddAsync_WithCondition_KeyExists_ReturnsFalse()
        {
            var key = nameof(AddAsync_WithCondition_KeyExists_ReturnsFalse);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            var result = await _redisService.AddAsync(key, value, When.NotExists);
            Assert.False(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion AddAsync

        #region IncrementAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task IncrementAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.IncrementAsync(key));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task IncrementAsync_KeyExistValueNotNumeric_ThrowsException()
        {
            var key = nameof(IncrementAsync_KeyExistValueNotNumeric_ThrowsException);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            await Assert.ThrowsAsync<RedisServerException>(async () => await _redisService.IncrementAsync(key).ConfigureAwait(true));
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_KeyNotExist_ReturnCorrectly()
        {
            var key = nameof(IncrementAsync_KeyNotExist_ReturnCorrectly);
            var value = 1;
            var result = await _redisService.IncrementAsync(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_KeyExist_ReturnCorrectly()
        {
            var key = nameof(IncrementAsync_KeyExist_ReturnCorrectly);
            var value = 11;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.IncrementAsync(key);
            Assert.Equal(value + 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_WithSpecificValue_ThrowsException()
        {
            var key = nameof(IncrementAsync_WithSpecificValue_ThrowsException);
            var value = 11;
            var incrementValue = 5;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.IncrementAsync(key, incrementValue);
            Assert.Equal(value + incrementValue, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_DoubleKeyExist_ReturnCorrectly()
        {
            var key = nameof(IncrementAsync_DoubleKeyExist_ReturnCorrectly);
            var value = 11.24;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.IncrementAsync(key, 1D);
            Assert.Equal(value + 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_DoubleWithSpecificValue_ThrowsException()
        {
            var key = nameof(IncrementAsync_DoubleWithSpecificValue_ThrowsException);
            var value = 11.24;
            var incrementValue = 5.37;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.IncrementAsync(key, incrementValue);
            Assert.Equal(value + incrementValue, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion IncrementAsync

        #region DecrementAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task DecrementAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.DecrementAsync(key));
            Assert.IsType<ArgumentException>(ex,false);
        }

        [Fact]
        public async Task DecrementAsync_KeyExistValueNotNumeric_ThrowsException()
        {
            var key = nameof(DecrementAsync_KeyExistValueNotNumeric_ThrowsException);
            var value = "dada";
            await _redisService.AddAsync(key, value);
            await Assert.ThrowsAsync<RedisServerException>(async () => await _redisService.DecrementAsync(key).ConfigureAwait(true));
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_KeyNotExist_ReturnCorrectly()
        {
            var key = nameof(DecrementAsync_KeyNotExist_ReturnCorrectly);
            var value = -1;
            var result = await _redisService.DecrementAsync(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_KeyExist_ReturnCorrectly()
        {
            var key = nameof(DecrementAsync_KeyExist_ReturnCorrectly);
            var value = 11;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key);
            Assert.Equal(value - 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_WithSpecificValue_ThrowsException()
        {
            var key = nameof(DecrementAsync_WithSpecificValue_ThrowsException);
            var value = 11;
            var incrementValue = 5;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key, incrementValue);
            Assert.Equal(value - incrementValue, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_DoubleKeyExist_ReturnCorrectly()
        {
            var key = nameof(DecrementAsync_DoubleKeyExist_ReturnCorrectly);
            var value = 11.24;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key, 1D);
            Assert.Equal(value - 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_DoubleWithSpecificValue_ThrowsException()
        {
            var key = nameof(DecrementAsync_DoubleWithSpecificValue_ThrowsException);
            var value = 11.24;
            var incrementValue = 5.37;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key, incrementValue);
            Assert.Equal(value - incrementValue, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion DecrementAsync

        #region GetBatchAsync

        [Fact]
        public async Task GetBatchAsync_WithNullKeys_ThrowsException()
        {
            var ex = await Record.ExceptionAsync(() => _redisService.GetBatchAsync<string>(null!));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task GetBatchAsync_WithEmptyKeys_ThrowsException()
        {
            var ex = await Record.ExceptionAsync(() => _redisService.GetBatchAsync<string>(new List<string>()));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task GetBatchAsync_ReturnsMultipleValues()
        {
            var key1 = nameof(GetBatchAsync_ReturnsMultipleValues) + "_1";
            var key2 = nameof(GetBatchAsync_ReturnsMultipleValues) + "_2";
            var key3 = nameof(GetBatchAsync_ReturnsMultipleValues) + "_3";
            var value1 = "value1";
            var value2 = "value2";

            await _redisService.AddAsync(key1, value1);
            await _redisService.AddAsync(key2, value2);

            var result = await _redisService.GetBatchAsync<string>(new List<string> { key1, key2, key3 });

            Assert.Equal(3, result.Count);
            Assert.Equal(value1, result[key1]);
            Assert.Equal(value2, result[key2]);
            Assert.Null(result[key3]);

            await _redisService.RemoveAsync(key1);
            await _redisService.RemoveAsync(key2);
        }

        [Fact]
        public async Task GetBatchAsync_WithComplexObjects_ReturnsCorrectly()
        {
            var key1 = nameof(GetBatchAsync_WithComplexObjects_ReturnsCorrectly) + "_1";
            var key2 = nameof(GetBatchAsync_WithComplexObjects_ReturnsCorrectly) + "_2";
            var models1 = new TestModelFaker().GenerateBetween(1, 5);
            var models2 = new TestModelFaker().GenerateBetween(1, 5);

            await _redisService.AddAsync(key1, models1);
            await _redisService.AddAsync(key2, models2);

            var result = await _redisService.GetBatchAsync<List<TestModel>>(new List<string> { key1, key2 });

            Assert.Equal(2, result.Count);
            Assert.Equal(models1, result[key1], new TestModelComparer());
            Assert.Equal(models2, result[key2], new TestModelComparer());

            await _redisService.RemoveAsync(key1);
            await _redisService.RemoveAsync(key2);
        }

        #endregion GetBatchAsync

        #region AddBatchAsync

        [Fact]
        public async Task AddBatchAsync_WithNullKeyValues_ThrowsException()
        {
            var ex = await Record.ExceptionAsync(() => _redisService.AddBatchAsync<string>(null!));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task AddBatchAsync_WithEmptyKeyValues_ThrowsException()
        {
            var ex = await Record.ExceptionAsync(() => _redisService.AddBatchAsync(new Dictionary<string, string>()));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task AddBatchAsync_AddsMultipleValues()
        {
            var prefix = nameof(AddBatchAsync_AddsMultipleValues);
            var keyValues = new Dictionary<string, string>
            {
                { $"{prefix}_1", "value1" },
                { $"{prefix}_2", "value2" },
                { $"{prefix}_3", "value3" }
            };

            var result = await _redisService.AddBatchAsync(keyValues);
            Assert.True(result);

            foreach (var kv in keyValues)
            {
                var value = await _redisService.GetAsync<string>(kv.Key);
                Assert.Equal(kv.Value, value);
                await _redisService.RemoveAsync(kv.Key);
            }
        }

        [Fact]
        public async Task AddBatchAsync_WithExpiry_SetsExpirationOnAllKeys()
        {
            var prefix = nameof(AddBatchAsync_WithExpiry_SetsExpirationOnAllKeys);
            var keyValues = new Dictionary<string, string>
            {
                { $"{prefix}_1", "value1" },
                { $"{prefix}_2", "value2" }
            };
            var expiry = TimeSpan.FromSeconds(2);

            var result = await _redisService.AddBatchAsync(keyValues, expiry);
            Assert.True(result);

            await Task.Delay(3000);

            foreach (var kv in keyValues)
            {
                var value = await _redisService.GetAsync<string>(kv.Key);
                Assert.Null(value);
            }
        }

        [Fact]
        public async Task AddBatchAsync_WithComplexObjects_AddsCorrectly()
        {
            var prefix = nameof(AddBatchAsync_WithComplexObjects_AddsCorrectly);
            var models1 = new TestModelFaker().GenerateBetween(1, 5);
            var models2 = new TestModelFaker().GenerateBetween(1, 5);
            var keyValues = new Dictionary<string, List<TestModel>>
            {
                { $"{prefix}_1", models1 },
                { $"{prefix}_2", models2 }
            };

            var result = await _redisService.AddBatchAsync(keyValues);
            Assert.True(result);

            var value1 = await _redisService.GetAsync<List<TestModel>>($"{prefix}_1");
            var value2 = await _redisService.GetAsync<List<TestModel>>($"{prefix}_2");

            Assert.Equal(models1, value1, new TestModelComparer());
            Assert.Equal(models2, value2, new TestModelComparer());

            await _redisService.RemoveAsync($"{prefix}_1");
            await _redisService.RemoveAsync($"{prefix}_2");
        }

        #endregion AddBatchAsync

        #region GetTimeToLiveAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task GetTimeToLiveAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.GetTimeToLiveAsync(key));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task GetTimeToLiveAsync_KeyDoesNotExist_ReturnsNull()
        {
            var key = nameof(GetTimeToLiveAsync_KeyDoesNotExist_ReturnsNull);
            var ttl = await _redisService.GetTimeToLiveAsync(key);
            Assert.Null(ttl);
        }

        [Fact]
        public async Task GetTimeToLiveAsync_KeyWithoutExpiry_ReturnsNull()
        {
            var key = nameof(GetTimeToLiveAsync_KeyWithoutExpiry_ReturnsNull);
            await _redisService.AddAsync(key, "value");
            
            var ttl = await _redisService.GetTimeToLiveAsync(key);
            
            Assert.Null(ttl);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetTimeToLiveAsync_KeyWithExpiry_ReturnsRemainingTime()
        {
            var key = nameof(GetTimeToLiveAsync_KeyWithExpiry_ReturnsRemainingTime);
            var expiry = TimeSpan.FromSeconds(10);
            await _redisService.AddAsync(key, "value", expiry);

            var ttl = await _redisService.GetTimeToLiveAsync(key);

            Assert.NotNull(ttl);
            Assert.True(ttl.Value.TotalSeconds > 0);
            Assert.True(ttl.Value.TotalSeconds <= expiry.TotalSeconds);
            await _redisService.RemoveAsync(key);
        }

        #endregion GetTimeToLiveAsync

        #region PersistAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task PersistAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.PersistAsync(key));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task PersistAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var key = nameof(PersistAsync_KeyDoesNotExist_ReturnsFalse);
            var result = await _redisService.PersistAsync(key);
            Assert.False(result);
        }

        [Fact]
        public async Task PersistAsync_KeyWithExpiry_RemovesExpiration()
        {
            var key = nameof(PersistAsync_KeyWithExpiry_RemovesExpiration);
            await _redisService.AddAsync(key, "value", TimeSpan.FromSeconds(5));

            var result = await _redisService.PersistAsync(key);
            Assert.True(result);

            var ttl = await _redisService.GetTimeToLiveAsync(key);
            Assert.Null(ttl);

            await Task.Delay(6000);
            var value = await _redisService.GetAsync<string>(key);
            Assert.NotNull(value);

            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task PersistAsync_KeyWithoutExpiry_ReturnsFalse()
        {
            var key = nameof(PersistAsync_KeyWithoutExpiry_ReturnsFalse);
            await _redisService.AddAsync(key, "value");

            var result = await _redisService.PersistAsync(key);
            Assert.False(result);

            await _redisService.RemoveAsync(key);
        }

        #endregion PersistAsync

        #region PingAsync

        [Fact]
        public async Task PingAsync_ReturnsResponseTime()
        {
            var responseTime = await _redisService.PingAsync();
            Assert.True(responseTime > TimeSpan.Zero);
            Assert.True(responseTime < TimeSpan.FromSeconds(1));
        }

        #endregion PingAsync
    }
}
#endif