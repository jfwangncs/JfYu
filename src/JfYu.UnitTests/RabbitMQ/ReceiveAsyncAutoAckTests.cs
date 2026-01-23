using JfYu.RabbitMQ;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace JfYu.UnitTests.RabbitMQ
{
    [Collection("RabbitMQ")]
    public class ReceiveAsyncAutoAckTests
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly Dictionary<string, object?> header = new() { { "x-expires", 600000 } };

        public ReceiveAsyncAutoAckTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            _rabbitMQService = serviceProvider.GetRequiredService<IRabbitMQService>();
        }

        [Fact]
        public async Task ReceiveAsync_AutoAck_String_SingleMessage_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_AutoAck_String_SingleMessage_Success)}";
            string queueName = $"{nameof(ReceiveAsync_AutoAck_String_SingleMessage_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Test message for auto-ack ReceiveAsync";
            string? receivedMessage = null;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            }, autoAck: true);

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            Assert.Equal(testMessage, receivedMessage);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_AutoAck_TestModel_SingleMessage_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_AutoAck_TestModel_SingleMessage_Success)}";
            string queueName = $"{nameof(ReceiveAsync_AutoAck_TestModel_SingleMessage_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var testMessage = new TestModelFaker().Generate();
            TestModel? receivedMessage = null;

            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async message =>
            {
                receivedMessage = message;
                return await Task.FromResult(true).ConfigureAwait(true);
            }, autoAck: true);

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            Assert.Equal(JsonConvert.SerializeObject(testMessage), JsonConvert.SerializeObject(receivedMessage));

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_AutoAck_MultipleMessages_Success()
        {
            string exchangeName = $"{nameof(ReceiveAsync_AutoAck_MultipleMessages_Success)}";
            string queueName = $"{nameof(ReceiveAsync_AutoAck_MultipleMessages_Success)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            var testMessages = new TestModelFaker().Generate(5);
            var receivedMessages = new List<TestModel>();

            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async message =>
            {
                receivedMessages.Add(message!);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, prefetchCount: 10, autoAck: true);

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
        public async Task ReceiveAsync_AutoAck_ProcessingReturnsFalse_MessageStillAcknowledged()
        {
            string exchangeName = $"{nameof(ReceiveAsync_AutoAck_ProcessingReturnsFalse_MessageStillAcknowledged)}";
            string queueName = $"{nameof(ReceiveAsync_AutoAck_ProcessingReturnsFalse_MessageStillAcknowledged)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Message that should be auto-acknowledged";
            var processingAttempts = 0;
            var messageProcessed = false;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                messageProcessed = true;
                return await Task.FromResult(false).ConfigureAwait(true); // Return false but should still be auto-acked
            }, autoAck: true);

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            // In auto-ack mode, the message should be processed only once and auto-acknowledged
            Assert.Equal(1, processingAttempts);
            Assert.True(messageProcessed);

            // Verify message is not in queue anymore (was auto-acked)
            var queueInfo = await channel.QueueDeclarePassiveAsync(queueName);
            Assert.Equal(0u, queueInfo.MessageCount);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_AutoAck_ProcessingThrowsException_MessageStillAcknowledged()
        {
            string exchangeName = $"{nameof(ReceiveAsync_AutoAck_ProcessingThrowsException_MessageStillAcknowledged)}";
            string queueName = $"{nameof(ReceiveAsync_AutoAck_ProcessingThrowsException_MessageStillAcknowledged)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Message that will cause exception";
            var processingAttempts = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                await Task.CompletedTask.ConfigureAwait(true);
                throw new InvalidOperationException("Test exception");
            }, autoAck: true);

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(1000);

            // In auto-ack mode, even with exception, message should be processed only once and auto-acknowledged
            Assert.Equal(1, processingAttempts);

            // Verify message is not in queue anymore (was auto-acked despite exception)
            var queueInfo = await channel.QueueDeclarePassiveAsync(queueName);
            Assert.Equal(0u, queueInfo.MessageCount);

            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task ReceiveAsync_ManualAck_ProcessingReturnsFalse_MessageRetried()
        {
            string exchangeName = $"{nameof(ReceiveAsync_ManualAck_ProcessingReturnsFalse_MessageRetried)}";
            string queueName = $"{nameof(ReceiveAsync_ManualAck_ProcessingReturnsFalse_MessageRetried)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", header);

            string testMessage = "Message that should be retried";
            var processingAttempts = 0;

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                processingAttempts++;
                return await Task.FromResult(false).ConfigureAwait(true); // Trigger retry
            }, autoAck: false); // Manual ack mode (default)

            await _rabbitMQService.SendAsync(exchangeName, testMessage);
            await Task.Delay(2000); // Wait longer to allow retries

            // In manual ack mode, the message should be retried multiple times
            Assert.True(processingAttempts > 1);

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
                await Task.Delay(10000).ConfigureAwait(true);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, prefetchCount: 1, autoAck: true, cancellationToken: cts.Token);

            // Send some messages
            for (int i = 0; i < 10; i++)
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
            Assert.True(processingCount < 10);
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
            for (int i = 0; i < 10; i++)
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
            }, 10, autoAck: true, cancellationToken: cts.Token);
            await Task.Delay(400);
            // Should process some messages before cancellation
            Assert.True(processingCount > 0);
            Assert.True(processingCount < 10);
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
                await _rabbitMQService.ReceiveAsync<string>("queue_not_used", async _ => await Task.FromResult(true).ConfigureAwait(true), prefetchCount: 1, autoAck: true, cancellationToken: cts.Token).ConfigureAwait(true);
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
            }, prefetchCount: 1, autoAck: true, cancellationToken: cts.Token);

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

    }
}
