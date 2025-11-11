namespace JfYu.RabbitMQ
{
    /// <summary>
    /// Configuration options for RabbitMQ message publishing, retry logic, and batch operations.
    /// Applied globally to all RabbitMQService operations when configured in service registration.
    /// </summary>
    public class MessageOptions
    {      
        /// <summary>
        /// Gets or sets the maximum number of retry attempts before sending a message to the dead letter queue.
        /// Default value is 3. Set to 0 to disable retries and send failed messages directly to DLQ.
        /// </summary>
        /// <remarks>
        /// Retry count is tracked using the x-retry-count message header.
        /// When a message exceeds this count:
        /// - If the queue has x-dead-letter-exchange configured, the message is sent to the DLQ
        /// - If no DLX is configured, the message is discarded
        /// </remarks>
        public int MaxRetryCount { get; set; } = 3;    

        /// <summary>
        /// Gets or sets the delay in milliseconds before retrying a failed message delivery.
        /// Default value is 5000ms (5 seconds). Applied between each retry attempt.
        /// </summary>
        /// <remarks>
        /// The delay is implemented using Task.Delay() before requeuing the message.
        /// Consider increasing this value for transient errors like temporary network issues.
        /// Minimum recommended value: 1000ms to avoid overwhelming the broker.
        /// </remarks>
        public int RetryDelayMilliseconds { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the maximum number of unconfirmed published messages allowed before blocking.
        /// Default value is 1000. Used with publisher confirms to prevent memory exhaustion.
        /// </summary>
        /// <remarks>
        /// Publisher confirms are enabled automatically by the service.
        /// When outstanding confirms reach this limit, the publisher will wait for confirmations
        /// before sending more messages. This prevents unbounded memory growth during high-throughput scenarios.
        /// Increase for higher throughput, decrease for lower memory usage.
        /// </remarks>
        public int MaxOutstandingConfirms { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the number of messages to publish in each batch when using SendBatchAsync.
        /// Default value is 20. Larger batches improve throughput but increase memory usage.
        /// </summary>
        /// <remarks>
        /// Applies only to SendBatchAsync operations. Each batch waits for publisher confirms
        /// before proceeding to the next batch.
        /// Recommended range: 10-100 depending on message size and broker capacity.
        /// </remarks>
        public int BatchSize { get; set; } = 20;

    }
}