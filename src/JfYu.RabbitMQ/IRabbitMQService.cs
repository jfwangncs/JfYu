using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.RabbitMQ
{
    /// <summary>
    /// Service interface for RabbitMQ operations including queue/exchange management, message publishing, and async consuming.
    /// Provides high-level abstractions over RabbitMQ.Client with built-in retry logic and dead letter queue support.
    /// </summary>
    public interface IRabbitMQService
    {
        /// <summary>
        /// Gets the persistent RabbitMQ connection shared across all operations.
        /// This connection is established during service registration and managed by the DI container.
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// Binds a source exchange to a destination exchange with optional routing key and headers.
        /// Useful for creating exchange-to-exchange routing topologies.
        /// </summary>
        /// <param name="destination">The destination exchange name that will receive messages.</param>
        /// <param name="source">The source exchange name that will forward messages.</param>
        /// <param name="exchangeType">The type of exchange (Direct, Fanout, Topic, Headers). See <see cref="ExchangeType"/>.</param>
        /// <param name="routingKey">Optional routing key for Direct and Topic exchanges. Empty for Fanout.</param>
        /// <param name="headers">Optional headers for Headers exchange type. Must be provided if exchangeType is "headers".</param>
        /// <returns>A task that completes when the binding is established.</returns>
        Task ExchangeBindAsync(string destination, string source, string exchangeType= ExchangeType.Direct, string routingKey = "", IDictionary<string, object?>? headers = null);

        /// <summary>
        /// Declares a queue and optionally binds it to an exchange.
        /// Creates the queue, exchange (if name provided), and establishes the binding in a single operation.
        /// </summary>
        /// <param name="queueName">The name of the queue to declare.</param>
        /// <param name="exchangeName">Optional exchange name. If empty, only declares the queue without binding.</param>
        /// <param name="exchangeType">The type of exchange (Direct, Fanout, Topic, Headers). See <see cref="ExchangeType"/>.</param>
        /// <param name="routingKey">Optional routing key for queue-to-exchange binding.</param>
        /// <param name="headers">Optional queue arguments (e.g., x-dead-letter-exchange, x-message-ttl, x-expires). For Headers exchange, must include binding headers.</param>
        /// <returns>A QueueDeclareOk result containing queue metadata (name, message count, consumer count).</returns>
        /// <remarks>
        /// Common header arguments:
        /// - x-dead-letter-exchange: DLX for failed messages
        /// - x-dead-letter-routing-key: Routing key for DLQ
        /// - x-message-ttl: Message time-to-live in milliseconds
        /// - x-expires: Queue auto-deletion timeout
        /// </remarks>
        Task<QueueDeclareOk> QueueDeclareAsync(string queueName, string exchangeName = "", string exchangeType = ExchangeType.Direct, string routingKey = "", IDictionary<string, object?>? headers = null);

        /// <summary>
        /// Publishes a single message to an exchange with publisher confirms.
        /// Supports automatic retry logic based on MessageOptions configuration.
        /// </summary>
        /// <typeparam name="T">The message type. Will be JSON-serialized before publishing.</typeparam>
        /// <param name="exchangeName">The exchange name to publish to.</param>
        /// <param name="message">The message object to publish.</param>
        /// <param name="routingKey">Optional routing key for message routing.</param>
        /// <param name="headers">Optional message headers (e.g., custom routing headers for Headers exchange).</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
        /// <returns>A task that completes when the message is confirmed by the broker.</returns>
        /// <remarks>
        /// The message is automatically tagged with retry metadata headers (x-retry-count).
        /// Publisher confirms are enabled to ensure reliable delivery.
        /// </remarks>
        Task SendAsync<T>(string exchangeName, T message, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes multiple messages to an exchange in batches with publisher confirms.
        /// More efficient than multiple SendAsync calls for bulk operations.
        /// </summary>
        /// <typeparam name="T">The message type. Each message will be JSON-serialized.</typeparam>
        /// <param name="exchangeName">The exchange name to publish to.</param>
        /// <param name="messages">The collection of messages to publish.</param>
        /// <param name="routingKey">Optional routing key applied to all messages.</param>
        /// <param name="headers">Optional headers applied to all messages.</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
        /// <returns>A task that completes when all messages are confirmed by the broker.</returns>
        /// <remarks>
        /// Messages are published in batches according to MessageOptions.BatchSize.
        /// Publisher confirms are awaited after each batch for reliability.
        /// Outstanding confirms are throttled by MessageOptions.MaxOutstandingConfirms.
        /// </remarks>
        Task SendBatchAsync<T>(string exchangeName, IList<T> messages, string routingKey = "", IDictionary<string, object?>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts an asynchronous consumer for the specified queue with automatic acknowledgement and retry handling.
        /// Messages are processed by the provided async function, with automatic retry on failure up to MaxRetryCount.
        /// Failed messages beyond retry limit are sent to dead letter queue if configured.
        /// </summary>
        /// <typeparam name="T">The message type to deserialize from JSON.</typeparam>
        /// <param name="queueName">The queue name to consume from.</param>
        /// <param name="func">Async function to process each message. Return true to ACK, false to trigger retry/DLQ logic.</param>
        /// <param name="prefetchCount">Number of unacknowledged messages the consumer can have (QoS setting). Higher values increase throughput but use more memory.</param>
        /// <param name="cancellationToken">Cancellation token to stop the consumer and dispose the channel gracefully.</param>
        /// <returns>The IChannel used for consuming. Dispose or cancel the token to stop consuming.</returns>
        /// <remarks>
        /// Retry behavior:
        /// - Messages are retried up to MessageOptions.MaxRetryCount times
        /// - Retry delay is MessageOptions.RetryDelayMilliseconds between attempts
        /// - Retry count is tracked in x-retry-count header
        /// - Messages exceeding retry limit are sent to dead letter exchange if configured on the queue
        /// - If no DLX configured, messages exceeding retry limit are discarded
        /// 
        /// The consumer continues until cancellationToken is triggered or the channel is disposed.
        /// </remarks>
        Task<IChannel> ReceiveAsync<T>(string queueName, Func<T?, Task<bool>> func, ushort prefetchCount = 1, CancellationToken cancellationToken = default);

    }
}