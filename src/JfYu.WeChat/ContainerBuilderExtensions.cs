using JfYu.Request.Extension;
using JfYu.WeChat.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JfYu.WeChat
{
    public static class ContainerBuilderExtensions
    {

        public static void AddMiniProgram(this IServiceCollection services, Action<MiniProgramOptions>? setupAction)
        {
            services.Configure<MiniProgramOptions>(opts => setupAction?.Invoke(opts));
            services.AddScoped<IMiniProgram, MiniProgram>();
            services.AddJfYuHttpRequest();
        }
    }
}
