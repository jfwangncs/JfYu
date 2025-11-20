#if NET8_0_OR_GREATER
using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static JfYu.UnitTests.Redis.RedisBaseTests;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class RedisSetTests
    {
        private readonly IRedisService _redisService;

        public RedisSetTests()
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

        #region SetAdd

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task SetAddAsync_WhenKeyOrValueIsNull_ShouldThrowArgumentException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetAddAsync(key, value).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetAddAsync_ValueNotExist_ReturnTrue()
        {
            string key = nameof(SetAddAsync_ValueNotExist_ReturnTrue);
            var value = "v1";
            bool result = await _redisService.SetAddAsync(key, value);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SetAddAsync_ValueExist_ReturnFalse()
        {
            string key = nameof(SetAddAsync_ValueExist_ReturnFalse);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            bool result = await _redisService.SetAddAsync(key, value);
            Assert.False(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetAdd

        #region SetAddAll

        [Theory]
        [ClassData(typeof(NullKeyAndValuesExpectData))]
        public async Task SetAddAllAsync_WhenKeyOrValueIsNull_ShouldThrowArgumentException(string key, string?[] values)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetAddAllAsync(key, values.ToList()).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetAddAllAsync_ValuesNotExist_ReturnCorrectLength()
        {
            string key = nameof(SetAddAllAsync_ValuesNotExist_ReturnCorrectLength);
            List<string> values = ["v1", "v2", "v3"];
            var result = await _redisService.SetAddAllAsync(key, values);
            Assert.Equal(values.Count, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SetAddAllAsync_ValuesExistPartially_ReturnCorrectLength()
        {
            string key = nameof(SetAddAllAsync_ValuesExistPartially_ReturnCorrectLength);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            List<string> values = ["v1", "v2", "v3"];
            var result = await _redisService.SetAddAllAsync(key, values);
            Assert.Equal(2, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetAddAll

        #region SetRemoveAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task SetRemoveAsync_WhenKeyOrValueIsNull_ShouldThrowArgumentException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetRemoveAsync(key, value).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetRemoveAsync_ValuesNotExist_ReturnFalse()
        {
            string key = nameof(SetRemoveAsync_ValuesNotExist_ReturnFalse);
            var value = "v1";
            var result = await _redisService.SetRemoveAsync(key, value);
            Assert.False(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SetRemoveAsync_ValuesExistPartially_ReturnTrue()
        {
            string key = nameof(SetRemoveAsync_ValuesExistPartially_ReturnTrue);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            var result = await _redisService.SetRemoveAsync(key, value);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetRemoveAsync

        #region SetRemoveAllAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValuesExpectData))]
        public async Task SetRemoveAllAsync_WhenKeyOrValueIsNull_ShouldThrowArgumentException(string key, string?[] values)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetRemoveAllAsync(key, values.ToList()).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetRemoveAllAsync_ValuesNotExist_ReturnCorrectLength()
        {
            string key = nameof(SetRemoveAllAsync_ValuesNotExist_ReturnCorrectLength);
            List<string> values = ["v1", "v2", "v3"];
            var result = await _redisService.SetRemoveAllAsync(key, values);
            Assert.Equal(0, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SetRemoveAllAsync_ValuesExist_ReturnCorrectLength()
        {
            string key = nameof(SetRemoveAllAsync_ValuesExist_ReturnCorrectLength);
            List<string> values = ["v1", "v2", "v3"];
            await _redisService.SetAddAllAsync(key, values);
            var result = await _redisService.SetRemoveAllAsync(key, values);
            Assert.Equal(values.Count, result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SetRemoveAllAsync_ValuesExistPartially_ReturnCorrectLength()
        {
            string key = nameof(SetRemoveAllAsync_ValuesExistPartially_ReturnCorrectLength);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            List<string> values = ["v1", "v2", "v3"];
            var result = await _redisService.SetRemoveAllAsync(key, values);
            Assert.Equal(1, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetRemoveAllAsync

        #region SetContainsAsync

        [Theory]
        [ClassData(typeof(NullKeyAndValueExpectData))]
        public async Task SetContainsAsync_WhenKeyOrValueIsNull_ShouldThrowArgumentException(string key, string value)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetContainsAsync(key, value).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetContainsAsync_KeyNotExist_ReturnFalse()
        {
            string key = nameof(SetContainsAsync_KeyNotExist_ReturnFalse);
            var value = "v1";
            bool result = await _redisService.SetContainsAsync(key, value);
            Assert.False(result);
        }

        [Fact]
        public async Task SetContainsAsync_ValueNotExist_ReturnFalse()
        {
            string key = nameof(SetContainsAsync_ValueNotExist_ReturnFalse);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            value = "v2";
            bool result = await _redisService.SetContainsAsync(key, value);
            Assert.False(result);
        }

        [Fact]
        public async Task SetContainsAsync_ValueExist_ReturnTrue()
        {
            string key = nameof(SetContainsAsync_ValueExist_ReturnTrue);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            var result = await _redisService.SetContainsAsync(key, value);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        [Fact]
        public async Task SetContainsAsync_complexValueExist_ReturnTrue()
        {
            string key = nameof(SetContainsAsync_complexValueExist_ReturnTrue);
            var value = new TestModelFaker().Generate();
            await _redisService.SetAddAsync(key, value);
            var result = await _redisService.SetContainsAsync(key, value);
            Assert.True(result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetContainsAsync

        #region SetMembersAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SetMembersAsync_WhenKeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetMembersAsync(key).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetMembersAsync_KeyNotExist_ReturnCorrectly()
        {
            string key = nameof(SetMembersAsync_KeyNotExist_ReturnCorrectly);
            await _redisService.RemoveAsync(key);
            var result = await _redisService.SetMembersAsync(key);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SetMembersAsync_ValueNotExist_ReturnCorrectly()
        {
            string key = nameof(SetMembersAsync_ValueNotExist_ReturnCorrectly);   
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            await _redisService.SetRemoveAsync(key, value);
            var result = await _redisService.SetMembersAsync(key);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SetMembersAsync_ReturnCorrectly()
        {
            string key = nameof(SetMembersAsync_ReturnCorrectly);
            var value = new TestModelFaker().Generate();
            List<object> values = ["v1", 1, "v3", 423.442, value, new TestModelFaker().Generate(2), "", new TestModelSubFaker().Generate()];
            await _redisService.SetAddAllAsync(key, values);
            var result = await _redisService.SetMembersAsync(key);
            Assert.Equal(values.Count, result.Count);
            await _redisService.SetRemoveAsync(key, value);
            result = await _redisService.SetMembersAsync(key);
            Assert.Equal(values.Count - 1, result.Count);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetMembersAsync

        #region SetLengthAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SetLengthAsync_WhenKeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetLengthAsync(key).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetLengthAsync_KeyNotExist_ReturnCorrectly()
        {
            string key = nameof(SetLengthAsync_KeyNotExist_ReturnCorrectly);
            var result = await _redisService.SetLengthAsync(key);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task SetLengthAsync_ValueNotExist_ReturnCorrectly()
        {
            string key = nameof(SetLengthAsync_ValueNotExist_ReturnCorrectly);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            await _redisService.SetRemoveAsync(key, value);
            var result = await _redisService.SetLengthAsync(key);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task SetLengthAsync_ReturnCorrectly()
        {
            string key = nameof(SetLengthAsync_ReturnCorrectly);
            List<object> values = ["v1", 1, "v3", 423.442, new TestModelFaker().Generate(), new TestModelFaker().Generate(2), "", new TestModelSubFaker().Generate()];
            await _redisService.SetAddAllAsync(key, values);
            var result = await _redisService.SetLengthAsync(key);
            Assert.Equal(values.Count, result);
            await _redisService.RemoveAsync(key);
        }

        #endregion SetLengthAsync

        #region SetRandomMemberAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SetRandomMemberAsync_WhenKeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetRandomMemberAsync(key).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetRandomMemberAsync_KeyNotExist_ReturnEmpty()
        {
            string key = nameof(SetRandomMemberAsync_KeyNotExist_ReturnEmpty);
            var result = await _redisService.SetRandomMemberAsync(key);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task SetRandomMemberAsync_ValueNotExist_ReturnEmpty()
        {
            string key = nameof(SetRandomMemberAsync_ValueNotExist_ReturnEmpty);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            await _redisService.SetRemoveAsync(key, value);
            var result = await _redisService.SetRandomMemberAsync(key);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task SetRandomMemberAsync_WithOneStringValue_ReturnOne()
        {
            string key = nameof(SetRandomMemberAsync_WithOneStringValue_ReturnOne);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            var result = await _redisService.SetRandomMemberAsync(key);
            Assert.True(result.HasValue);
            Assert.Equal(value, _redisService.Serializer.Deserialize<string>(result!));
        }

        [Fact]
        public async Task SetRandomMemberAsync_WithOneModelValue_ReturnOne()
        {
            string key = nameof(SetRandomMemberAsync_WithOneModelValue_ReturnOne);
            var value = new TestModelFaker().Generate(10);
            await _redisService.SetAddAsync(key, value);
            var result = await _redisService.SetRandomMemberAsync(key);
            Assert.True(result.HasValue);
            Assert.Equal(value, _redisService.Serializer.Deserialize<List<TestModel>>(result!));
        }

        [Fact]
        public async Task SetRandomMemberAsync_WithMoreValues_ReturnOne()
        {
            string key = nameof(SetRandomMemberAsync_WithMoreValues_ReturnOne);
            List<string> values = ["v1", "v2", "v3", "v4"];
            await _redisService.SetAddAllAsync(key, values);
            var result = await _redisService.SetRandomMemberAsync(key);
            Assert.True(result.HasValue);
            Assert.Contains(_redisService.Serializer.Deserialize<string>(result!), values);
        }

        #endregion SetRandomMemberAsync

        #region SetRandomMembersAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task SetRandomMembersAsync_WhenKeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.SetRandomMembersAsync(key, 1).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex, false);
        }

        [Fact]
        public async Task SetRandomMembersAsync_KeyNotExist_ReturnEmpty()
        {
            string key = nameof(SetRandomMembersAsync_KeyNotExist_ReturnEmpty);
            var result = await _redisService.SetRandomMembersAsync(key, 1);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SetRandomMembersAsync_ValueNotExist_ReturnEmpty()
        {
            string key = nameof(SetRandomMembersAsync_ValueNotExist_ReturnEmpty);
            var value = "v1";
            await _redisService.SetAddAsync(key, value);
            await _redisService.SetRemoveAsync(key, value);
            var result = await _redisService.SetRandomMembersAsync(key, 1);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SetRandomMembersAsync_ReturnCorrectly()
        {
            string key = nameof(SetRandomMembersAsync_ReturnCorrectly);
            List<string> values = ["v1", "v2", "v3", "v4"];
            await _redisService.SetAddAllAsync(key, values);
            var result = await _redisService.SetRandomMembersAsync(key, 3);
            Assert.Equal(3, result.Count);
        }

        #endregion SetRandomMembersAsync
    }
}
#endif