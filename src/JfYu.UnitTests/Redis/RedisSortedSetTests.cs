#if NET8_0_OR_GREATER
using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using static JfYu.UnitTests.Redis.RedisBaseTests;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class RedisSortedSetTests
    {
        private readonly IRedisService _redisService;

        public RedisSortedSetTests()
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

        #region SortedSetAddAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task SortedSetAddAsync_WhenkeyOrValueIsNull_ShouldThrowArgumentException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetAddAsync(key, value, 1d).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetAddAsync_KeyNotExist_ReturnTrue()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 1d;
            var result = await _redisService.SortedSetAddAsync(key, value, score);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetAddAsync_KeyExist_ReturnFalse()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 5.32d;
            await _redisService.SortedSetAddAsync(key, value, 1d);
            var result = await _redisService.SortedSetAddAsync(key, value, score);
            Assert.False(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetAddAsync

        #region SortedSetAddAllAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SortedSetAddAllAsync_WhenkeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetAddAsync(key, "", 1d).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetAddAllAsync_WhenValuesIsNull_ShouldThrowArgumentException()
        {
            var key = "testSortedSet";
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetAddAllAsync<string>(key, null!).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetAddAllAsync_WhenValuesIsEmpty_ShouldThrowArgumentException()
        {
            var key = "testSortedSet";
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetAddAllAsync<string>(key, []).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetAddAllAsync_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"test", 12.21d},
                {"dadwa", 2d}
            };
            var result = await _redisService.SortedSetAddAllAsync(key, values);
            Assert.Equal(values.Count, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetAddAllAsync

        #region SortedSetRemoveAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValuesExpectData))]
        public async Task SortedSetRemoveAsync_WhenkeyIsNullOrValuesIsNull_ShouldThrowArgumentException(string key, string?[] values)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetRemoveAsync(key, values.ToList()).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetRemoveAsync_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"test", 12.21d},
                {"1da", 1d},
                {"dwad1", 3d},
                {"da2", 4d},
                {"dwa3", 5d}
            };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetRemoveAsync(key, values.Keys.ToList());
            Assert.Equal(result, values.Count);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetRemoveAsync_PartiallyValues_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"test", 12.21d},
                {"1da", 1d},
                {"dwad1", 3d},
                {"da2", 4d},
                {"dwa3", 5d}
            };
            var keys = new List<string> { "1da", "test", "213131", "313123dsad" };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetRemoveAsync(key, keys);
            Assert.Equal(2, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetRemoveAsync

        #region SortedSetIncrementScoreAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task SortedSetIncrementScoreAsync_WhenkeyIsNullOrValueIsNull_ShouldThrowArgumentException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetIncrementScoreAsync(key, value, 1d).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetIncrementScoreAsync_KeyNotExist_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 23.213d;
            var result = await _redisService.SortedSetIncrementScoreAsync(key, value, score);
            Assert.Equal(score, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetIncrementScoreAsync_KeyExist_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 23.213d;
            await _redisService.SortedSetAddAsync(key, value, score);
            var result = await _redisService.SortedSetIncrementScoreAsync(key, value, score);
            Assert.Equal(score * 2, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetIncrementScoreAsync

        #region SortedSetRankAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task SortedSetRankAsync_WhenkeyIsNullOrValueIsNull_ShouldThrowArgumentException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetRankAsync(key, value).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetRankAsync_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"test", 12.21d},
                {"1da", 1d},
                {"dwad1", 3d},
                {"da2", 4d},
                {"dwa3", 5d}
            };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetRankAsync(key, "dwa3");
            Assert.Equal(3, result);
            result = await _redisService.SortedSetRankAsync(key, "dwa3", StackExchange.Redis.Order.Descending);
            Assert.Equal(1, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetRankAsync_ValueNotExist_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"test", 12.21d},
                {"1da", 1d},
                {"dwad1", 3d},
                {"da2", 4d},
                {"dwa3", 5d}
            };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetRankAsync(key, "111");
            Assert.Null(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetRankAsync

        #region SortedSetRangeByRankAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SortedSetRangeByRankAsync_WhenkeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetRangeByRankAsync(key, 0, 1).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetRangeByRankAsync_KeyOrValuesNotExist_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 23.213d;
            var result = await _redisService.SortedSetRangeByRankAsync(key, 0, 10);
            Assert.Empty(result);
            await _redisService.SortedSetAddAsync(key, value, score);
            await _redisService.SortedSetRemoveAsync(key, [value]);
            result = await _redisService.SortedSetRangeByRankAsync(key, 0, 10);
            Assert.Empty(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetRangeByRankAsync_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"a", 0.8d},
                {"b", 1.10d},
                {"c", 1.12d},
                {"d", 2.3d},
                {"e", 9.9d}
            };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetRangeByRankAsync(key, 0, 1);
            Assert.Equal("a", _redisService.Serializer.Deserialize<string>(result[0]!));
            result = await _redisService.SortedSetRangeByRankAsync(key, 0, 1, StackExchange.Redis.Order.Descending);
            Assert.Equal("e", _redisService.Serializer.Deserialize<string>(result[0]!));
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetRangeByRankAsync

        #region SortedSetRangeByScoreAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SortedSetRangeByScoreAsync_WhenkeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetRangeByScoreAsync(key, 1, 2).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetRangeByScoreAsync_KeyOrValuesNotExist_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 23.213d;
            var result = await _redisService.SortedSetRangeByScoreAsync(key, 1, 2);
            Assert.Empty(result);
            await _redisService.SortedSetAddAsync(key, value, score);
            await _redisService.SortedSetRemoveAsync(key, [value]);
            result = await _redisService.SortedSetRangeByScoreAsync(key, 1, 2);
            Assert.Empty(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetRangeByScoreAsync_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"a", 0.8d},
                {"b", 1.10d},
                {"c", 1.12d},
                {"d", 2.3d},
                {"e", 9.9d}
            };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetRangeByScoreAsync(key, 1.1d, 1.3d);
            Assert.Equal("b", _redisService.Serializer.Deserialize<string>(result[0]!));
            Assert.Equal("c", _redisService.Serializer.Deserialize<string>(result[^1]!));
            result = await _redisService.SortedSetRangeByScoreAsync(key, 1.1d, 1.3d, StackExchange.Redis.Exclude.None, StackExchange.Redis.Order.Descending);
            Assert.Equal("c", _redisService.Serializer.Deserialize<string>(result[0]!));
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetRangeByScoreAsync

        #region SortedSetCountAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SortedSetCountAsync_WhenkeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SortedSetCountAsync(key, 1, 2).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task SortedSetCountAsync_KeyOrValuesNotExist_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var value = "testValue";
            var score = 23.213d;
            var result = await _redisService.SortedSetCountAsync(key, 1, 2);
            Assert.Equal(0, result);
            await _redisService.SortedSetAddAsync(key, value, score);
            await _redisService.SortedSetRemoveAsync(key, [value]);
            result = await _redisService.SortedSetCountAsync(key, 1, 2);
            Assert.Equal(0, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SortedSetCountAsync_ReturnCorrectly()
        {
            var key = "testSortedSet";
            var values = new Dictionary<string, double>
            {
                {"a", 0.8d},
                {"b", 1.10d},
                {"c", 1.12d},
                {"d", 2.3d},
                {"e", 9.9d}
            };
            await _redisService.SortedSetAddAllAsync(key, values);
            var result = await _redisService.SortedSetCountAsync(key, 1.1d, 1.3d);
            Assert.Equal(2, result);
            result = await _redisService.SortedSetCountAsync(key, 1.1d, 1.3d, StackExchange.Redis.Exclude.Both);
            Assert.Equal(1, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SortedSetCountAsync
    }
}
#endif