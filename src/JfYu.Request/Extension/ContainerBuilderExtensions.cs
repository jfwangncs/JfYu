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
        /// Registers the modern HttpClient-based HTTP service with HttpClientFactory support and configurable options.
        /// Recommended for .NET 8+ applications due to better connection pooling, handler lifetime management, and testability.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="configureOptions">Optional action to configure HttpClient options including client name, handler, and client configuration.</param>
        /// <param name="filter">Optional action to configure logging filters for request/response data sanitization and field selection.</param>
        /// <remarks>
        /// This overload allows full customization of HttpClient behavior through JfYuHttpClientOptions.
        /// You can configure:
        /// - Custom HttpClient name for multiple client registrations
        /// - Custom HttpClientHandler for proxy, SSL, compression settings
        /// - Client-level configuration (timeout, default headers)
        /// - Cookie container sharing behavior
        /// 
        /// Default configuration when configureOptions is null:
        /// - HttpClient name: "JfYuHttpClient"
        /// - Automatic cookie management via shared CookieContainer singleton
        /// - Automatic decompression for GZip, Deflate, and Brotli
        /// </remarks>
        /// <example>
        /// <code>
        /// // Simple registration with defaults
        /// services.AddJfYuHttpClient();
        /// 
        /// // Register multiple named clients with different configurations
        /// services.AddJfYuHttpClient(options =>
        /// {
        ///     options.HttpClientName = "ApiClient";
        ///     options.HttpClientHandler = () => new HttpClientHandler
        ///     {
        ///         Proxy = new WebProxy("http://proxy:8080"),
        ///         UseProxy = true
        ///     };
        ///     options.ConfigureClient = client =>
        ///     {
        ///         client.Timeout = TimeSpan.FromMinutes(5);
        ///         client.DefaultRequestHeaders.Add("X-Api-Key", "secret");
        ///     };
        /// }, filter =>
        /// {
        ///     filter.LoggingFields = JfYuLoggingFields.All;
        /// });
        /// 
        /// // Register another client for a different API
        /// services.AddJfYuHttpClient(options =>
        /// {
        ///     options.HttpClientName = "ExternalApi";
        ///     options.UseSharedCookieContainer = false;
        /// });
        /// </code>
        /// </example>
        public static void AddJfYuHttpClient(this IServiceCollection services, Action<JfYuHttpClientOptions>? configureOptions = null, Action<LogFilter>? filter = null)
        {
            var options = new JfYuHttpClientOptions();
            configureOptions?.Invoke(options);

            var build = services.AddHttpClient(options.HttpClientName);
            
            if (options.UseSharedCookieContainer)
                services.AddSingleton<CookieContainer>();

            if (options.HttpClientHandler is not null)
            {
                build.ConfigurePrimaryHttpMessageHandler(options.HttpClientHandler);
            }
            else
            {
                build.ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                    };

                    if (options.UseSharedCookieContainer)
                    {
                        var cookieContainer = sp.GetRequiredService<CookieContainer>();
                        handler.CookieContainer = cookieContainer;
                    }

                    return handler;
                });
            }

            if (options.ConfigureClient is not null)
            {
                build.ConfigureHttpClient(options.ConfigureClient);
            }

            // Store the HttpClient name in DI for JfYuHttpClient to use
            services.AddSingleton(new JfYuHttpClientConfiguration { HttpClientName = options.HttpClientName });

            services.AddScoped<IJfYuRequest, JfYuHttpClient>();
            services.AddScoped<IJfYuRequestFactory, JfYuRequestFactory>();
            
            var logFilter = new LogFilter();
            filter?.Invoke(logFilter);
            services.AddSingleton(logFilter);
        }
#endif
    }
}