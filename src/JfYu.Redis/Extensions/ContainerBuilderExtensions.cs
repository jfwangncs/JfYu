using JfYu.Redis.Implementation;
using JfYu.Redis.Interface;
using JfYu.Redis.Options;
using JfYu.Redis.Serializer.MessagePack;
using JfYu.Redis.Serializer.Newtonsoft;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace JfYu.Redis.Extensions
{
    /// <summary>
    /// Adds Redis services extensions
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Adds Redis services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="setupAction">An action to configure RedisOptions.</param>
        /// <returns>The IServiceCollection with the added services.</returns>
        /// <exception cref="ArgumentNullException">Thrown when setupAction is null.</exception>
        /// <exception cref="NullReferenceException">Thrown when no endpoints are configured.</exception>
        public static IServiceCollection AddRedisService(this IServiceCollection services, Action<RedisOptions>? setupAction)
        {
            ArgumentNullException.ThrowIfNull(setupAction);

            var options = new RedisOptions();
            setupAction.Invoke(options);

            if (options.EndPoints.Count == 0)
                throw new ArgumentNullException(nameof(setupAction), "EndPoints cannot be empty.");

            var configurationOptions = new ConfigurationOptions()
            {
                Password = options.Password,
                ConnectTimeout = options.Timeout,
                KeepAlive = 60,
                AbortOnConnectFail = false,
                Ssl = options.SSL,
            };

            foreach (var endPoint in options.EndPoints)
            {
                configurationOptions.EndPoints.Add(endPoint.Host, endPoint.Port);
            }

            services.Configure(setupAction);

            services.AddSingleton(sp => new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions)).Value);

            services.AddScoped<IRedisService, RedisService>();

            // Check if ISerializer is already registered
            if (options.SerializerOptions == null)
                // If not, use Newtonsoft.Json as the default serializer
                new NewtonsoftOptionsExtension(null).AddServices(services);
            else
                // If it is, use the registered serializer
                options.SerializerOptions.AddServices(services);

            return services;
        }

        /// <summary>
        /// Configures Redis to use Newtonsoft.Json for serialization.
        /// </summary>
        /// <param name="services">The RedisOptions to configure.</param>
        /// <param name="setupAction">An optional action to configure JsonSerializerSettings.</param>
        public static void UsingNewtonsoft(this RedisOptions services, Action<JsonSerializerSettings>? setupAction = null)
        {
            services.SerializerOptions = new NewtonsoftOptionsExtension(setupAction);
        }

        /// <summary>
        /// Configures Redis to use MessagePack for serialization.
        /// </summary>
        /// <param name="services">The RedisOptions to configure.</param>
        /// <param name="setupAction">An optional action to configure MessagePackSerializerOptions.</param>
        public static void UsingMsgPack(this RedisOptions services, Action<MessagePackSerializerOptions>? setupAction = null)
        {
            services.SerializerOptions = new MessagePackOptionsExtension(setupAction);
        }
    }
}