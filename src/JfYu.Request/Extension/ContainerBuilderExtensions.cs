using JfYu.Request.Logs;
using Microsoft.Extensions.DependencyInjection;
using System;

#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
#endif

namespace JfYu.Request.Extension
{
    /// <summary>
    /// Provides extension methods for registering JfYu.Request HTTP services in the dependency injection container.
    /// Supports both legacy HttpWebRequest and modern HttpClient implementations.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the legacy HttpWebRequest-based HTTP client service.
        /// Use this for scenarios requiring WebProxy, direct SSL certificate control, or compatibility with older .NET Framework code.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="filter">Optional action to configure logging filters for request/response data sanitization and field selection.</param>
        /// <remarks>
        /// This implementation uses HttpWebRequest which is available in all .NET versions but is considered legacy.
        /// Features specific to this implementation:
        /// - Direct WebProxy support via Proxy property
        /// - X509Certificate2 SSL certificates via Certificate property
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddJfYuHttpRequest(filter =>
        /// {
        ///     filter.LoggingFields = JfYuLoggingFields.All;
        ///     filter.RequestFilter = req => SanitizePasswords(req);
        ///     filter.ResponseFilter = resp => RemoveSensitiveData(resp);
        /// });
        /// </code>
        /// </example>
        public static void AddJfYuHttpRequest(this IServiceCollection services, Action<LogFilter>? filter = null)
        {
            services.AddScoped<IJfYuRequest, JfYuHttpRequest>();
            var logFilter = new LogFilter();
            filter?.Invoke(logFilter);
            services.AddSingleton(logFilter);
        }

#if NET8_0_OR_GREATER

        /// <summary>
        /// Registers the modern HttpClient-based HTTP service with HttpClientFactory support.
        /// Recommended for .NET 8+ applications due to better connection pooling, handler lifetime management, and testability.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="httpClientHandler">Optional factory function to provide a custom HttpClientHandler. If null, uses a default handler with cookie management and automatic decompression.</param>
        /// <param name="filter">Optional action to configure logging filters for request/response data sanitization and field selection.</param>
        /// <remarks>
        /// Default configuration when httpClientHandler is null:
        /// - Automatic cookie management via CookieContainer (shared singleton)
        /// - Automatic decompression for GZip, Deflate, and Brotli
        /// - HttpClientFactory integration for proper handler lifecycle and connection pooling
        /// 
        /// For proxy or SSL certificates with HttpClient, configure via the HttpClientHandler:
        /// <code>
        /// services.AddJfYuHttpClient(() => new HttpClientHandler
        /// {
        ///     Proxy = new WebProxy("http://proxy:8080"),
        ///     UseProxy = true,
        ///     ClientCertificates = { myCertificate },
        ///     ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true // for testing only
        /// });
        /// </code>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Default configuration
        /// services.AddJfYuHttpClient();
        /// 
        /// // With custom handler and logging
        /// services.AddJfYuHttpClient(
        ///     () => new HttpClientHandler { UseCookies = true },
        ///     filter =>
        ///     {
        ///         filter.LoggingFields = JfYuLoggingFields.RequestUrl | JfYuLoggingFields.ResponseStatus;
        ///         filter.RequestFilter = r => r; // custom sanitization
        ///     }
        /// );
        /// </code>
        /// </example>
        public static void AddJfYuHttpClient(this IServiceCollection services, Func<HttpClientHandler>? httpClientHandler = null, Action<LogFilter>? filter = null)
        {
            var build = services.AddHttpClient("httpclient");
            services.AddSingleton<CookieContainer>();
            if (httpClientHandler is not null)
                build.ConfigurePrimaryHttpMessageHandler(httpClientHandler);
            else
            {
                build.ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var cookieContainer = sp.GetRequiredService<CookieContainer>();
                    return new HttpClientHandler
                    {
                        CookieContainer = cookieContainer,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                    };
                });
            }

            services.AddScoped<IJfYuRequest, JfYuHttpClient>();
            var logFilter = new LogFilter();
            filter?.Invoke(logFilter);
            services.AddSingleton(logFilter);
        }
#endif
    }
}