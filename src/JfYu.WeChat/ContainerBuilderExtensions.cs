using JfYu.Request.Enum;
using JfYu.Request.Extension;
using JfYu.WeChat.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JfYu.WeChat
{
    /// <summary>
    /// Extension methods for configuring WeChat Mini Program services
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Adds MiniProgram services and configuration to the specified service collection.
        /// </summary>
        /// <remarks>This method registers the <see cref="IMiniProgram"/> service with scoped lifetime and
        /// configures MiniProgram options. Call this method during application startup to enable MiniProgram
        /// functionality.</remarks>
        /// <param name="services">The service collection to which the MiniProgram services will be added. Cannot be null.</param>
        /// <param name="setupAction">An optional action to configure the MiniProgram options. If null, default options are used.</param>
        /// <returns>The original <see cref="IServiceCollection"/> instance with MiniProgram services registered.</returns>
        public static IServiceCollection AddMiniProgram(this IServiceCollection services, Action<MiniProgramOptions>? setupAction)
        {
            var options = new MiniProgramOptions();
            setupAction?.Invoke(options);

            services.Configure<MiniProgramOptions>(opts => setupAction?.Invoke(opts));
            services.AddScoped<IMiniProgram, MiniProgram>();
            services.AddJfYuHttpRequest(filter =>
            {
                filter.LoggingFields = options.EnableHttpLogging ? JfYuLoggingFields.All : JfYuLoggingFields.None;
            });
            return services;
        }
    }
}
