using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JfYu.RabbitMQ.Logs;

namespace JfYu.RabbitMQ
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="messageOption"></param>
    /// <param name="logger"></param>
    public class RabbitMQService(IConnection connection, MessageOptions messageOption, ILogger<RabbitMQService>? logger = null) : IRabbitMQService
    {
        /// <summary>
        /// Rabbit MQ connection.
        /// </summary>
        public IConnection Connection { get; } = connection;

        private const string xretrycount = "x-retry-count";
        private const string xexchangename = "x-exchange-name";
        private const string xexchangeroutingkey = "x-exchange-routing-key";
        private readonly MessageOptions _messageOption = messageOption;
        private readonly ILogger<RabbitMQService>? _logger = logger;

        /// <inheritdoc/>
        public async Task<QueueDeclareOk> QueueDeclareAsync(string queueName, string exchangeName = "", string exchangeType = ExchangeType.Direct, string routingKey = "", IDictionary<string, object?>? headers = null)
        {
            using var _channel = await Connection.CreateChannelAsync().ConfigureAwait(false);
            var queue = await _channel.QueueDeclareAsync(queueName, true, false, false, headers).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(exchangeName))
            {
                await _channel.ExchangeDeclareAsync(exchangeName, exchangeType, true).ConfigureAwait(false);
                await _channel.QueueBindAsync(queueName, exchangeName, routingKey, headers).ConfigureAwait(false);
            }
            return queue;
        }

        /// <inheritdoc/>
        public async Task ExchangeBindAsync(string destination, string source, string exchangeType = ExchangeType.Direct, string routingKey = "", IDictionary<string, object?>? headers = null)
        {
            using var _channel = await Connection.CreateChannelAsync().ConfigureAwait(false);
            await _channel.ExchangeDeclareAsync(destination, exchangeType, true).ConfigureAwait(false);
            await _channel.ExchangeDeclareAsync(source, exchangeType, true).ConfigureAwait(false);
            await _channel.ExchangeBindAsync(destination, source, routingKey, headers).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SendAsync<T>(string exchangeName, T message, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default) => await SendInternalAsync(exchangeName, routingKey, headers, cancellationToken, message).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task SendBatchAsync<T>(string exchangeName, IList<T> messages, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default) => await SendInternalAsync(exchangeName, routingKey, headers, cancellationToken, messages.ToArray()).ConfigureAwait(false);

        private async Task SendInternalAsync<T>(string exchangeName, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default, params T[] messages)
        {
            using var limiter = new ThrottlingRateLimiter(_messageOption.MaxOutstandingConfirms);
            var channelOpts = new CreateChannelOptions(true, true, limiter);
            using var _channel = await Connection.CreateChannelAsync(channelOpts, cancellationToken).ConfigureAwait(false);
            headers ??= new Dictionary<string, object?>();
            headers[xretrycount] = 0;
            headers[xexchangename] = exchangeName;
            headers[xexchangeroutingkey] = routingKey;
            var basicProperties = new BasicProperties
            {
                Persistent = true,
                Headers = headers
            };
            var publishTasks = new List<ValueTask>();

            for (int i = 0; i < messages.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var msg = messages[i];
                byte[] payload;
                if (msg is null)
                    payload = [];
                else if (msg is string str)
                    payload = Encoding.UTF8.GetBytes(str);
                else
                    payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
                ValueTask publishTask = _channel.BasicPublishAsync(exchangeName, routingKey, true, basicProperties, payload, cancellationToken);
                publishTasks.Add(publishTask);

                if (publishTasks.Count >= _messageOption.BatchSize)
                {
                    await Publishes(publishTasks).ConfigureAwait(false);
                }
            }

            if (publishTasks.Count > 0)
                await Publishes(publishTasks).ConfigureAwait(false);

            static async Task Publishes(List<ValueTask> publishTasks)
            {
                foreach (ValueTask pt in publishTasks)
                {
                    await pt.ConfigureAwait(false);
                }
                publishTasks.Clear();
            }
        }

        /// <inheritdoc/>
        public async Task<IChannel> ReceiveAsync<T>(string queueName, Func<T?, Task<bool>> func, ushort prefetchCount = 1, bool autoAck = false, CancellationToken cancellationToken = default)
        {
            var _channel = await Connection.CreateChannelAsync(null, cancellationToken).ConfigureAwait(false);
            await _channel.BasicQosAsync(0, prefetchCount, false, cancellationToken).ConfigureAwait(false);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            string consumerTag = null!;
            cancellationToken.Register(async () =>
            {
                if (consumerTag != null)
                    await _channel.BasicCancelAsync(consumerTag).ConfigureAwait(false);
                _channel.Dispose();
            });

            if (autoAck)
            {
                // Auto acknowledgement mode: messages are ACKed immediately, no retry logic
                consumer.ReceivedAsync += async (ch, ea) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    await func(ParsePayload<T>(ea.Body, Encoding.UTF8.GetString(ea.Body.ToArray()))).ConfigureAwait(false);
                };
            }
            else
            {
                // Manual acknowledgement mode: messages must be ACKed/NACKed, with retry logic
                consumer.ReceivedAsync += async (ch, ea) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    string message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    try
                    {
                        if (await func(ParsePayload<T>(ea.Body, message)).ConfigureAwait(false))
                            await _channel.BasicAckAsync(ea.DeliveryTag, false).ConfigureAwait(false);
                        else
                            await TryToMoveToDeadLetterQueue(ea, _channel, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, string.Format("Receive message have error.message:{0}", message));
                        await TryToMoveToDeadLetterQueue(ea, _channel, cancellationToken).ConfigureAwait(false);
                    }
                };
            }

            consumerTag = await _channel.BasicConsumeAsync(queueName, autoAck, consumer, cancellationToken).ConfigureAwait(false);
            return _channel;
        }
        private static T? ParsePayload<T>(ReadOnlyMemory<byte> body, string message)
        {
            if (body.Length == 0)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)string.Empty;

                return default;
            }

            if (typeof(T) == typeof(string))
                return (T?)(object)message;

            return JsonConvert.DeserializeObject<T>(message);
        }

        private async Task TryToMoveToDeadLetterQueue(BasicDeliverEventArgs ea, IChannel channel, CancellationToken cancellationToken = default)
        {
            if (ea.BasicProperties.Headers is null)
            {
                _logger?.LogWarning("This Message didn't have header,can't use retry algorithm.");
                await channel.BasicRejectAsync(ea.DeliveryTag, true, cancellationToken).ConfigureAwait(false);
                return;
            }

            try
            {
                int retryCount = -1;
                var originalRoutingKey = "";
                var originalExchangeName = "";
                if (ea.BasicProperties.Headers.TryGetValue(xretrycount, out object? value))
                    retryCount = Convert.ToInt32(value);
                if (ea.BasicProperties.Headers.TryGetValue(xexchangename, out object? value1) && value1 is byte[] bytes1 && bytes1.Length > 0)
                    originalExchangeName = Encoding.UTF8.GetString(bytes1);
                if (ea.BasicProperties.Headers.TryGetValue(xexchangeroutingkey, out object? value2) && value2 is byte[] bytes2 && bytes2.Length > 0)
                    originalRoutingKey = Encoding.UTF8.GetString(bytes2);

                if (retryCount == -1)
                {
                    _logger?.LogWarning("This Message didn't have x-retry-count header,can't use retry algorithm.");
                    await channel.BasicRejectAsync(ea.DeliveryTag, true, cancellationToken).ConfigureAwait(false);
                    return;
                }
                if (retryCount >= _messageOption.MaxRetryCount)
                {
                    //send to dead letter queue 
                    _logger?.LogWarning("This message exceeds the retry count and goes to a dead letter directly.");
                    await channel.BasicRejectAsync(ea.DeliveryTag, false, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    retryCount++;
                    ea.BasicProperties.Headers[xretrycount] = retryCount;
                    var basicProperties = new BasicProperties
                    {
                        Persistent = true,
                        Headers = ea.BasicProperties.Headers
                    };
                    await channel.BasicPublishAsync(originalExchangeName, originalRoutingKey, true, basicProperties, ea.Body, cancellationToken).ConfigureAwait(false);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "This Message retry encountered error.");
                throw;
            }
        }

    }
}