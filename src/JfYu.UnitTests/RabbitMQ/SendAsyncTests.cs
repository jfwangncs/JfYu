using JfYu.RabbitMQ;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace JfYu.UnitTests.RabbitMQ
{
    [Collection("RabbitMQ")]
    public class SendAsyncTests
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly Dictionary<string, object?> _header = new() { { "x-expires", 60000 } };

        public SendAsyncTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            _rabbitMQService = serviceProvider.GetRequiredService<IRabbitMQService>();
        }

        public class NullStringExpectData : TheoryData<string, string?>
        {
            public NullStringExpectData()
            {
                Add("1", null);
                Add("2", "");
                Add("3", "    ");
                Add("4", "This is message");
            }
        }
        [Fact]
        public async Task BindAsync_Again_WithDifferentHeaders_ThrowException()
        {
            string queueName = $"{nameof(BindAsync_Again_WithDifferentHeaders_ThrowException)}";
            await _rabbitMQService.QueueDeclareAsync(queueName);
            await Assert.ThrowsAnyAsync<Exception>(async () => await _rabbitMQService.QueueDeclareAsync(queueName, "", ExchangeType.Direct, "", new Dictionary<string, object?> { ["x-message-ttl"] = 2000 }).ConfigureAwait(true));
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel.QueueDeleteAsync(queueName);
        }

        [Fact]
        public void AddRabbitMQService_ThrowException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddRabbitMQ(null!));
        }

        [Fact]
        public async Task SendSync_QueueNotAvailable_ThrowException()
        {
            string queueName = $"{nameof(SendSync_QueueNotAvailable_ThrowException)}";
            var exception = await Record.ExceptionAsync(async () => await _rabbitMQService.SendAsync("", "This is a test message", queueName).ConfigureAwait(true));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom<PublishException>(exception);             
        }

        [Fact]
        public async Task SendSync_ExchangeNotAvailable_ThrowException()
        {
            string exchangeName = $"{nameof(SendSync_ExchangeNotAvailable_ThrowException)}";
            await Assert.ThrowsAsync<AlreadyClosedException>(async () => await _rabbitMQService.SendAsync(exchangeName, "This is a test message").ConfigureAwait(true));
        }

        [Fact]
        public async Task SendSync_ExchangeAvailableQueueNot_ThrowException()
        {
            string exchangeName = $"{nameof(SendSync_ExchangeAvailableQueueNot_ThrowException)}";
            var channel = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, true, true);

            var exception = await Record.ExceptionAsync(async () => await _rabbitMQService.SendAsync(exchangeName, "This is a test message").ConfigureAwait(true));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom<PublishException>(exception);
            await channel.ExchangeDeleteAsync(exchangeName);
        }

        [Theory]
        [ClassData(typeof(NullStringExpectData))]
        public async Task SendSync_NullString_Correctly(string index, string? message)
        {
            string exchangeName = $"{nameof(SendSync_NullString_Correctly)}{index}";
            string queueName = $"{nameof(SendSync_NullString_Correctly)}{index}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", _header);

            await _rabbitMQService.SendAsync(exchangeName, message);

            var receivedMessages = "";
            var channel = await _rabbitMQService.ReceiveAsync<string?>(queueName, async q =>
              {
                  receivedMessages = q;
                  return await Task.FromResult(true).ConfigureAwait(true);
              });

            await Task.Delay(500);
            Assert.Equal(message ?? "", receivedMessages);
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_NullT_Correctly()
        {
            string exchangeName = $"{nameof(SendSync_NullT_Correctly)})";
            string queueName = $"{nameof(SendSync_NullT_Correctly)})";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", _header);

            await _rabbitMQService.SendAsync(exchangeName, "");
            await _rabbitMQService.SendAsync(exchangeName, "           ");
            await _rabbitMQService.SendAsync(exchangeName, (TestModel)null!);

            var channel = await _rabbitMQService.ReceiveAsync<TestModel?>(queueName, async q =>
            {
                Assert.Null(q);
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await Task.Delay(500);
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_CanBeCancelled_Midway_ThrowException()
        {
            string exchangeName = $"{nameof(SendSync_CanBeCancelled_Midway_ThrowException)}";
            string queueName = $"{nameof(SendSync_CanBeCancelled_Midway_ThrowException)}";
            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName);
            using var cts = new CancellationTokenSource();

            var messages = new TestModelFaker().Generate(20000);
            // Act
            var sendingTask = _rabbitMQService.SendBatchAsync(exchangeName, messages, "", null, cts.Token);

            await Task.Delay(100);
#if NET8_0_OR_GREATER
            await cts.CancelAsync().ConfigureAwait(true);
#else
            cts.Cancel();
#endif

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sendingTask);
            var queue = await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName);
            Assert.True(queue.MessageCount > 1);
            Assert.True(queue.MessageCount < 20000);

            var channel = await _rabbitMQService.Connection.CreateChannelAsync();
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
        }

        [Fact]
        public async Task SendSync_Model_Correctly()
        {
            string exchangeName = $"{nameof(SendSync_Model_Correctly)}";
            string queueName = $"{nameof(SendSync_Model_Correctly)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", _header);

            var message = new TestModelFaker().Generate(12);
            await _rabbitMQService.SendAsync(exchangeName, message);

            var receivedMessages = new List<TestModel>();
            var channel = await _rabbitMQService.ReceiveAsync<List<TestModel>>(queueName, async q =>
            {
                receivedMessages = q;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await Task.Delay(500);
            Assert.Equal(JsonConvert.SerializeObject(message), JsonConvert.SerializeObject(receivedMessages));
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_Strings_Correctly()
        {
            string exchangeName = $"{nameof(SendSync_Strings_Correctly)}";
            string queueName = $"{nameof(SendSync_Strings_Correctly)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", _header);


            var messages = new TestModelFaker().Generate(12).Select(q => q.Name).ToList();
            await _rabbitMQService.SendBatchAsync(exchangeName, messages);

            List<string> receivedMessages = [];
            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async q =>
            {
                receivedMessages.Add(q!);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, 100);

            await Task.Delay(500);
            Assert.Equal(JsonConvert.SerializeObject(messages), JsonConvert.SerializeObject(receivedMessages));
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_Models_Correctly()
        {
            string exchangeName = $"{nameof(SendSync_Models_Correctly)}";
            string queueName = $"{nameof(SendSync_Models_Correctly)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct, "", _header);

            var message = new TestModelFaker().Generate(12);
            await _rabbitMQService.SendBatchAsync(exchangeName, message);

            var receivedMessages = new List<TestModel>();
            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async q =>
            {
                receivedMessages.Add(q!);
                return await Task.FromResult(true).ConfigureAwait(true);
            }, 100);

            await Task.Delay(500);
            Assert.Equal(JsonConvert.SerializeObject(message), JsonConvert.SerializeObject(receivedMessages));
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_Fanout_Correctly()
        {
            string exchangeName = $"{nameof(SendSync_Fanout_Correctly)}";
            string queueName1 = $"{nameof(SendSync_Fanout_Correctly)}q1";
            string queueName2 = $"{nameof(SendSync_Fanout_Correctly)}q2";

            await _rabbitMQService.QueueDeclareAsync(queueName1, exchangeName, ExchangeType.Fanout, "", _header);
            await _rabbitMQService.QueueDeclareAsync(queueName2, exchangeName, ExchangeType.Fanout, "", _header);


            string receivedMessagesQueue1 = "";
            var receivedMessagesQueue2 = "";

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName1, async q =>
            {
                receivedMessagesQueue1 = q!;
                return await Task.FromResult(true).ConfigureAwait(true);
            });
            await _rabbitMQService.ReceiveAsync<string>(queueName2, async q =>
            {
                receivedMessagesQueue2 = q!;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            string message = "Broadcast message";
            await _rabbitMQService.SendAsync(exchangeName, message);

            await Task.Delay(500);
            Assert.Equal(message, receivedMessagesQueue1);
            Assert.Equal(message, receivedMessagesQueue2);
            await channel.QueueDeleteAsync(queueName1);
            await channel.QueueDeleteAsync(queueName2);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_Headers_Correctly()
        {
            string exchangeName = $"{nameof(SendSync_Headers_Correctly)}";
            string queueName = $"{nameof(SendSync_Headers_Correctly)}";

            var headers = new Dictionary<string, object?> { { "x-match", "all" }, { "type", "error" }, { "format", "json" }, { "x-expires", 6000 } };
            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Headers, "", headers);

            string message = "Headers matched message";
            await _rabbitMQService.SendAsync(exchangeName, message, "", headers);

            var receivedMessages = "";
            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async q =>
            {
                receivedMessages = q!;
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            await Task.Delay(500);
            Assert.Equal(message, receivedMessages);
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task Test_Topic_Correctly()
        {
            string exchangeName = $"{nameof(Test_Topic_Correctly)}";
            string queueName1 = $"{nameof(Test_Topic_Correctly)}q1";
            string queueName2 = $"{nameof(Test_Topic_Correctly)}q2";

            await _rabbitMQService.QueueDeclareAsync(queueName1, exchangeName, ExchangeType.Topic, "logs.error.#", _header);
            await _rabbitMQService.QueueDeclareAsync(queueName2, exchangeName, ExchangeType.Topic, "logs.*.database", _header);

            var receivedMessagesQueue1 = new List<string>();
            var receivedMessagesQueue2 = new List<string>();

            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName1, async q =>
            {
                receivedMessagesQueue1.Add(q!);
                return await Task.FromResult(true).ConfigureAwait(true);
            });
            await _rabbitMQService.ReceiveAsync<string>(queueName2, async q =>
            {
                receivedMessagesQueue2.Add(q!);
                return await Task.FromResult(true).ConfigureAwait(true);
            });

            string message1 = "Error in database";
            await _rabbitMQService.SendAsync(exchangeName, message1, "logs.error.database");

            string message2 = "Info in database";
            await _rabbitMQService.SendAsync(exchangeName, message2, "logs.info.database");

            await Task.Delay(500);
            Assert.Contains(message1, receivedMessagesQueue1);
            Assert.Contains(message1, receivedMessagesQueue2);
            Assert.DoesNotContain(message2, receivedMessagesQueue1);
            Assert.Contains(message2, receivedMessagesQueue2);
            await channel.QueueDeleteAsync(queueName1);
            await channel.QueueDeleteAsync(queueName2);
            await channel.ExchangeDeleteAsync(exchangeName);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        [Fact]
        public async Task SendSync_MultipleExchange_Correctly()
        {
            string exchangeName1 = $"{nameof(SendSync_MultipleExchange_Correctly)}1";
            string exchangeName2 = $"{nameof(SendSync_MultipleExchange_Correctly)}2";
            string queueName = $"{nameof(SendSync_MultipleExchange_Correctly)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName1, ExchangeType.Direct, "", _header);
            await _rabbitMQService.ExchangeBindAsync(exchangeName1, exchangeName2, ExchangeType.Direct, "", _header);

            string message = "This is an error message";
            await _rabbitMQService.SendAsync(exchangeName2, message);

            var receivedMessages = "";
            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async q =>
            {
                receivedMessages = q;
                return await Task.FromResult(true).ConfigureAwait(true);
            });
            await Task.Delay(500);
            Assert.Equal(message, receivedMessages);
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName1);
            await channel.ExchangeDeleteAsync(exchangeName2);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }
    }
}