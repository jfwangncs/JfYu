using JfYu.Redis.Serializer;
using System.Collections.Generic;

namespace JfYu.Redis.Options
{
    /// <summary>
    /// The Redis options.
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Redis server endpoints
        /// </summary>
        public List<RedisEndPoint> EndPoints { get; set; } = [];

        /// <summary>
        /// Redis server password
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Redis server database index default:0
        /// </summary>

        public int DbIndex { get; set; }

        /// <summary>
        /// Redis server connect timeout, default:5000 Milliseconds
        /// </summary>

        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// SSL
        /// </summary>
        public bool SSL { get; set; }

        /// <summary>
        /// Prefix
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Enabled or disabled logs default:false
        /// </summary>
        public bool EnableLogs { get; set; }

        /// <summary>
        /// Serializer Options
        /// </summary>
        internal ISerializerOptionsExtension? SerializerOptions { get; set; }
    }

    /// <summary>
    /// The Redis endpoint.
    /// </summary>
    public class RedisEndPoint
    {
        /// <summary>
        /// Redis server host url
        /// </summary>
        public string Host { get; set; } = "";

        /// <summary>
        /// Redis server port
        /// </summary>
        public int Port { get; set; } = 6379;
    }
}