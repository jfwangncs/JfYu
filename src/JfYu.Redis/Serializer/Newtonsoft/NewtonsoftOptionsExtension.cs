using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace JfYu.Redis.Serializer.Newtonsoft
{
    /// <summary>
    /// Newtonsoft Options Extension
    /// </summary>
    internal sealed class NewtonsoftOptionsExtension(Action<JsonSerializerSettings>? _configure) : ISerializerOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<JsonSerializerSettings>? configure = _configure;

        /// <summary>
        ///  Adds Newtonsoft Serializer.
        /// </summary>
        /// <param name="services">services</param>
        public void AddServices(IServiceCollection services)
        {
            var options = new JsonSerializerSettings();
            configure?.Invoke(options);
            services.AddSingleton<ISerializer>(new NewtonsoftSerializer(options));
        }
    }
}