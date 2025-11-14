using JfYu.Request.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.Request
{
    /// <summary>
    /// Interface for configuring and executing HTTP requests with support for dual implementations (HttpClient and HttpWebRequest).
    /// Provides a unified API for HTTP operations including standard requests, file downloads, and custom headers/authentication.
    /// </summary>
    public interface IJfYuRequest
    {
        /// <summary>
        /// Gets or sets the target URL for the HTTP request.
        /// Must be a valid HTTP or HTTPS URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Content-Type header for the request.
        /// Default value is <see cref="RequestContentType.Json"/> ("application/json").
        /// See <see cref="RequestContentType"/> for common values.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method (GET, POST, PUT, DELETE, etc.).
        /// Uses <see cref="HttpMethod"/> constants from System.Net.Http.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the Authorization header value (e.g., token, API key).
        /// Used in conjunction with <see cref="AuthorizationScheme"/>.
        /// </summary>
        /// <example>
        /// For Bearer token: AuthorizationScheme = "Bearer", Authorization = "your-token-here"
        /// </example>
        public string Authorization { get; set; }

        /// <summary>
        /// Gets or sets the Authorization scheme (e.g., "Bearer", "Basic", "ApiKey").
        /// Combined with <see cref="Authorization"/> to form the complete Authorization header.
        /// </summary>
        public string AuthorizationScheme { get; set; }

        /// <summary>
        /// Gets or sets the character encoding for the request body.
        /// Default value is UTF-8. See <see cref="Encoding"/> class for other options.
        /// </summary>
        public Encoding RequestEncoding { get; set; }

        /// <summary>
        /// Gets or sets the cookie container for sending cookies with the request.
        /// Cookies are automatically managed and persisted across requests when using the same instance.
        /// See <see cref="CookieContainer"/> class.
        /// </summary>
        public CookieContainer RequestCookies { get; set; }

        /// <summary>
        /// Gets the cookies returned by the server in the response.
        /// Populated after SendAsync completes. See <see cref="CookieCollection"/> class.
        /// </summary>
        public CookieCollection ResponseCookies { get; set; }

        /// <summary>
        /// Gets or sets the HTTP proxy to use for the request.
        /// Only applicable for JfYuHttpRequest (HttpWebRequest-based implementation).
        /// For HttpClient-based requests, configure proxy using HttpClientHandler.UseProxy and HttpClientHandler.Proxy.
        /// See <see cref="WebProxy"/> class.
        /// </summary>
        public WebProxy? Proxy { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of files to upload in multipart/form-data requests.
        /// Key: Form field name, Value: File path on disk.
        /// </summary>
        /// <example>
        /// Files = new Dictionary&lt;string, string&gt; { { "file", "C:\\path\\to\\file.txt" } }
        /// </example>
        public Dictionary<string, string> Files { get; set; }

        /// <summary>
        /// Gets or sets common HTTP request headers (User-Agent, Accept, Referer, etc.).
        /// See <see cref="RequestHeaders"/> class for available headers.
        /// </summary>
        public RequestHeaders RequestHeader { get; set; }

        /// <summary>
        /// Gets or sets the request timeout in seconds.
        /// Default value is 30 seconds. Applies to both connection and read timeouts.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of custom HTTP headers to include in the request.
        /// Use for non-standard headers not covered by <see cref="RequestHeader"/>.
        /// </summary>
        /// <example>
        /// RequestCustomHeaders = new Dictionary&lt;string, string&gt; { { "X-Custom-Header", "value" } }
        /// </example>
        public Dictionary<string, string> RequestCustomHeaders { get; set; }

        /// <summary>
        /// Gets or sets the request payload data in various formats (JSON, form data, XML, plain text).
        /// The format is determined by the <see cref="ContentType"/> property.
        /// </summary>
        /// <remarks>
        /// <para><b>JSON format:</b> <c>{"username":"testUser"}</c></para>
        /// <para><b>Form Data format:</b> <c>username=testUser&amp;password=pass123</c></para>
        /// <para><b>XML format:</b> <c>&lt;user&gt;&lt;username&gt;testUser&lt;/username&gt;&lt;/user&gt;</c></para>
        /// <para><b>Plain text:</b> Any string content</para>
        /// </remarks>
        public string RequestData { get; set; }

        /// <summary>
        /// Gets or sets the SSL/TLS client certificate for mutual authentication.
        /// Only applicable for JfYuHttpRequest (HttpWebRequest-based implementation).
        /// For HttpClient-based requests, configure certificate using HttpClientHandler.ClientCertificates.
        /// See <see cref="X509Certificate2"/> class.
        /// </summary>
        public X509Certificate2? Certificate { get; set; }

        /// <summary>
        /// Gets or sets whether to validate the server's SSL certificate.
        /// Default value is false (validation disabled, useful for self-signed certificates).
        /// Only applicable for JfYuHttpRequest (HttpWebRequest-based implementation).
        /// </summary>
        /// <remarks>
        /// Setting to false bypasses SSL validation. Only use in development or for trusted self-signed certificates.
        /// </remarks>
        public bool CertificateValidation { get; set; }

        /// <summary>
        /// Gets or sets a custom initialization action for the underlying HTTP client.
        /// For JfYuHttpRequest: The parameter is <see cref="HttpWebRequest"/>.
        /// For JfYuHttpClient: The parameter is <see cref="HttpClient"/>.
        /// </summary>
        /// <remarks>
        /// Use this to configure low-level settings not exposed by the interface.
        /// Example: Setting KeepAlive, AllowAutoRedirect, or custom HttpMessageHandler.
        /// </remarks>
        Action<object>? CustomInit { get; set; }

        /// <summary>
        /// Gets the response headers returned by the server after SendAsync completes.
        /// Each header name maps to a list of values (headers can have multiple values).
        /// </summary>
        public Dictionary<string, List<string>> ResponseHeader { get; }

        /// <summary>
        /// Gets the HTTP status code of the last response.
        /// Available after SendAsync completes. See <see cref="HttpStatusCode"/> enum.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Sends the configured HTTP request asynchronously and returns the response body as a string.
        /// Automatically handles serialization, headers, cookies, and authentication.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to abort the request.</param>
        /// <returns>The response body as a string.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails due to network or server errors.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request times out or is cancelled.</exception>
        Task<string> SendAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file from the specified <see cref="Url"/> and saves it to the local file system.
        /// Supports progress reporting during download.
        /// </summary>
        /// <param name="path">The absolute file path where the downloaded file will be saved.</param>
        /// <param name="progress">Optional progress callback with parameters: (bytesReceived, totalBytes, percentage).</param>
        /// <param name="cancellationToken">Cancellation token to abort the download.</param>
        /// <returns>True if the download completes successfully, otherwise false.</returns>
        /// <exception cref="IOException">Thrown when file write operations fail.</exception>
        /// <exception cref="HttpRequestException">Thrown when the download request fails.</exception>
        Task<bool> DownloadFileAsync(string path, Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file from the specified <see cref="Url"/> and returns it as a <see cref="MemoryStream"/>.
        /// Supports progress reporting during download. Useful for in-memory processing without saving to disk.
        /// </summary>
        /// <param name="progress">Optional progress callback with parameters: (bytesReceived, totalBytes, percentage).</param>
        /// <param name="cancellationToken">Cancellation token to abort the download.</param>
        /// <returns>A MemoryStream containing the downloaded file content, or null if the download fails.</returns>
        /// <exception cref="HttpRequestException">Thrown when the download request fails.</exception>
        /// <remarks>
        /// The caller is responsible for disposing the returned MemoryStream.
        /// For large files, consider using the file path overload to avoid high memory usage.
        /// </remarks>
        Task<MemoryStream?> DownloadFileAsync(Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default);
    }
}