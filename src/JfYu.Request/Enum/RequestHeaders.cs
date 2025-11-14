namespace JfYu.Request.Enum
{
    /// <summary>
    /// Contains default values for common HTTP request headers.
    /// Provides sensible defaults for browser-like HTTP requests.
    /// Use with <see cref="IJfYuRequest.RequestHeader"/> property.
    /// </summary>
    public class RequestHeaders
    {
        /// <summary>
        /// Gets or sets the Accept header, indicating which content types the client can process.
        /// Default value accepts HTML, XHTML, XML, and images with quality preferences.
        /// </summary>
        public string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

        /// <summary>
        /// Gets or sets the Accept-Encoding header, specifying supported compression algorithms.
        /// Default value supports gzip, deflate, and Brotli compression.
        /// </summary>
        public string AcceptEncoding { get; set; } = "gzip, deflate, br";

        /// <summary>
        /// Gets or sets the Accept-Language header, indicating preferred response languages.
        /// Default value prefers Chinese (zh-CN) followed by English.
        /// </summary>
        public string AcceptLanguage { get; set; } = "zh-CN,zh;q=0.9,en;q=0.8";

        /// <summary>
        /// Gets or sets the Cache-Control header for caching directives.
        /// Default value is "no-cache" to bypass browser caching.
        /// </summary>
        public string CacheControl { get; set; } = "no-cache";

        /// <summary>
        /// Gets or sets the Connection header to control persistent connections.
        /// Default value is "keep-alive" to reuse TCP connections.
        /// </summary>
        public string Connection { get; set; } = "keep-alive";

        /// <summary>
        /// Gets or sets the Host header, specifying the target server's domain name.
        /// Usually set automatically by the HTTP client based on the URL.
        /// </summary>
        public string Host { get; set; } = "";

        /// <summary>
        /// Gets or sets the Pragma header for HTTP/1.0 cache compatibility.
        /// Default value is "no-cache". Legacy header superseded by Cache-Control.
        /// </summary>
        public string Pragma { get; set; } = "no-cache";

        /// <summary>
        /// Gets or sets the Referer header, indicating the URL of the referring page.
        /// Used for analytics and security checks. Empty by default.
        /// </summary>
        public string Referer { get; set; } = "";

        /// <summary>
        /// Gets or sets the User-Agent header, identifying the client application.
        /// Default value mimics Chrome 68 on Windows to ensure compatibility with most servers.
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
    }
}