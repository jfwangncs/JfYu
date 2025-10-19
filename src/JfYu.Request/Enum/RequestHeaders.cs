namespace JfYu.Request.Enum
{
    /// <summary>
    /// Request header default values.
    /// </summary>
    public class RequestHeaders
    {
        /// <summary>
        /// Accept.
        /// </summary>
        public string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

        /// <summary>
        /// AcceptEncoding.
        /// </summary>
        public string AcceptEncoding { get; set; } = "gzip, deflate, br";

        /// <summary>
        /// AcceptLanguage.
        /// </summary>
        public string AcceptLanguage { get; set; } = "zh-CN,zh;q=0.9,en;q=0.8";

        /// <summary>
        /// CacheControl.
        /// </summary>
        public string CacheControl { get; set; } = "no-cache";

        /// <summary>
        /// Connection.
        /// </summary>
        public string Connection { get; set; } = "keep-alive";

        /// <summary>
        /// Host.
        /// </summary>
        public string Host { get; set; } = "";

        /// <summary>
        /// Pragma.
        /// </summary>
        public string Pragma { get; set; } = "no-cache";

        /// <summary>
        /// Referer.
        /// </summary>
        public string Referer { get; set; } = "";

        /// <summary>
        /// UserAgent.
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
    }
}