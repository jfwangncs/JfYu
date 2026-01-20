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
                await Task.CompletedTask;
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
        public async Task ReceiveAsync_AutoAck_WithCancellationToken_Cancelled()
        {
            string exchangeName = $"{nameof(ReceiveAsync_AutoAck_WithCancellationToken_Cancelled)}";
            string queueName = $"{nameof(ReceiveAsync_AutoAck_WithCancellationToken_Cancelled)}";

            var tempChannel = await _rabbitMQService.Connection.CreateChannelAsync();
            await tempChannel.QueueDeclareAsync(queueName, true, false, false, header);
            await tempChannel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, true);
            await tempChannel.QueueBindAsync(queueName, exchangeName, "", null);

            using var cts = new CancellationTokenSource();
            var receivedMessages = new List<string>();

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async message =>
            {
                receivedMessages.Add(message!);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, autoAck: true, cancellationToken: cts.Token);

            await _rabbitMQService.SendAsync(exchangeName, "Message 1");
            await Task.Delay(500);

            // Cancel the consumer
            cts.Cancel();
            await Task.Delay(1000); // Wait longer for cancellation to take effect

            var initialCount = receivedMessages.Count;

            // Send more messages after cancellation
            await _rabbitMQService.SendAsync(exchangeName, "Message 2");
            await Task.Delay(1000);

            // After cancellation, no more messages should be received
            Assert.Equal(initialCount, receivedMessages.Count);
            Assert.Contains("Message 1", receivedMessages);

            // Clean up using tempChannel since the consumer channel is disposed
            await tempChannel.QueueDeleteAsync(queueName);
            await tempChannel.ExchangeDeleteAsync(exchangeName);
            await tempChannel.CloseAsync();
            await tempChannel.DisposeAsync();
        }
    }
}
