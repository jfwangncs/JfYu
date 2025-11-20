using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JfYu.Redis.Serializer.MessagePack
{
    /// <summary>
    /// MessagePack Options Extension
    /// </summary>
    internal sealed class MessagePackOptionsExtension(Action<MessagePackSerializerOptions>? _configure) : ISerializerOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<MessagePackSerializerOptions>? configure = _configure;

        /// <summary>
        ///  Adds MessagePack Serializer.
        /// </summary>
        /// <param name="services">services</param>
        public void AddServices(IServiceCollection services)
        {
            var options = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);
            configure?.Invoke(options);
            services.AddSingleton<ISerializer>(new MsgPackObjectSerializer(options));
        }
    }
}