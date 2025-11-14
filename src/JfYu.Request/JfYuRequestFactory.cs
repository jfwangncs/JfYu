#if NET8_0_OR_GREATER
using JfYu.Request.Logs;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;

namespace JfYu.Request
{
    /// <summary>
    /// Factory implementation for creating IJfYuRequest instances with different named HttpClient configurations.
    /// </summary>
#pragma warning disable CA1812 // Class is instantiated via dependency injection
    internal sealed class JfYuRequestFactory : IJfYuRequestFactory
#pragma warning restore CA1812
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CookieContainer? _cookieContainer;
        private readonly LogFilter _logFilter;
        private readonly ILogger<JfYuHttpClient>? _logger;
        private readonly JfYuHttpClientConfiguration _defaultConfiguration;

        /// <summary>
        /// Initializes a new instance of the JfYuRequestFactory class.
        /// </summary>
        public JfYuRequestFactory(
            IHttpClientFactory httpClientFactory,
            JfYuHttpClientConfiguration defaultConfiguration,
            CookieContainer? cookieContainer,
            LogFilter logFilter,
            ILogger<JfYuHttpClient>? logger = null)
        {
            _httpClientFactory = httpClientFactory;
            _defaultConfiguration = defaultConfiguration;
            _cookieContainer = cookieContainer;
            _logFilter = logFilter;
            _logger = logger;
        }

        /// <inheritdoc/>
        public IJfYuRequest CreateRequest()
        {
            return new JfYuHttpClient(_httpClientFactory, _defaultConfiguration, _cookieContainer, _logFilter, _logger);
        }

        /// <inheritdoc/>
        public IJfYuRequest CreateRequest(string httpClientName)
        {
            var config = new JfYuHttpClientConfiguration { HttpClientName = httpClientName };
            return new JfYuHttpClient(_httpClientFactory, config, _cookieContainer, _logFilter, _logger);
        }
    }
}
#endif
