namespace JfYu.WeChat.Options
{
    /// <summary>
    /// Configuration options for WeChat Mini Program
    /// </summary>
    public class MiniProgramOptions
    {
        /// <summary>
        /// Mini Program AppId (Application ID)
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// Mini Program AppSecret (Application Secret Key)
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether HTTP request and response information is logged.
        /// </summary>
        /// <remarks>Enable this property to assist with debugging or monitoring HTTP traffic. Logging
        /// HTTP data may include sensitive information; ensure that logs are handled securely and comply with privacy
        /// requirements.</remarks>
        public bool EnableHttpLogging { get; set; }
    }
}
