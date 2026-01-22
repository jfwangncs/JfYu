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
        /// Registers WeChat Mini Program services with dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="setupAction">Configuration action for Mini Program options (AppId, Secret)</param>
        public static void AddMiniProgram(this IServiceCollection services, Action<MiniProgramOptions>? setupAction)
        {
            services.Configure<MiniProgramOptions>(opts => setupAction?.Invoke(opts));
            services.AddScoped<IMiniProgram, MiniProgram>();
            services.AddJfYuHttpRequest();
        }
    }
}
