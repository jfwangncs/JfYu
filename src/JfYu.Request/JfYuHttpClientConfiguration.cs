#if NET8_0_OR_GREATER
namespace JfYu.Request
{
    /// <summary>
    /// Internal configuration class to store the registered HttpClient name.
    /// Used by JfYuHttpClient to retrieve the correct named HttpClient from IHttpClientFactory.
    /// </summary>
    public class JfYuHttpClientConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the HttpClient to use from IHttpClientFactory.
        /// </summary>
        public string HttpClientName { get; set; } = "JfYuHttpClient";
    }
}
#endif
