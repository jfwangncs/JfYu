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
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task ExpireAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var result = await _redisService.ExpireAsync("nonexistent_key", TimeSpan.FromSeconds(3));
            Assert.False(result);
        }

        [Fact]
        public async Task ExpireAsync_KeyExists_ReturnsTrue()
        {
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.ExpireAsync("existent_key", TimeSpan.FromSeconds(3));
            Assert.True(result);
            var value = await _redisService.GetAsync<string>("existent_key");
            Assert.Equal("value", value);
            await Task.Delay(4000);
            value = await _redisService.GetAsync<string>("existent_key");
            Assert.Null(value);
        }

        [Fact]
        public async Task ExpireAsync_WithTimeSpanZero_ReturnsTrue()
        {
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.ExpireAsync("existent_key", TimeSpan.Zero);
            Assert.True(result);
            var value = await _redisService.GetAsync<string>("existent_key");
            Assert.Null(value);
        }

        #endregion ExpireAsync

        #region ExistsAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task ExistsAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.ExistsAsync(key));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task ExistsAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var result = await _redisService.ExistsAsync("nonexistent_key");
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_KeyExists_ReturnsTrue()
        {
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.ExistsAsync("existent_key");
            Assert.True(result);
            await _redisService.RemoveAsync("existent_key");
        }

        #endregion ExistsAsync

        #region RemoveAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task RemoveAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.RemoveAsync(key));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task RemoveAsync_KeyDoesNotExist_ReturnsFalse()
        {
            var result = await _redisService.RemoveAsync("nonexistent_key");
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_KeyExists_ReturnsTrue()
        {
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.RemoveAsync("existent_key");
            Assert.True(result);
        }

        #endregion RemoveAsync

        #region RemoveAllAsync

        [Theory]
        [ClassData(typeof(NullKeysExpectData))]
        public async Task RemoveAllAsync_KeyIsNull_ThrowsException(string[] keys)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _redisService.RemoveAllAsync([.. keys]));
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
            Assert.IsType<ArgumentException>(ex);
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
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.GetAsync<string>("existent_key");
            Assert.Equal("value", result);
            await _redisService.RemoveAsync("existent_key");
        }

        [Fact]
        public async Task GetAsync_ValueInt_ReturnsValue()
        {
            var key = "key1";
            var value = 1;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<int>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueLong_ReturnsValue()
        {
            var key = "key1";
            long value = 121132321321;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<long>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueDouble_ReturnsValue()
        {
            var key = "key1";
            double value = 121132321321.3123131414;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<double>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueDecimal_ReturnsValue()
        {
            var key = "key1";
            decimal value = 12113232132131.3114142131M;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<decimal>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueDateTime_ReturnsValue()
        {
            var key = "key1";
            DateTime value = DateTime.UtcNow;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.GetAsync<DateTime>(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task GetAsync_ValueModel_ReturnsValue()
        {
            var key = "key1";
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
            Assert.IsType<ArgumentException>(ex);
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
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.GetAsync<string>("existent_key", TimeSpan.FromSeconds(3));
            Assert.Equal("value", result);
            await Task.Delay(5000);
            result = await _redisService.GetAsync<string>("existent_key");
            Assert.Null(result);
            await _redisService.RemoveAsync("existent_key");
        }

        [Fact]
        public async Task GetAsync_WithExpiry_KeyExist_ReturnsValue()
        {
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.GetAsync<string>("existent_key", TimeSpan.FromSeconds(5));
            Assert.Equal("value", result);
            await Task.Delay(3000);
            result = await _redisService.GetAsync<string>("existent_key");
            Assert.Equal("value", result);
            await Task.Delay(3000);
            result = await _redisService.GetAsync<string>("existent_key");
            Assert.Null(result);
            await _redisService.RemoveAsync("existent_key");
        }

        [Fact]
        public async Task GetAsync_WithExpiry_KeyExists_ReturnsValueAndUpdatesExpiry()
        {
            var expiry = TimeSpan.FromSeconds(10);
            await _redisService.AddAsync("key_with_expiry", "value", expiry);
            var result = await _redisService.GetAsync<string>("key_with_expiry", expiry);
            Assert.Equal("value", result);
            await _redisService.RemoveAsync("key_with_expiry");
        }

        #endregion GetAsync

        #region AddAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task AddAsync_KeyIsNull_ThrowsException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.AddAsync(key, value));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task AddAsync_KeyAlreadyExists_ReturnsNewValue()
        {
            await _redisService.AddAsync("existent_key", "value");
            var result = await _redisService.AddAsync("existent_key", "new_value");
            Assert.True(result);
            var value = await _redisService.GetAsync<string>("existent_key");
            Assert.Equal("new_value", value);
            await _redisService.RemoveAsync("existent_key");
        }

        [Fact]
        public async Task AddAsync_KeyDoesNotExist_ReturnsTrue()
        {
            var result = await _redisService.AddAsync("nonexistent_key", "value");
            Assert.True(result);
            await _redisService.RemoveAsync("nonexistent_key");
        }

        [Fact]
        public async Task AddAsync_WithExpiry_KeyDoesNotExist_ReturnsTrue()
        {
            var expiry = TimeSpan.FromSeconds(3);
            var result = await _redisService.AddAsync("key_with_expiry", "value", expiry);
            Assert.True(result);
            await Task.Delay(5000);
            var value = await _redisService.GetAsync<string>("key_with_expiry");
            Assert.Null(value);
            await _redisService.RemoveAsync("key_with_expiry");
        }

        [Fact]
        public async Task AddAsync_WithExpiry_KeyExist_ReturnsNewValue()
        {
            await _redisService.AddAsync("key_with_expiry", "value");
            var expiry = TimeSpan.FromSeconds(10);
            var result = await _redisService.AddAsync("key_with_expiry", "new_value", expiry);
            Assert.True(result);
            var value = await _redisService.GetAsync<string>("key_with_expiry");
            Assert.Equal("new_value", value);
            await _redisService.RemoveAsync("key_with_expiry");
        }

        [Fact]
        public async Task AddAsync_WithCondition_KeyDoesNotExist_ReturnsTrue()
        {
            var result = await _redisService.AddAsync("conditional_key", "value", When.NotExists);
            Assert.True(result);
            await _redisService.RemoveAsync("conditional_key");
        }

        [Fact]
        public async Task AddAsync_WithCondition_KeyExists_ReturnsFalse()
        {
            await _redisService.AddAsync("conditional_key", "value");
            var result = await _redisService.AddAsync("conditional_key", "new_value", When.NotExists);
            Assert.False(result);
            await _redisService.RemoveAsync("conditional_key");
        }

        #endregion AddAsync

        #region IncrementAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task IncrementAsync_KeyIsNull_ThrowsException(string key)
        {
            var ex = await Record.ExceptionAsync(() => _redisService.IncrementAsync(key));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task IncrementAsync_KeyExistValueNotNumeric_ThrowsException()
        {
            var key = "key";
            var value = "dada";
            await _redisService.AddAsync(key, value);
            await Assert.ThrowsAsync<RedisServerException>(async () => await _redisService.IncrementAsync(key).ConfigureAwait(true));
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_KeyNotExist_ReturnCorrectly()
        {
            var key = "key";
            var value = 1;
            var result = await _redisService.IncrementAsync(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_KeyExist_ReturnCorrectly()
        {
            var key = "key";
            var value = 11;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.IncrementAsync(key);
            Assert.Equal(value + 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_WithSpecificValue_ThrowsException()
        {
            var key = "key";
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
            var key = "key";
            var value = 11.24;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.IncrementAsync(key, 1D);
            Assert.Equal(value + 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task IncrementAsync_DoubleWithSpecificValue_ThrowsException()
        {
            var key = "key";
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
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task DecrementAsync_KeyExistValueNotNumeric_ThrowsException()
        {
            var key = "key";
            var value = "dada";
            await _redisService.AddAsync(key, value);
            await Assert.ThrowsAsync<RedisServerException>(async () => await _redisService.DecrementAsync(key).ConfigureAwait(true));
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_KeyNotExist_ReturnCorrectly()
        {
            var key = "key";
            var value = -1;
            var result = await _redisService.DecrementAsync(key);
            Assert.Equal(value, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_KeyExist_ReturnCorrectly()
        {
            var key = "key";
            var value = 11;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key);
            Assert.Equal(value - 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_WithSpecificValue_ThrowsException()
        {
            var key = "key";
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
            var key = "key";
            var value = 11.24;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key, 1D);
            Assert.Equal(value - 1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task DecrementAsync_DoubleWithSpecificValue_ThrowsException()
        {
            var key = "key";
            var value = 11.24;
            var incrementValue = 5.37;
            await _redisService.AddAsync(key, value);
            var result = await _redisService.DecrementAsync(key, incrementValue);
            Assert.Equal(value - incrementValue, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion DecrementAsync
    }
}
#endif