using JfYu.Redis.Extensions;
using JfYu.Redis.Interface;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;


namespace JfYu.Redis.Implementation
{
    /// <summary>
    /// The implementation of the Redis Pub/Sub service.
    /// </summary>
    public partial class RedisService : IRedisService
    {
        /// <inheritdoc/>
        public async Task<long> PublishAsync<T>(string channel, T message)
        {
#if NETSTANDARD2_0

            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(channel);
            ArgumentNullExceptionExtension.ThrowIfNull(message);

#else

            ArgumentException.ThrowIfNullOrWhiteSpace(channel);
            ArgumentNullException.ThrowIfNull(message);

#endif
            Log(nameof(PublishAsync), channel);

            var subscriber = _client.GetSubscriber();
            var serializedMessage = _serializer.Serialize(message);
            return await subscriber.PublishAsync(RedisChannel.Literal(channel), serializedMessage).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SubscribeAsync<T>(string channel, Action<string, T?> handler)
        {
#if NETSTANDARD2_0

            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(channel);
            ArgumentNullExceptionExtension.ThrowIfNull(handler);

#else

            ArgumentException.ThrowIfNullOrWhiteSpace(channel);
            ArgumentNullException.ThrowIfNull(handler);

#endif
            Log(nameof(SubscribeAsync), channel);

            var subscriber = _client.GetSubscriber();
            await subscriber.SubscribeAsync(RedisChannel.Literal(channel), (ch, value) =>
            {
                if (value.HasValue)
                    handler(ch!, _serializer.Deserialize<T>(value!));
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SubscribePatternAsync<T>(string channelPattern, Action<string, T?> handler)
        {
#if NETSTANDARD2_0

            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(channelPattern);
            ArgumentNullExceptionExtension.ThrowIfNull(handler);

#else

            ArgumentException.ThrowIfNullOrWhiteSpace(channelPattern);
            ArgumentNullException.ThrowIfNull(handler);

#endif
            Log(nameof(SubscribePatternAsync), channelPattern);

            var subscriber = _client.GetSubscriber();
            await subscriber.SubscribeAsync(new RedisChannel(channelPattern, RedisChannel.PatternMode.Pattern), (ch, value) =>
            {
                if (value.HasValue)
                    handler(ch!, _serializer.Deserialize<T>(value!));
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UnsubscribeAsync(string channel)
        {
#if NETSTANDARD2_0

            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(channel);

#else

            ArgumentException.ThrowIfNullOrWhiteSpace(channel);

#endif
            Log(nameof(UnsubscribeAsync), channel);

            var subscriber = _client.GetSubscriber();
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(channel)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UnsubscribeAllAsync()
        {
            Log(nameof(UnsubscribeAllAsync), "all channels");

            var subscriber = _client.GetSubscriber();
            await subscriber.UnsubscribeAllAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UnsubscribePatternAsync(string channelPattern)
        {
#if NETSTANDARD2_0

            ArgumentNullExceptionExtension.ThrowIfNullOrWhiteSpace(channelPattern);

#else

            ArgumentException.ThrowIfNullOrWhiteSpace(channelPattern);

#endif
            Log(nameof(UnsubscribePatternAsync), channelPattern);

            var subscriber = _client.GetSubscriber();
            await subscriber.UnsubscribeAsync(new RedisChannel(channelPattern, RedisChannel.PatternMode.Pattern)).ConfigureAwait(false);
        }
    }
}
