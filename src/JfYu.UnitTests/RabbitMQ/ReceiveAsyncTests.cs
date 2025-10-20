using JfYu.RabbitMQ;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text;

namespace JfYu.UnitTests.RabbitMQ
{
    [Collection("RabbitMQ")]
    public class ReceiveAsyncTests
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly Dictionary<string, object?> header = new() { { "x-expires", 600000 } };

        public ReceiveAsyncTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            _rabbitMQService = serviceProvider.GetRequiredService<IRabbitMQService>();
        }

        [Fact]
        public async Task ReceiveAsync_String_SingleMessage_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_String_SingleMessage_Success)}";
            string queueName = $"{nameof(ReceiveAsync_String_SingleMessage_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Test message for ReceiveAsync";
            string? receivedMessage = null;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            Assert.Equal(testMessage, receivedMessage);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_TestModel_SingleMessage_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_TestModel_SingleMessage_Success)}";
            string queueName = $"{nameof(ReceiveAsync_TestModel_SingleMessage_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var testMessage = new TestModelFaker().Generate();
            TestModel? receivedMessage = null;

            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            Assert.Equal(JsonConvert.SerializeObject(testMessage), JsonConvert.SerializeObject(receivedMessage));

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_MultipleMessages_WithPrefetchCount_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_MultipleMessages_WithPrefetchCount_Success)}";
            string queueName = $"{nameof(ReceiveAsync_MultipleMessages_WithPrefetchCount_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var testMessages = new TestModelFaker().Generate(5);
            var receivedMessages = new List<TestModel>();

            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async message =>
            {
                receivedMessages.Add(message!);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, prefetchCount: 10);

            foreach (var message in testMessages)
            {
                await _rabbitMQService.SendAsync(exchangeName, message);
            }

            await Task.Delay(1000);

            Assert.Equal(testMessages.Count, receivedMessages.Count);
            Assert.Equal(JsonConvert.SerializeObject(testMessages), JsonConvert.SerializeObject(receivedMessages));

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_ProcessingReturnsFalse_MessageNotAcknowledged()
        {
            string exchangeName = $"{nameof(ReceiveAsync_ProcessingReturnsFalse_MessageNotAcknowledged)}";
            string queueName = $"{nameof(ReceiveAsync_ProcessingReturnsFalse_MessageNotAcknowledged)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Message that should not be acknowledged";
            var processingAttempts = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                return await Task.FromResult(false).ConfigureAwait(true); // Don't acknowledge the message
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            // The message should be processed at least once but not acknowledged
            Assert.True(processingAttempts >= 1);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_WithCancellationToken_Cancelled()
        {
            string exchangeName = $"{nameof(ReceiveAsync_WithCancellationToken_Cancelled)}";
            string queueName = $"{nameof(ReceiveAsync_WithCancellationToken_Cancelled)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            using var cts = new CancellationTokenSource();
            var processingCount = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                Interlocked.Increment(ref processingCount);
                await Task.Delay(500).ConfigureAwait(true); 
                return await Task.FromResult(true).ConfigureAwait(true);
            }, cancellationToken: cts.Token);

            // Send some messages
            for (int i = 0; i < 50; i++)
            {
                await _rabbitMQService.SendAsync(exchangeName, $"Message {i}");
            }

#if NET8_0_OR_GREATER
            await cts.CancelAsync().ConfigureAwait(true);
#else
            cts.Cancel();
#endif
            await Task.Delay(1000);

            // Should process some messages before cancellation
            Assert.True(processingCount > 0);
            Assert.True(processingCount < 50);
            Assert.True(channel.IsClosed);

            var channel1 = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel1.QueueDeleteAsync(queueName);
            await channel1.ExchangeDeleteAsync(exchangeName);
            await channel1.CloseAsync();
        }

        [Fact]
        public async Task ReceiveAsync_WithCancellationToken10_Cancelled()
        {
            string exchangeName = $"{nameof(ReceiveAsync_WithCancellationToken10_Cancelled)}";
            string queueName = $"{nameof(ReceiveAsync_WithCancellationToken10_Cancelled)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            using var cts = new CancellationTokenSource();
            var processingCount = 0;
            // Send some messages
            for (int i = 0; i < 50; i++)
            {
                await _rabbitMQService.SendAsync(exchangeName, $"Message {i}");
            }
            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                Interlocked.Increment(ref processingCount);
#if NET8_0_OR_GREATER
            await cts.CancelAsync().ConfigureAwait(true);
#else
                cts.Cancel();
#endif
                return await Task.FromResult(true).ConfigureAwait(true);
            }, 10, cancellationToken: cts.Token);
            await Task.Delay(2000);
            // Should process some messages before cancellation
            Assert.True(processingCount > 0);
            Assert.True(processingCount < 50);
            Assert.True(channel.IsClosed);

            var channel1 = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel1.QueueDeleteAsync(queueName);
            await channel1.ExchangeDeleteAsync(exchangeName);
            await channel1.CloseAsync();
        }
        [Fact]
        public async Task ReceiveAsync_CancelledBeforeStart_Throws()
        {
            using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
            await cts.CancelAsync().ConfigureAwait(true);
#else
            cts.Cancel();
#endif
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _rabbitMQService.ReceiveAsync<string>("queue_not_used", async _ => await Task.FromResult(true).ConfigureAwait(true), cancellationToken: cts.Token).ConfigureAwait(true);
            });
        }

        [Fact]
        public async Task ReceiveAsync_CancelledBeforeDelivery_DoesNotProcess()
        {
            string exchangeName = $"{nameof(ReceiveAsync_CancelledBeforeDelivery_DoesNotProcess)}";
            string queueName = $"{nameof(ReceiveAsync_CancelledBeforeDelivery_DoesNotProcess)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            using var cts = new CancellationTokenSource();
            var processed = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                Interlocked.Increment(ref processed);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, cancellationToken: cts.Token);

            // Cancel immediately to trigger early return in consumer
#if NET8_0_OR_GREATER
            await cts.CancelAsync().ConfigureAwait(true);
#else
            cts.Cancel();
#endif

            // Send a message after cancellation
            await _rabbitMQService.SendAsync(exchangeName, "after cancel");
            await Task.Delay(500);

            // No processing should have happened
            Assert.Equal(0, processed);

            // Message should still be in queue (not acked)
            var q = await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);
            Assert.True(q.MessageCount >= 1);

            var cleanup = await _rabbitMQService.Connection.CreateChannelAsync();
            await cleanup.QueuePurgeAsync(queueName);
            await cleanup.QueueDeleteAsync(queueName);
            await cleanup.ExchangeDeleteAsync(exchangeName);
            await cleanup.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_EmptyQueue_NoMessagesReceived()
        {
            string exchangeName = $"{nameof(ReceiveAsync_EmptyQueue_NoMessagesReceived)}";
            string queueName = $"{nameof(ReceiveAsync_EmptyQueue_NoMessagesReceived)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var receivedMessages = new List<string>();

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessages.Add(message ?? "");
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await Task.Delay(1000);

            Assert.Empty(receivedMessages);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_NonExistentQueue_ThrowsException()
        {
            string nonExistentQueue = $"{nameof(ReceiveAsync_NonExistentQueue_ThrowsException)}_NonExistent";

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _rabbitMQService.ReceiveAsync<string>(nonExistentQueue, async message =>
                {
                    return await Task.FromResult(true).ConfigureAwait(true);
                }).ConfigureAwait(true);
            });
        }

        [Fact]
        public async Task ReceiveAsync_ProcessingThrowsException_MessageHandledGracefully()
        {
            string exchangeName = $"{nameof(ReceiveAsync_ProcessingThrowsException_MessageHandledGracefully)}";
            string queueName = $"{nameof(ReceiveAsync_ProcessingThrowsException_MessageHandledGracefully)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Message that causes exception";
            var processingAttempts = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                if (processingAttempts == 1)
                {
                    throw new InvalidOperationException("Simulated processing error");
                }
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            // The message should be processed at least once despite the exception
            Assert.True(processingAttempts >= 1);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_Fanout_MultipleQueues_AllReceiveMessage()
        {
            string exchangeName = $"{nameof(ReceiveAsync_Fanout_MultipleQueues_AllReceiveMessage)}";
            string queueName1 = $"{nameof(ReceiveAsync_Fanout_MultipleQueues_AllReceiveMessage)}_1";
            string queueName2 = $"{nameof(ReceiveAsync_Fanout_MultipleQueues_AllReceiveMessage)}_2";

            await _rabbitMQService.QueueDeclareAsync(queueName1, exchangeName, ExchangeType.Fanout, "", header);
            await _rabbitMQService.QueueDeclareAsync(queueName2, exchangeName, ExchangeType.Fanout, "", header);

            string testMessage = "Fanout broadcast message";
            string? receivedMessage1 = null;
            string? receivedMessage2 = null;

            var channel1 = await _rabbitMQService.ReceiveAsync<string>(queueName1, async message =>
            {
                receivedMessage1 = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            var channel2 = await _rabbitMQService.ReceiveAsync<string>(queueName2, async message =>
            {
                receivedMessage2 = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1500);

            Assert.Equal(testMessage, receivedMessage1);
            Assert.Equal(testMessage, receivedMessage2);

            await channel1.QueueDeleteAsync(queueName1);
            await channel1.QueueDeleteAsync(queueName2);
            await channel1.ExchangeDeleteAsync(exchangeName);
            await channel1.CloseAsync();
            await channel1.DisposeAsync();
            await channel2.CloseAsync();
            await channel2.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_Topic_RoutingKeyMatching_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_Topic_RoutingKeyMatching_Success)}";
            string queueName1 = $"{nameof(ReceiveAsync_Topic_RoutingKeyMatching_Success)}_error";
            string queueName2 = $"{nameof(ReceiveAsync_Topic_RoutingKeyMatching_Success)}_all";

            await _rabbitMQService.QueueDeclareAsync(queueName1, exchangeName, ExchangeType.Topic, "logs.error.*", header);
            await _rabbitMQService.QueueDeclareAsync(queueName2, exchangeName, ExchangeType.Topic, "logs.*.*", header);

            var receivedMessages1 = new List<string>();
            var receivedMessages2 = new List<string>();

            var channel1 = await _rabbitMQService.ReceiveAsync<string>(queueName1, async message =>
            {
                receivedMessages1.Add(message ?? "");
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            var channel2 = await _rabbitMQService.ReceiveAsync<string>(queueName2, async message =>
            {
                receivedMessages2.Add(message ?? "");
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            string errorMessage = "Error occurred";
            string infoMessage = "Info message";

            await _rabbitMQService.SendAsync(exchangeName, errorMessage, "logs.error.database");
            await _rabbitMQService.SendAsync(exchangeName, infoMessage, "logs.info.application");

            await Task.Delay(1500);

            // Error queue should only receive error message
            Assert.Single(receivedMessages1);
            Assert.Contains(errorMessage, receivedMessages1);

            // All queue should receive both messages
            Assert.Equal(2, receivedMessages2.Count);
            Assert.Contains(errorMessage, receivedMessages2);
            Assert.Contains(infoMessage, receivedMessages2);

            await channel1.QueueDeleteAsync(queueName1);
            await channel1.QueueDeleteAsync(queueName2);
            await channel1.ExchangeDeleteAsync(exchangeName);
            await channel1.CloseAsync();
            await channel1.DisposeAsync();
            await channel2.CloseAsync();
            await channel2.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_Headers_HeaderMatching_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_Headers_HeaderMatching_Success)}";
            string queueName = $"{nameof(ReceiveAsync_Headers_HeaderMatching_Success)}";

            var bindingHeaders = new Dictionary<string, object?>
            {
                { "x-match", "all" },
                { "type", "error" },
                { "format", "json" },
                { "x-expires", 60000 }
            };

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Headers, "", bindingHeaders);

            var receivedMessages = new List<string>();

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessages.Add(message ?? "");
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            string matchingMessage = "Message with matching headers";
            string nonMatchingMessage = "Message with non-matching headers";

            var matchingHeaders = new Dictionary<string, object?>
            {
                { "type", "error" },
                { "format", "json" }
            };

            var nonMatchingHeaders = new Dictionary<string, object?>
            {
                { "type", "info" },
                { "format", "xml" }
            };
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _rabbitMQService.SendAsync(exchangeName, nonMatchingMessage, "", nonMatchingHeaders).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, matchingMessage, "", matchingHeaders);

            await Task.Delay(1500);

            // Only message with matching headers should be received
            Assert.Single(receivedMessages);
            Assert.Contains(matchingMessage, receivedMessages);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_LargeMessage_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_LargeMessage_Success)}";
            string queueName = $"{nameof(ReceiveAsync_LargeMessage_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            // Create a large message (1MB of text)
            var largeMessage = new StringBuilder();
            for (int i = 0; i < 100000; i++)
            {
                largeMessage.Append("This is a large message content. ");
            }

            string testMessage = largeMessage.ToString();
            string? receivedMessage = null;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(3000); // Give more time for large message

            Assert.Equal(testMessage, receivedMessage);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_NullMessage_HandledGracefully()
        {
            string exchangeName = $"{nameof(ReceiveAsync_NullMessage_HandledGracefully)}";
            string queueName = $"{nameof(ReceiveAsync_NullMessage_HandledGracefully)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            object? receivedMessage = "not_null";

            var channel = await _rabbitMQService.ReceiveAsync<string?>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            // Send null message
            await _rabbitMQService.SendAsync<string?>(exchangeName, null);
            await Task.Delay(1000);

            Assert.Equal(string.Empty, receivedMessage);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        public async Task ReceiveAsync_DifferentPrefetchCounts_Success(ushort prefetchCount)
        {
            string exchangeName = $"{nameof(ReceiveAsync_DifferentPrefetchCounts_Success)}_{prefetchCount}";
            string queueName = $"{nameof(ReceiveAsync_DifferentPrefetchCounts_Success)}_{prefetchCount}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var messageCount = Math.Max(prefetchCount * 2, 10);
            var testMessages = Enumerable.Range(1, messageCount).Select(i => $"Message {i}").ToList();
            var receivedMessages = new List<string>();
            var processedCount = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessages.Add(message ?? "");
                Interlocked.Increment(ref processedCount);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, prefetchCount);

            // Send all messages
            await _rabbitMQService.SendBatchAsync(exchangeName, testMessages);

            // Wait for processing
            await Task.Delay(5000);

            Assert.Equal(testMessages.Count, receivedMessages.Count);
            Assert.Equal(testMessages.Count, processedCount);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_ComplexModel_WithNestedObjects_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_ComplexModel_WithNestedObjects_Success)}";
            string queueName = $"{nameof(ReceiveAsync_ComplexModel_WithNestedObjects_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var testMessage = new TestModelFaker().Generate();
            TestModel? receivedMessage = null;

            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            Assert.NotNull(receivedMessage);
            Assert.Equal(testMessage.Id, receivedMessage.Id);
            Assert.Equal(testMessage.Name, receivedMessage.Name);
            Assert.Equal(testMessage.Age, receivedMessage.Age);
            Assert.Equal(testMessage.Address, receivedMessage.Address);
            Assert.Equal(JsonConvert.SerializeObject(testMessage.Sub), JsonConvert.SerializeObject(receivedMessage.Sub));
            Assert.Equal(JsonConvert.SerializeObject(testMessage.Items), JsonConvert.SerializeObject(receivedMessage.Items));

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task QueueDeclareAsync_NoExchange_Success()
        {
            string queueName = $"{nameof(QueueDeclareAsync_NoExchange_Success)}";

            // Act
            var queueDeclareOk = await _rabbitMQService.QueueDeclareAsync(queueName, headers: header);

            // Assert
            Assert.Equal(queueName, queueDeclareOk.QueueName);
            Assert.Equal(0, (int)queueDeclareOk.MessageCount);
            Assert.Equal(0, (int)queueDeclareOk.ConsumerCount);

            // Cleanup
            using var channel = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel.QueueDeleteAsync(queueName);
        }

        [Fact]
        public async Task ReceiveAsync_ProcessingThrowsException_MovesToRetry()
        {
            var services = new ServiceCollection();
            var logger = new Mock<ILogger<RabbitMQService>>();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();

            // Arrange
            string exchangeName = $"{nameof(ReceiveAsync_ProcessingThrowsException_MovesToRetry)}";
            string queueName = $"{nameof(ReceiveAsync_ProcessingThrowsException_MovesToRetry)}";

            await service.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var processingAttempts = 0;
            var completionSource = new TaskCompletionSource<bool>();

            await service.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                if (processingAttempts == 1)
                {
                    throw new InvalidOperationException("Simulated processing error");
                }
                // On second attempt, we succeed.
                completionSource.SetResult(true);
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            // Act
            await service.SendAsync(exchangeName, "test message");

            // Assert
            await Task.WhenAny(completionSource.Task, Task.Delay(1000));
            Assert.True(completionSource.Task.IsCompleted);
            Assert.Equal(2, processingAttempts); // Should be processed twice (original + 1 retry)
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(1));
            // Cleanup
            using var channel = await service.Connection.CreateChannelAsync();
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
        }

        [Fact]
        public async Task ReceiveAsync_ProcessingThrowsException_NoLogger_MovesToRetry()
        {
            // Arrange
            string exchangeName = $"{nameof(ReceiveAsync_ProcessingThrowsException_NoLogger_MovesToRetry)}";
            string queueName = $"{nameof(ReceiveAsync_ProcessingThrowsException_NoLogger_MovesToRetry)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var processingAttempts = 0;
            var completionSource = new TaskCompletionSource<bool>();

            await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                if (processingAttempts == 1)
                {
                    throw new InvalidOperationException("Simulated processing error");
                }
                // On second attempt, we succeed.
                completionSource.SetResult(true);
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            // Act
            await _rabbitMQService.SendAsync(exchangeName, "test message");

            // Assert
            await Task.WhenAny(completionSource.Task, Task.Delay(1000));
            Assert.True(completionSource.Task.IsCompleted);
            Assert.Equal(2, processingAttempts); // Should be processed twice (original + 1 retry)

            // Cleanup
            using var channel = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
        }

        [Fact]
        public async Task TryToMoveToDeadLetterQueue_NoHeaders_RejectsMessage()
        {
            // Arrange 
            var services = new ServiceCollection();
            var logger = new Mock<ILogger<RabbitMQService>>();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();


            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = null }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            // We expect this to throw because BasicRejectAsync will fail on a non-existent delivery tag.
            // This confirms the correct path was taken.
            await ((Task)method!.Invoke(service, [deliverEventArgs, channel, CancellationToken.None])!).ConfigureAwait(true);
            logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(1));
            await channel.CloseAsync();
        }

        [Fact]
        public async Task TryToMoveToDeadLetterQueue_NoRetryCountHeader_RejectsMessage()
        {

            // Arrange 
            var services = new ServiceCollection();
            var logger = new Mock<ILogger<RabbitMQService>>();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();


            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = new Dictionary<string, object?>() }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            // We expect this to throw because BasicRejectAsync will fail on a non-existent delivery tag.
            // This confirms the correct path was taken.
            await ((Task)method!.Invoke(service, [deliverEventArgs, channel, CancellationToken.None])!).ConfigureAwait(true);
            logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(1));
            await channel.CloseAsync();
        }
        [Fact]
        public async Task TryToMoveToDeadLetterQueue_NoHeaders_NoLogger_RejectsMessage()
        {
            // Arrange 
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();


            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = null }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            // We expect this to throw because BasicRejectAsync will fail on a non-existent delivery tag.
            // This confirms the correct path was taken.
            await ((Task)method!.Invoke(service, [deliverEventArgs, channel, CancellationToken.None])!).ConfigureAwait(true);
            Assert.True(true);
            await channel.CloseAsync();
        }

        [Fact]
        public async Task TryToMoveToDeadLetterQueue_NoRetryCountHeader_NoLogger_RejectsMessage()
        {

            // Arrange 
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();


            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = new Dictionary<string, object?>() }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            // We expect this to throw because BasicRejectAsync will fail on a non-existent delivery tag.
            // This confirms the correct path was taken.
            await ((Task)method!.Invoke(service, [deliverEventArgs, channel, CancellationToken.None])!).ConfigureAwait(true);
            Assert.True(true);
            await channel.CloseAsync();
        }

        [Fact]
        public async Task TryToMoveToDeadLetterQueue_MaxRetryExceeded_RejectsToDeadLetter()
        {
            // Arrange 
            var services = new ServiceCollection();
            var logger = new Mock<ILogger<RabbitMQService>>();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();

            var headers = new Dictionary<string, object?>
            {
                { "x-retry-count", 999 } // Exceeds any reasonable MaxRetryCount
            };
            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = headers }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            // We expect this to throw because BasicRejectAsync will fail on a non-existent delivery tag.
            // This confirms the correct path was taken.
            await ((Task)method!.Invoke(service, [deliverEventArgs, channel, CancellationToken.None])!).ConfigureAwait(true);
            logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(1));
            await channel.CloseAsync();
        }
        [Fact]
        public async Task TryToMoveToDeadLetterQueue_ThrowException()
        {
            // Arrange 
            var services = new ServiceCollection();
            var logger = new Mock<ILogger<RabbitMQService>>();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var mockChannel = new Mock<IChannel>();
            mockChannel.Setup(x => x.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Publish failed"));

            var headers = new Dictionary<string, object?>
            {
                  { "x-retry-count", 1 },
                { "x-exchange-name", Encoding.UTF8.GetBytes("exchange") },
                { "x-exchange-routing-key", Encoding.UTF8.GetBytes("routingKey") }
            };
            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = headers }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)method!.Invoke(service, [deliverEventArgs, mockChannel.Object, CancellationToken.None])!);
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(1));
        }

        [Fact]
        public async Task TryToMoveToDeadLetterQueue_NoLogger_ThrowException()
        {
            // Arrange 
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            var service = (RabbitMQService)serviceProvider.GetRequiredService<IRabbitMQService>();
            var mockChannel = new Mock<IChannel>();
            mockChannel.Setup(x => x.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Publish failed"));
            var headers = new Dictionary<string, object?>
            {
                  { "x-retry-count", 1 },
                { "x-exchange-name", Encoding.UTF8.GetBytes("exchange") },
                { "x-exchange-routing-key", Encoding.UTF8.GetBytes("routingKey") }
            };
            var deliverEventArgs = new BasicDeliverEventArgs("", 1, false, "", "", new BasicProperties { Headers = headers }, Encoding.UTF8.GetBytes("test message"));


            var method = service.GetType().GetMethod("TryToMoveToDeadLetterQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert    
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)method!.Invoke(service, [deliverEventArgs, mockChannel.Object, CancellationToken.None])!);
        }
    }
}