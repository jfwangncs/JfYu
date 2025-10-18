using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.RabbitMQ
{
    /// <summary>
    /// RabbitMQService interface.
    /// </summary>
    public interface IRabbitMQService
    {
        /// <summary>
        /// Rabbit MQ connection.
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// Binds an exchange to another exchange with a routing key.
        /// </summary>
        /// <param name="destination">The destination exchange.</param>
        /// <param name="source">The source exchange.</param>
        /// <param name="exchangeType">The type of the exchange.<see cref="ExchangeType"/></param>
        /// <param name="routingKey">The routing key (optional).</param>
        /// <param name="headers">The headers (optional) if exchangeType="header" must be mandatory.</param>
        /// <returns>True if the binding is successful, otherwise false.</returns>
        Task ExchangeBindAsync(string destination, string source, string exchangeType= ExchangeType.Direct, string routingKey = "", IDictionary<string, object?>? headers = null);

        /// <summary>
        /// Binds a queue to an exchange with a routing key.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="exchangeType">The type of the exchange.<see cref="ExchangeType"/></param>
        /// <param name="routingKey">The routing key (optional).</param>
        /// <param name="headers">The headers (optional) if exchangeType="header" must be mandatory.</param>
        /// <returns>True if the binding is successful, otherwise false.</returns>
        Task<QueueDeclareOk> QueueDeclareAsync(string queueName, string exchangeName = "", string exchangeType = ExchangeType.Direct, string routingKey = "", IDictionary<string, object?>? headers = null);

        /// <summary>
        /// Sends a message to an exchange with a specific routing key.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="message">The messages to send.</param> 
        /// <param name="routingKey">The routing key (optional).</param>
        /// <param name="headers">The headers (optional) if exchangeType="header" must be mandatory.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        Task SendAsync<T>(string exchangeName, T message, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to an exchange with a specific routing key.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="messages">The messages to send.</param> 
        /// <param name="routingKey">The routing key (optional).</param>
        /// <param name="headers">The headers (optional) if exchangeType="header" must be mandatory.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        Task SendBatchAsync<T>(string exchangeName, IList<T> messages, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives messages from a queue asynchronously with a specified prefetch count.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="func">The async function to process the message.</param>
        /// <param name="prefetchCount">The number of messages to prefetch (default is 1).</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns> </returns>
        Task<IChannel> ReceiveAsync<T>(string queueName, Func<T?, Task<bool>> func, ushort prefetchCount = 1, CancellationToken cancellationToken = default);

    }
}