namespace JfYu.RabbitMQ
{
    /// <summary>
    /// Rabbit MQ message retry policy.
    /// </summary>
    public class MessageOptions
    {      
        /// <summary>
        /// Maximum number of retry attempts before sending to the dead letter queue.
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;    

        /// <summary>
        /// Delay time before retrying message delivery.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 5000;

        /// <summary>
        /// Maximum number of outstanding confirms allowed.
        /// </summary>
        public int MaxOutstandingConfirms { get; set; } = 1000;

        /// <summary>
        /// Batch size for publishing messages in bulk.
        /// </summary>
        public int BatchSize { get; set; } = 20;

    }
}