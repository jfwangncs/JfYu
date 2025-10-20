using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace JfYu.RabbitMQ
{
    /// <summary>
    ///  Adds RabbitMQ services extensions
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Adds RabbitMQ services to the IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="configure">An action to configure the ConnectionFactory and MessageRetryPolicy.</param>
        /// <returns>The IServiceCollection with the RabbitMQ services added.</returns>
        /// <exception cref="ArgumentNullException">Thrown when configureConnectionFactory is null.</exception>
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