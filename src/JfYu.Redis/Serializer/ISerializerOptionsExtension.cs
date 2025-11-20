using Microsoft.Extensions.DependencyInjection;

namespace JfYu.Redis.Serializer
{
    /// <summary>
    /// Serializer Options Extension.
    /// </summary>
    public interface ISerializerOptionsExtension
    {
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        void AddServices(IServiceCollection services);
    }
}