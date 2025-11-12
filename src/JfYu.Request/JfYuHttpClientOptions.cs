#if NET8_0_OR_GREATER
using System;
using System.Net;
using System.Net.Http;

namespace JfYu.Request
{
    /// <summary>
    /// Configuration options for registering named HttpClient instances with JfYu.Request.
    /// Allows customization of HttpClient behavior including handler configuration, cookie management, and client name.
    /// </summary>
    public class JfYuHttpClientOptions
    {
        /// <summary>
        /// Gets or sets the name for the HttpClient registration.
        /// This name is used with IHttpClientFactory to retrieve the configured client.
        /// Default value is "JfYuHttpClient".
        /// </summary>
        /// <remarks>
        /// Use different names to register multiple HttpClient configurations for different scenarios
        /// (e.g., "ApiClient", "ProxyClient", "AuthClient").
        /// </remarks>
        public string HttpClientName { get; set; } = "JfYuHttpClient";

        /// <summary>
        /// Gets or sets a factory function to create a custom HttpClientHandler.
        /// If null, a default handler with cookie management and automatic decompression will be used.
        /// </summary>
        /// <remarks>
        /// Use this to configure:
        /// - Proxy settings (UseProxy, Proxy)
        /// - SSL/TLS certificates (ClientCertificates, ServerCertificateCustomValidationCallback)
        /// - Cookie handling (UseCookies, CookieContainer)
        /// - Compression (AutomaticDecompression)
        /// - Timeouts and retries
        /// </remarks>
        /// <example>
        /// <code>
        /// HttpClientHandler = () => new HttpClientHandler
        /// {
        ///     Proxy = new WebProxy("http://proxy:8080"),
        ///     UseProxy = true,
        ///     AutomaticDecompression = DecompressionMethods.All
        /// };
        /// </code>
        /// </example>
        public Func<HttpClientHandler>? HttpClientHandler { get; set; }

        /// <summary>
        /// Gets or sets an action to configure the HttpClient after creation.
        /// Use this to set default headers, timeout, or other client-level settings.
        /// </summary>
        /// <example>
        /// <code>
        /// ConfigureClient = client =>
        /// {
        ///     client.Timeout = TimeSpan.FromMinutes(5);
        ///     client.DefaultRequestHeaders.Add("X-Api-Version", "v1");
        /// };
        /// </code>
        /// </example>
        public Action<HttpClient>? ConfigureClient { get; set; }

        /// <summary>
        /// Gets or sets whether to use a shared singleton CookieContainer across all requests.
        /// Default is true. Set to false if you want isolated cookie management per request.
        /// </summary>
        public bool UseSharedCookieContainer { get; set; } = true;
    }
}
#endif
