using MessagePack;
using System.Collections.Generic;

namespace JfYu.Redis.Serializer.MessagePack
{
    /// <summary>
    /// MsgPack implementation of <see cref="ISerializer"/>
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MsgPackObjectSerializer"/> class.
    /// </remarks>
    /// <param name="settings">The settings.</param>
    public class MsgPackObjectSerializer(MessagePackSerializerOptions settings) : ISerializer
    {
        private readonly MessagePackSerializerOptions _settings = settings;

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] serializedObject)
        {
            return MessagePackSerializer.Deserialize<T>(serializedObject, _settings);
        }

        /// <inheritdoc/>
        public byte[] Serialize<T>(T? value)
        {
            if (EqualityComparer<T?>.Default.Equals(value, default))
                return [];

            return MessagePackSerializer.Serialize(value!.GetType(), value, _settings);
        }
    }
}