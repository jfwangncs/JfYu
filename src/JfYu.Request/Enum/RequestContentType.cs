namespace JfYu.Request.Enum
{
    /// <summary>
    /// Request header default values.
    /// </summary>
    public static class RequestContentType
    {
        /// <summary>
        /// Plain.
        /// </summary>
        public static string Plain { get; } = "text/plain";

        /// <summary>
        /// Xml.
        /// </summary>
        public static string Xml { get; } = "application/xml";

        /// <summary>
        /// Json.
        /// </summary>
        public static string Json { get; } = "application/json";

        /// <summary>
        /// FormUrlEncoded.
        /// </summary>
        public static string FormUrlEncoded { get; } = "application/x-www-form-urlencoded";

        /// <summary>
        /// FormData.
        /// </summary>
        public static string FormData { get; } = "multipart/form-data";
    }
}