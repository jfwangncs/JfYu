namespace JfYu.Redis.Serializer
{
    /// <summary>
    /// Serializer implementation
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes
        /// </summary>
        ///<typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>Serialized object</returns>
        byte[] Serialize<T>(T? value);

        /// <summary>
        /// Deserializes
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified value
        /// </returns>
        T? Deserialize<T>(byte[] serializedObject);
    }
}