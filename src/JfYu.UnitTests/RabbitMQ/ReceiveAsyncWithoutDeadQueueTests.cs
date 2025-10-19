using JfYu.RabbitMQ;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace JfYu.UnitTests.RabbitMQ
{
    public class ReceiveAsyncWithoutDeadQueueTests
    {
        private readonly IRabbitMQService _rabbitMQService;
        public ReceiveAsyncWithoutDeadQueueTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddRabbitMQServices().BuildServiceProvider();
            _rabbitMQService = serviceProvider.GetRequiredService<IRabbitMQService>();
        }

        [Fact]
        public async Task ReceiveAsyncT_Successful()
        {
            const string exchangeName = $"{nameof(ReceiveAsyncT_Successful)}";
            const string queueName = $"{nameof(ReceiveAsyncT_Successful)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct);
            TestModel? receivedMessages = null;
            var channel = await _rabbitMQService.ReceiveAsync<TestModel>(queueName, async q =>
             {
                 await Task.Delay(1).ConfigureAwait(true);
                 receivedMessages = q;
                 return true;
             });
            var message = new TestModelFaker().Generate(1).FirstOrDefault();

            await _rabbitMQService.SendAsync(exchangeName, message);

            await Task.Delay(1000);
            Assert.Equal(JsonConvert.SerializeObject(message), JsonConvert.SerializeObject(receivedMessages));
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
        }
        [Fact]
        public async Task ReceiveAsyncString_Successful()
        {
            const string exchangeName = $"{nameof(ReceiveAsyncString_Successful)}";
            const string queueName = $"{nameof(ReceiveAsyncString_Successful)}";

            await _rabbitMQService.QueueDeclareAsync(queueName, exchangeName, ExchangeType.Direct);
            string? receivedMessages = null;
            var channel = await _rabbitMQService.ReceiveAsync<string>(queueName, async q =>
            {
                await Task.Delay(1).ConfigureAwait(true);
                receivedMessages = q;
                return true;
            });
            var message = new TestModelFaker().Generate(1).FirstOrDefault();

            await _rabbitMQService.SendAsync(exchangeName, message);

            await Task.Delay(1000);
            Assert.Equal(JsonConvert.SerializeObject(message), receivedMessages);
            await channel.QueueDeleteAsync(queueName);
            await channel.ExchangeDeleteAsync(exchangeName);
        }
    }
}
