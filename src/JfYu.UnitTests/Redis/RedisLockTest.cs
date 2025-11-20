#if NET8_0_OR_GREATER
using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static JfYu.UnitTests.Redis.RedisBaseTests;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class RedisLockTests
    {
        private readonly IRedisService _redisService;

        public RedisLockTests()
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

        #region LockTakeAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task LockTakeAsync_WhenkeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.LockTakeAsync(key).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task LockTakeAsync_WhenKeyNotExist_ReturnsTrue()
        {
            string key = "testLock";
            bool result = await _redisService.LockTakeAsync(key);
            Assert.True(result);
            await _redisService.LockReleaseAsync(key);
        }

        [Fact]
        public async Task LockTakeAsync_WhenKeyExist_ReturnsFalse()
        {
            string key = "testLock";
            await _redisService.LockTakeAsync(key);
            bool result = await _redisService.LockTakeAsync(key);
            Assert.False(result);
            await _redisService.LockReleaseAsync(key);
        }

        [Fact]
        public async Task LockTakeAsync_WhenKeyExpires_ReturnsTrue()
        {
            string key = "testLock";
            await _redisService.LockTakeAsync(key, TimeSpan.FromSeconds(3));
            bool result = await _redisService.LockTakeAsync(key);
            Assert.False(result);
            await Task.Delay(4000);
            result = await _redisService.LockTakeAsync(key);
            Assert.True(result);
            await _redisService.LockReleaseAsync(key);
        }

        #endregion LockTakeAsync

        #region LockReleaseAsync

        [Theory]
        [ClassData(typeof(NullKeyExpectData))]
        public async Task LockReleaseAsync_WhenkeyIsNull_ShouldThrowArgumentException(string key)
        {
            var ex = await Record.ExceptionAsync(async () => await _redisService.LockReleaseAsync(key).ConfigureAwait(true));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public async Task LockReleaseAsync_WhenKeyNotExist_ReturnsFalse()
        {
            string key = "testLock";
            bool result = await _redisService.LockReleaseAsync(key);
            Assert.False(result);
        }

        [Fact]
        public async Task LockReleaseAsync_WhenKeyExist_ReturnsTrue()
        {
            string key = "testLock";
            await _redisService.LockTakeAsync(key);
            bool result = await _redisService.LockReleaseAsync(key);
            Assert.True(result);
        }

        #endregion LockReleaseAsync

        #region HighConcurrency

        [Fact]
        public async Task HighConcurrency_LockCorrectly()
        {
            const string LockKey = "test:lock";
            const int NumberOfTasks = 100;
            const int LockTimeoutInSeconds = 5;
            const int IncrementCount = 1000;
            // Arrange 
            int count = 0; 
            var tasks = Enumerable.Range(0, NumberOfTasks).Select(i => Task.Run(async () =>
            {
                var locked = false;
                do
                {
                    locked = await _redisService.LockTakeAsync(LockKey, TimeSpan.FromSeconds(LockTimeoutInSeconds)).ConfigureAwait(true);
                    if (locked)
                    {
                        for (int i = 0; i < IncrementCount; i++)
                        {
                            count++;
                        }
                        await _redisService.RemoveAsync(LockKey).ConfigureAwait(true);
                    }
                } while (!locked);
            })).ToList();

            await Task.WhenAll(tasks);
            // Assert
            Assert.True(count == NumberOfTasks * IncrementCount);
        }

        [Fact]
        public async Task HighConcurrency_ReleaseTwice_LockFaild()
        {
            const string LockKey = "test:lock";
            const int NumberOfTasks = 100;
            const int LockTimeoutInSeconds = 5;
            const int IncrementCount = 1000;
            // Arrange 
            int count = 0; 
            var tasks = Enumerable.Range(0, NumberOfTasks).Select(i => Task.Run(async () =>
            {
                var locked = false;
                do
                {
                    locked = await _redisService.LockTakeAsync(LockKey, TimeSpan.FromSeconds(LockTimeoutInSeconds)).ConfigureAwait(true);
                    if (locked)
                    {
                        try
                        {
                            await Task.Delay(500).ConfigureAwait(true);

                            for (int i = 0; i < IncrementCount; i++)
                            {
                                count++;
                            }

                            await Task.Delay(500).ConfigureAwait(true);
                        }
                        finally
                        {
                            await _redisService.RemoveAsync(LockKey).ConfigureAwait(true);
                        }
                    }
                } while (!locked);
            })).ToList();

            await Task.WhenAll(tasks);
            // Assert
            Assert.True(count <= NumberOfTasks * IncrementCount);
        }

        #endregion HighConcurrency
    }
}
#endif