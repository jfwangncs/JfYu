using System;
using System.Threading.Tasks;

namespace JfYu.Redis.Interface
{
    /// <summary>
    /// The interface for the Redis Pub/Sub service.
    /// </summary>
    public partial interface IRedisService
    {
        /// <summary>
        /// Publishes a message to the specified channel.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="message">The message to publish.</param>
        /// <returns>The number of clients that received the message.</returns>
        Task<long> PublishAsync<T>(string channel, T message);

        /// <summary>
        /// Subscribes to a channel and invokes the handler for each received message.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="handler">The handler to invoke when a message is received.</param>
        /// <returns>A task representing the subscription.</returns>
        Task SubscribeAsync<T>(string channel, Action<string, T?> handler);

        /// <summary>
        /// Subscribes to a channel pattern and invokes the handler for each received message.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="channelPattern">The channel pattern (e.g., "news.*").</param>
        /// <param name="handler">The handler to invoke when a message is received.</param>
        /// <returns>A task representing the subscription.</returns>
        Task SubscribePatternAsync<T>(string channelPattern, Action<string, T?> handler);

        /// <summary>
        /// Unsubscribes from the specified channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        /// <returns>A task representing the unsubscribe operation.</returns>
        Task UnsubscribeAsync(string channel);

        /// <summary>
        /// Unsubscribes from all channels.
        /// </summary>
        /// <returns>A task representing the unsubscribe operation.</returns>
        Task UnsubscribeAllAsync();

        /// <summary>
        /// Unsubscribes from the specified channel pattern.
        /// </summary>
        /// <param name="channelPattern">The channel pattern.</param>
        /// <returns>A task representing the unsubscribe operation.</returns>
        Task UnsubscribePatternAsync(string channelPattern);
    }
}
