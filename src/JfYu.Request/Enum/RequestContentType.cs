namespace JfYu.Request.Enum
{
    /// <summary>
    /// Provides standard Content-Type header values for HTTP requests.
    /// Use these constants when setting the <see cref="IJfYuRequest.ContentType"/> property.
    /// </summary>
    public static class RequestContentType
    {
        /// <summary>
        /// Plain text content type: "text/plain".
        /// Used for unformatted text data.
        /// </summary>
        /// <example>
        /// client.ContentType = RequestContentType.Plain;
        /// client.RequestData = "username=testUser&amp;age=30";
        /// </example>
        public static string Plain { get; } = "text/plain";

        /// <summary>
        /// XML content type: "application/xml".
        /// Used for XML-formatted data.
        /// </summary>
        /// <example>
        /// client.ContentType = RequestContentType.Xml;
        /// client.RequestData = "&lt;user&gt;&lt;username&gt;testUser&lt;/username&gt;&lt;/user&gt;";
        /// </example>
        public static string Xml { get; } = "application/xml";

        /// <summary>
        /// JSON content type: "application/json".
        /// Default content type for most modern REST APIs.
        /// </summary>
        /// <example>
        /// client.ContentType = RequestContentType.Json;
        /// client.RequestData = "{\"username\":\"testUser\",\"age\":30}";
        /// </example>
        public static string Json { get; } = "application/json";

        /// <summary>
        /// URL-encoded form content type: "application/x-www-form-urlencoded".
        /// Used for standard HTML form submissions with key-value pairs.
        /// </summary>
        /// <example>
        /// client.ContentType = RequestContentType.FormUrlEncoded;
        /// client.RequestData = "username=testUser&amp;age=30";
        /// </example>
        public static string FormUrlEncoded { get; } = "application/x-www-form-urlencoded";

        /// <summary>
        /// Multipart form data content type: "multipart/form-data".
        /// Required for file uploads and mixed form data with files.
        /// </summary>
        /// <example>
        /// client.ContentType = RequestContentType.FormData;
        /// client.RequestData = "username=testUser";
        /// client.Files = new Dictionary&lt;string, string&gt; { { "file", "path/to/file.txt" } };
        /// </example>
        public static string FormData { get; } = "multipart/form-data";
    }
}