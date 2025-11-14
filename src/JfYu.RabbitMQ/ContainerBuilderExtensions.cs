using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace JfYu.RabbitMQ
{
    /// <summary>
    /// Provides extension methods for registering JfYu.RabbitMQ services in the dependency injection container.
    /// Configures RabbitMQ connection, message options, and service registration.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers RabbitMQ services including connection factory, persistent connection, message options, and IRabbitMQService.
        /// The connection is established immediately during service registration and reused throughout the application lifetime.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="configure">Action to configure both the RabbitMQ ConnectionFactory and MessageOptions (retry policy, batch settings, etc.).</param>
        /// <returns>The IServiceCollection with RabbitMQ services registered, for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the configure action is null.</exception>
        /// <remarks>
        /// This method registers:
        /// - ConnectionFactory (singleton) - RabbitMQ connection configuration
        /// - IConnection (singleton) - Persistent connection to RabbitMQ server
        /// - MessageOptions (singleton) - Message retry and batch processing configuration
        /// - IRabbitMQService (scoped) - Service for queue/exchange operations and messaging
        /// 
        /// The IConnection is created synchronously during registration, which may block briefly.
        /// Ensure RabbitMQ server is accessible at registration time.
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddRabbitMQ((factory, options) =>
        /// {
        ///     // Configure connection
        ///     factory.HostName = "localhost";
        ///     factory.Port = 5672;
        ///     factory.UserName = "guest";
        ///     factory.Password = "guest";
        ///     factory.VirtualHost = "/";
        ///     factory.DispatchConsumersAsync = true;
        ///     
        ///     // Configure message options
        ///     options.MaxRetryCount = 3;
        ///     options.RetryDelayMilliseconds = 5000;
        ///     options.MaxOutstandingConfirms = 1000;
        ///     options.BatchSize = 20;
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<ConnectionFactory, MessageOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure), "The configureConnectionFactory action cannot be null.");

            var factory = new ConnectionFactory();
            var messageOption = new MessageOptions();
            configure(factory, messageOption);
            services.AddSingleton(factory);
            services.AddSingleton(factory.CreateConnectionAsync().Result);
            services.AddSingleton(messageOption);
            services.AddScoped<IRabbitMQService, RabbitMQService>();
            return services;
        }
    }
}