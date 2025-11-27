#if NET8_0_OR_GREATER
using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class RedisPubSubTests
    {
        private readonly IRedisService _redisService;

        public RedisPubSubTests()
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
                options.UsingNewtonsoft(settings =>
                {
                    settings.MaxDepth = 12;
                });
            }).BuildServiceProvider();

            _redisService = serviceProvider.GetRequiredService<IRedisService>();
        }

        [Fact]
        public async Task PublishAsync_ShouldPublishMessage()
        {
            // Arrange
            var channel = $"test-channel-{Guid.NewGuid()}";
            var message = "Hello, Redis Pub/Sub!";

            // Act
            var subscribers = await _redisService.PublishAsync(channel, message);

            // Assert - 没有订阅者，返回0
            Assert.True(subscribers >= 0);
        }

        [Fact]
        public async Task PublishAsync_WithNullChannel_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.PublishAsync<string>(null!, "message"));
        }

        [Fact]
        public async Task PublishAsync_WithEmptyChannel_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _redisService.PublishAsync("", "message"));
        }

        [Fact]
        public async Task PublishAsync_WithNullMessage_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.PublishAsync<string>("channel", null!));
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReceiveMessage()
        {
            // Arrange
            var channel = $"test-subscribe-{Guid.NewGuid()}";
            var expectedMessage = "Test Message";
            var receivedMessage = "";
            using var messageReceived = new ManualResetEventSlim(false);

            // Act
            await _redisService.SubscribeAsync<string>(channel, (ch, msg) =>
            {
                receivedMessage = msg;
                messageReceived.Set();
            });

            // 等待订阅生效
            await Task.Delay(100);

            var subscribers = await _redisService.PublishAsync(channel, expectedMessage);

            // Wait for message
            var received = messageReceived.Wait(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(received, "Message was not received within timeout");
            Assert.Equal(expectedMessage, receivedMessage);
            Assert.True(subscribers > 0);

            // Cleanup
            await _redisService.UnsubscribeAsync(channel);
        }

        [Fact]
        public async Task SubscribeAsync_WithNullString_ShouldReceiveNull()
        {
            // Arrange
            var channel = $"test-null-{Guid.NewGuid()}";
            string? receivedMessage = "not-set";
            using var messageReceived = new ManualResetEventSlim(false);

            // Act
            await _redisService.SubscribeAsync<string>(channel, (ch, msg) =>
            {
                receivedMessage = msg;
                messageReceived.Set();
            });

            await Task.Delay(100);


            var subscriber = _redisService.Client.GetSubscriber();
            var serializedMessage = JsonConvert.SerializeObject(null);
            await subscriber.PublishAsync(RedisChannel.Literal(channel), serializedMessage).ConfigureAwait(true);

            var received = messageReceived.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.True(received, "Message was not received within timeout");
            Assert.Null(receivedMessage);

            // Cleanup
            await _redisService.UnsubscribeAsync(channel);
        } 

        [Fact]
        public async Task SubscribeAsync_WithEmptyString_ShouldReceiveEmpty()
        {
            // Arrange
            var channel = $"test-empty-{Guid.NewGuid()}";
            string? receivedMessage = null;
            using var messageReceived = new ManualResetEventSlim(false);

            // Act
            await _redisService.SubscribeAsync<string>(channel, (ch, msg) =>
            {
                receivedMessage = msg;
                messageReceived.Set();
            });

            await Task.Delay(100);

            // 发布空字符串
            await _redisService.PublishAsync(channel, string.Empty);

            var received = messageReceived.Wait(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(received, "Message was not received within timeout");
            Assert.NotNull(receivedMessage);
            Assert.Equal(string.Empty, receivedMessage);

            // Cleanup
            await _redisService.UnsubscribeAsync(channel);
        }

        [Fact]
        public async Task SubscribePatternAsync_WithNullString_ShouldReceiveNull()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");
            var pattern = $"test.null.pattern.{guid}.*";
            var channel = $"test.null.pattern.{guid}.channel";
            string? receivedMessage = "not-set";
            using var messageReceived = new ManualResetEventSlim(false);

            // Act
            await _redisService.SubscribePatternAsync<string>(pattern, (ch, msg) =>
            {
                receivedMessage = msg;
                messageReceived.Set();
            });

            await Task.Delay(100);


            var subscriber = _redisService.Client.GetSubscriber();
            var serializedMessage = JsonConvert.SerializeObject(null);
            await subscriber.PublishAsync(RedisChannel.Literal(channel), serializedMessage).ConfigureAwait(true);

            var received = messageReceived.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.True(received, "Message was not received within timeout");
            Assert.Null(receivedMessage);

            // Cleanup
            await _redisService.UnsubscribePatternAsync(pattern);
        }

        [Fact]
        public async Task SubscribePatternAsync_WithEmptyString_ShouldReceiveEmpty()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");
            var pattern = $"test.empty.pattern.{guid}.*";
            var channel = $"test.empty.pattern.{guid}.channel";
            string? receivedMessage = null;
            using var messageReceived = new ManualResetEventSlim(false);

            // Act
            await _redisService.SubscribePatternAsync<string>(pattern, (ch, msg) =>
            {
                receivedMessage = msg;
                messageReceived.Set();
            });

            await Task.Delay(100);

            // 发布空字符串
            await _redisService.PublishAsync(channel, string.Empty);

            var received = messageReceived.Wait(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(received, "Message was not received within timeout");
            Assert.NotNull(receivedMessage);
            Assert.Equal(string.Empty, receivedMessage);

            // Cleanup
            await _redisService.UnsubscribePatternAsync(pattern);
        }
        [Fact]
        public async Task SubscribeAsync_WithNullHandler_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.SubscribeAsync<string>("channel", null!));
        }

        [Fact]
        public async Task SubscribeAsync_WithComplexObject_ShouldReceiveObject()
        {
            // Arrange
            var channel = $"test-object-{Guid.NewGuid()}";
            var expectedUser = new TestUser { Id = 1, Name = "John Doe", Email = "john@example.com" };
            TestUser? receivedUser = null;
            using var messageReceived = new ManualResetEventSlim(false);

            // Act
            await _redisService.SubscribeAsync<TestUser>(channel, (ch, user) =>
            {
                receivedUser = user;
                messageReceived.Set();
            });

            await Task.Delay(100);
            await _redisService.PublishAsync(channel, expectedUser);
            var received = messageReceived.Wait(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(received, "User object was not received");
            Assert.NotNull(receivedUser);
            Assert.Equal(expectedUser.Id, receivedUser.Id);
            Assert.Equal(expectedUser.Name, receivedUser.Name);
            Assert.Equal(expectedUser.Email, receivedUser.Email);

            // Cleanup
            await _redisService.UnsubscribeAsync(channel);
        }

        [Fact]
        public async Task SubscribePatternAsync_ShouldReceiveMatchingMessages()
        {
            // Arrange
            var pattern = "news.*";
            var channel1 = "news.sports";
            var channel2 = "news.tech";
            var receivedMessages = new System.Collections.Concurrent.ConcurrentBag<string>();
            using var messagesReceived = new ManualResetEventSlim(false);
            var expectedCount = 2;

            // Act
            await _redisService.SubscribePatternAsync<string>(pattern, (ch, msg) =>
            {
                receivedMessages.Add(msg!);
                if (receivedMessages.Count >= expectedCount)
                {
                    messagesReceived.Set();
                }
            });

            await Task.Delay(100);
            await _redisService.PublishAsync(channel1, "Sports news");
            await _redisService.PublishAsync(channel2, "Tech news");

            var received = messagesReceived.Wait(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(received, "Pattern messages were not received");
            Assert.Equal(expectedCount, receivedMessages.Count);
            Assert.Contains("Sports news", receivedMessages);
            Assert.Contains("Tech news", receivedMessages);

            // Cleanup
            await _redisService.UnsubscribePatternAsync(pattern);
        }

        [Fact]
        public async Task SubscribePatternAsync_WithNullPattern_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.SubscribePatternAsync<string>(null!, (ch, msg) => { }));
        }

        [Fact]
        public async Task SubscribePatternAsync_WithNullHandler_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.SubscribePatternAsync<string>("pattern.*", null!));
        }

        [Fact]
        public async Task UnsubscribeAsync_ShouldStopReceivingMessages()
        {
            // Arrange
            var channel = $"test-unsub-{Guid.NewGuid()}";
            var messageCount = 0;

            await _redisService.SubscribeAsync<string>(channel, (ch, msg) =>
            {
                messageCount++;
            });

            await Task.Delay(100);

            // Act - 发送第一条消息
            await _redisService.PublishAsync(channel, "Message 1");
            await Task.Delay(100);

            // 取消订阅
            await _redisService.UnsubscribeAsync(channel);
            await Task.Delay(100);

            // 发送第二条消息
            await _redisService.PublishAsync(channel, "Message 2");
            await Task.Delay(100);

            // Assert - 应该只收到第一条消息
            Assert.Equal(1, messageCount);
        }

        [Fact]
        public async Task UnsubscribeAsync_WithNullChannel_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.UnsubscribeAsync(null!));
        }

        [Fact]
        public async Task UnsubscribeAllAsync_ShouldUnsubscribeFromAllChannels()
        {
            // Arrange
            var channel1 = $"test-all-1-{Guid.NewGuid()}";
            var channel2 = $"test-all-2-{Guid.NewGuid()}";
            var messageCount = 0;

            await _redisService.SubscribeAsync<string>(channel1, (ch, msg) => messageCount++);
            await _redisService.SubscribeAsync<string>(channel2, (ch, msg) => messageCount++);
            await Task.Delay(100);

            // Act
            await _redisService.UnsubscribeAllAsync();
            await Task.Delay(100);

            await _redisService.PublishAsync(channel1, "Message 1");
            await _redisService.PublishAsync(channel2, "Message 2");
            await Task.Delay(100);

            // Assert - 应该没有收到任何消息
            Assert.Equal(0, messageCount);
        }

        [Fact]
        public async Task UnsubscribePatternAsync_ShouldStopReceivingPatternMessages()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");
            var pattern = $"test.pattern.{guid}.*";
            var channel = $"test.pattern.{guid}.channel";
            var messageCount = 0;

            await _redisService.SubscribePatternAsync<string>(pattern, (ch, msg) =>
            {
                messageCount++;
            });

            await Task.Delay(100);

            // 发送第一条消息
            await _redisService.PublishAsync(channel, "Message 1");
            await Task.Delay(100);

            // Act - 取消模式订阅
            await _redisService.UnsubscribePatternAsync(pattern);
            await Task.Delay(100);

            // 发送第二条消息
            await _redisService.PublishAsync(channel, "Message 2");
            await Task.Delay(100);

            // Assert - 应该只收到第一条消息
            Assert.Equal(1, messageCount);
        }

        [Fact]
        public async Task UnsubscribePatternAsync_WithNullPattern_ShouldThrowException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _redisService.UnsubscribePatternAsync(null!));
        }

        [Fact]
        public async Task MultipleSubscribers_ShouldAllReceiveMessage()
        {
            // Arrange
            var channel = $"test-multi-{Guid.NewGuid()}";
            var message = "Broadcast message";
            var received1 = false;
            var received2 = false;
            using var event1 = new ManualResetEventSlim(false);
            using var event2 = new ManualResetEventSlim(false);

            // Act - 两个订阅者
            await _redisService.SubscribeAsync<string>(channel, (ch, msg) =>
            {
                if (msg == message)
                {
                    received1 = true;
                    event1.Set();
                }
            });
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            var serviceProvider = services.AddRedisService(options =>
            {
                configuration.GetSection("Redis").Bind(options);
                options.UsingNewtonsoft(settings =>
                {
                    settings.MaxDepth = 12;
                });
            }).BuildServiceProvider();

            var _redisService2 = serviceProvider.GetRequiredService<IRedisService>();

            await _redisService2.SubscribeAsync<string>(channel, (ch, msg) =>
            {
                if (msg == message)
                {
                    received2 = true;
                    event2.Set();
                }
            });

            await Task.Delay(100);
            var subscribers = await _redisService.PublishAsync(channel, message);

            var bothReceived = event1.Wait(TimeSpan.FromSeconds(5)) && event2.Wait(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(bothReceived, "Not all subscribers received the message");
            Assert.True(received1);
            Assert.True(received2);
            Assert.True(subscribers >= 2);

            // Cleanup
            await _redisService.UnsubscribeAsync(channel);
        }

        private class TestUser
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
#endif
