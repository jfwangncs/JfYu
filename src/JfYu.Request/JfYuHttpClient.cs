#if NET8_0_OR_GREATER
using JfYu.Request.Enum;
using JfYu.Request.Logs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.Request
{

    /// <summary>
    /// Represents an HTTP request with logging and additional features.(Using HttpClient)
    /// </summary>
    /// <param name="factory"> Initializes a new instance of the <see cref="IHttpClientFactory"/> class.</param>
    /// <param name="configuration">The HttpClient configuration containing the client name to use.</param>
    /// <param name="cookieContainer">The cookie container</param>
    /// <param name="logFilter">The log filter.</param>
    /// <param name="logger">The logger.</param>
    public class JfYuHttpClient(IHttpClientFactory factory, JfYuHttpClientConfiguration configuration,  LogFilter logFilter,CookieContainer? cookieContainer = null, ILogger<JfYuHttpClient>? logger = null) : JfYuBaseRequest
    {
        /// <summary>
        /// The HTTP web request.
        /// </summary>
        private HttpClient? _request;

        /// <summary>
        /// The HTTP client factory to create the HttpClient instance.
        /// </summary>
        private readonly ILogger<JfYuHttpClient>? _logger = logger;

        /// <summary>
        /// The HTTP client factory to create the HttpClient instance.
        /// </summary>
        private readonly LogFilter _logFilter = logFilter;

        /// <summary>
        /// The cookie container
        /// </summary>
        private readonly CookieContainer? _cookieContainer = cookieContainer;

        /// <summary>
        /// Semaphore to protect concurrent access to CookieContainer
        /// </summary>
        private readonly SemaphoreSlim _cookieLock = new(1, 1);

        /// <summary>
        /// The configuration containing the HttpClient name to use
        /// </summary>
        private readonly JfYuHttpClientConfiguration _configuration = configuration;

        /// <inheritdoc/>
        private async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _request = factory.CreateClient(_configuration.HttpClientName);
            _request.Timeout = TimeSpan.FromSeconds(Timeout);
            if (!string.IsNullOrEmpty(RequestHeader.UserAgent))
                _request.DefaultRequestHeaders.UserAgent.ParseAdd(RequestHeader.UserAgent);
            if (!string.IsNullOrEmpty(RequestHeader.AcceptEncoding))
                _request.DefaultRequestHeaders.AcceptEncoding.ParseAdd(RequestHeader.AcceptEncoding);
            if (!string.IsNullOrEmpty(RequestHeader.AcceptLanguage))
                _request.DefaultRequestHeaders.AcceptLanguage.ParseAdd(RequestHeader.AcceptLanguage);
            if (!string.IsNullOrEmpty(RequestHeader.CacheControl))
                _request.DefaultRequestHeaders.Add("Cache-Control", RequestHeader.CacheControl);
            if (!string.IsNullOrEmpty(RequestHeader.Pragma))
                _request.DefaultRequestHeaders.Pragma.ParseAdd(RequestHeader.Pragma);
            if ("keep-alive".Equals(RequestHeader.Connection))
                _request.DefaultRequestHeaders.ConnectionClose = false;
            if (!string.IsNullOrEmpty(RequestHeader.Host))
                _request.DefaultRequestHeaders.Host = RequestHeader.Host.Replace("https://", "").Replace("http://", "");
            if (!string.IsNullOrEmpty(RequestHeader.Referer))
                _request.DefaultRequestHeaders.Referrer = new Uri(RequestHeader.Referer);
            if (!string.IsNullOrEmpty(RequestHeader.Accept))
                _request.DefaultRequestHeaders.Accept.ParseAdd(RequestHeader.Accept);

            if (!string.IsNullOrEmpty(Authorization))
                _request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, Authorization.Replace($"{AuthorizationScheme} ", ""));
            if (_cookieContainer is not null)
            {
                await _cookieLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    RequestCookies?.GetCookies(new Uri(Url)).ToList().ForEach(x => _cookieContainer.SetCookies(new Uri(Url), $"{x.Name}={x.Value}"));
                }
                finally
                {
                    _cookieLock.Release();
                }
            }
            foreach (var item in RequestCustomHeaders)
            {
                _request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
            }

            CustomInit?.Invoke(_request);
        }

        /// <inheritdoc/>        
        public override async Task<string> SendAsync(CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
            var requestId = Guid.NewGuid().ToString();
            string html = string.Empty;
            if (_logFilter.LoggingFields != JfYuLoggingFields.None)
                _logger?.LogRequestWithFilter(_logFilter.LoggingFields, requestId, Url, Method.ToString(), JsonConvert.SerializeObject(_request!.DefaultRequestHeaders.ToDictionary(header => header.Key, header => header.Value.ToList())), _logFilter.RequestFilter.Invoke(RequestData));
            var response = await SendHttpRequestAsync(cancellationToken).ConfigureAwait(false);
            ResponseHeader = response.Headers.ToDictionary(header => header.Key, header => header.Value.ToList());
            StatusCode = response.StatusCode;
            if (_cookieContainer is not null)
            {
                await _cookieLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    _cookieContainer.GetCookies(new Uri(Url)).ToList().ForEach(ResponseCookies.Add);
                }
                finally
                {
                    _cookieLock.Release();
                }
            }
            string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            html = RequestEncoding.GetString(RequestEncoding.GetBytes(content));
            if (_logFilter.LoggingFields != JfYuLoggingFields.None)
                _logger?.LogResponseWithFilter(_logFilter.LoggingFields, requestId, StatusCode.ToString(), _logFilter.ResponseFilter.Invoke(html));
            return html;
        }

        /// <inheritdoc/>
        public override async Task<bool> DownloadFileAsync(string path, Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            await InitializeAsync(cancellationToken).ConfigureAwait(false);

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            using var response = await _request!.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            StatusCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
                return false;
            var filesize = (decimal?)response.Content.Headers.ContentLength ?? 0M;
            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, FileOptions.Asynchronous);
            await DownloadFileInternalAsync(fileStream, responseStream, filesize, progress, cancellationToken).ConfigureAwait(false);
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public override async Task<MemoryStream?> DownloadFileAsync(Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
            using var response = await _request!.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return default;
            var filesize = (decimal?)response.Content.Headers.ContentLength ?? 0M;
            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var memoryStream = new MemoryStream();
            await DownloadFileInternalAsync(memoryStream, responseStream, filesize, progress, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Sends the HTTP request asynchronously based on the specified HTTP method and content type.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The HttpResponse Message</returns>
        private async Task<HttpResponseMessage> SendHttpRequestAsync(CancellationToken cancellationToken = default)
        {
            if (Method == HttpMethod.Post)
            {
                if (string.Compare(ContentType, RequestContentType.FormData, StringComparison.Ordinal) == 0)
                    return await SendFormDataAsync(cancellationToken).ConfigureAwait(false);
                else
                    return await SendStringContentAsync(HttpMethod.Post, cancellationToken).ConfigureAwait(false);
            }
            else if (Method == HttpMethod.Put)
                return await SendStringContentAsync(HttpMethod.Put, cancellationToken).ConfigureAwait(false);
            else if (Method == HttpMethod.Patch)
                return await SendStringContentAsync(new HttpMethod("PATCH"), cancellationToken).ConfigureAwait(false);
            else if (Method == HttpMethod.Delete)
                return await _request!.DeleteAsync(Url, cancellationToken).ConfigureAwait(false);
            else if (Method == HttpMethod.Get)
                return await _request!.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            else
                using (var request = new HttpRequestMessage(Method, Url))
                {
                    if (!string.IsNullOrEmpty(RequestData))
                        request.Content = new StringContent(RequestData, RequestEncoding, ContentType);
                    return await _request!.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }
        }

        /// <summary>
        /// Sends a string content request asynchronously based on the specified HTTP method.
        /// </summary>
        /// <param name="method">The HttpMethod</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The HttpResponse Message</returns>
        private async Task<HttpResponseMessage> SendStringContentAsync(HttpMethod method, CancellationToken cancellationToken = default)
        {
            using var stringContent = new StringContent(RequestData, RequestEncoding, ContentType);
            if (method == HttpMethod.Patch)
                return await _request!.PatchAsync(Url, stringContent, cancellationToken).ConfigureAwait(false);
            else if (method == HttpMethod.Put)
                return await _request!.PutAsync(Url, stringContent, cancellationToken).ConfigureAwait(false);
            else
                return await _request!.PostAsync(Url, stringContent, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a multipart/form-data request asynchronously with the specified files and form data.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The HttpResponse Message</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2000:Dispose objects before losing scope")]
        private async Task<HttpResponseMessage> SendFormDataAsync(CancellationToken cancellationToken = default)
        {
            using var formData = new MultipartFormDataContent();
            if (!string.IsNullOrEmpty(RequestData))
            {
                var paras = RequestData;
                string[] parameters = paras.Split('&');
                foreach (string param in parameters)
                {
                    string[] keyValue = param.Split('=');
                    if (keyValue.Length == 2)
                    {
                        // stringContent will be disposed by formData.Dispose()
                        var stringContent = new StringContent(keyValue[1]);
                        formData.Add(stringContent, keyValue[0]);
                    }
                }
            }

            foreach (var file in Files)
            {
                // stringContent will be disposed by formData.Dispose()
                var fileStream = File.OpenRead(file.Value);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                formData.Add(fileContent, file.Key, Path.GetFileName(file.Value));
            }
            return await _request!.PostAsync(Url, formData, cancellationToken).ConfigureAwait(false);
        }
    }
}
#endif