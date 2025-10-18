using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using JfYu.Request.Enum;
using JfYu.Request.Logs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.Request
{
    /// <summary>
    /// Represents an HTTP request with logging and additional features.(Using HttpWebRequest)
    /// </summary>
    public class JfYuHttpRequest : JfYuBaseRequest
    {
        /// <summary>
        /// The HTTP web request.
        /// </summary>
        private HttpWebRequest? _request;

        /// <summary>
        /// Logger for JfYuHttpRequest, used to log request and response details.
        /// </summary>
        private readonly ILogger<JfYuHttpRequest>? _logger;

        /// <summary>
        /// The log filter for controlling which fields are logged.
        /// </summary>
        private readonly LogFilter _logFilter;

        /// <summary>
        /// The old certificate validation callback, used to restore the original behavior after custom validation is applied.
        /// </summary>
        private RemoteCertificateValidationCallback? _oldCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="JfYuHttpRequest"/> class.
        /// </summary>
        /// <param name="logFilter">The log filter.</param>
        /// <param name="logger">The logger.</param>
        public JfYuHttpRequest(LogFilter? logFilter = null, ILogger<JfYuHttpRequest>? logger = null)
        {
            _logFilter = logFilter ?? new LogFilter() { LoggingFields = JfYuLoggingFields.None };
            _logger = logger;
        }

        /// <inheritdoc/>
        private void Initialize()
        {
            _request = (HttpWebRequest)WebRequest.Create(Url);
            _request.Method = Method.ToString().ToUpper();

            try
            {
                SetupHeaders(_request);

                CustomInit?.Invoke(_request);

                if (!Method.Equals(HttpMethod.Get))
                {
                    if (string.Compare(ContentType, RequestContentType.FormData, StringComparison.Ordinal) == 0)
                        WriteFormData(_request);
                    else
                    {
                        _request.ContentType = ContentType;
                        if (!string.IsNullOrEmpty(RequestData))
                        {
                            var data = RequestEncoding.GetBytes(RequestData);
                            _request.ContentLength = data.Length;
                            using var requestStream = _request.GetRequestStream();
                            requestStream.Write(data, 0, data.Length);
                        }
                    }
                }
                SetupCertificate(_request);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while initialize.");
                throw;
            }
        }

        /// <summary>
        /// Sets up the headers for the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        private void SetupHeaders(HttpWebRequest request)
        {
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.CookieContainer = RequestCookies;
            request.Timeout = Timeout * 1000;
            request.ReadWriteTimeout = Timeout * 1000;
            request.UserAgent = RequestHeader.UserAgent;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, RequestHeader.AcceptEncoding);// Define gzip compression support
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, RequestHeader.AcceptLanguage);
            request.Headers.Add(HttpRequestHeader.CacheControl, RequestHeader.CacheControl);
            request.Headers.Add(HttpRequestHeader.Pragma, RequestHeader.Pragma);
            if ("keep-alive".Equals(RequestHeader.Connection))
                request.KeepAlive = true;
            if (!string.IsNullOrEmpty(RequestHeader.Host))
                request.Host = RequestHeader.Host.Replace("https://", "").Replace("http://", "");
            request.Referer = RequestHeader.Referer;
            request.Accept = RequestHeader.Accept;

            if (Proxy != null)
                request.Proxy = Proxy;

            if (!string.IsNullOrEmpty(Authorization))
                request.Headers.Add("Authorization", $"{AuthorizationScheme} {Authorization.Replace($"{AuthorizationScheme} ", "")}");

            foreach (var item in RequestCustomHeaders)
            {
                request.Headers.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Sets up the certificate for the HTTP request if provided, and configures certificate validation behavior.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        private void SetupCertificate(HttpWebRequest request)
        {
            if (Certificate != null)
            {
                CertificateValidation = true;
                request.ClientCertificates.Add(Certificate);
            }
            if (!CertificateValidation)
            {
                _oldCallback = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
        }

        /// <summary>
        /// Writes form data to the HTTP request for multipart/form-data content type.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        private void WriteFormData(HttpWebRequest request)
        {

            var memStream = new MemoryStream();
            string boundary = $"--{DateTime.Now.Ticks:x}";
            if (!string.IsNullOrEmpty(RequestData))
            {
                var paras = RequestData;
                string[] parameters = paras.Split('&');
                foreach (string param in parameters)
                {
                    string[] keyValue = param.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0];
                        string value = keyValue[1];
                        var paramByte = RequestEncoding.GetBytes($"\r\n--{boundary}\r\nContent-Disposition: form-data; name=\"{key}\"\r\n\r\n{value}");
                        memStream.Write(paramByte, 0, paramByte.Length);
                    }
                }
            }
            foreach (var item in Files)
            {
                string filePath = item.Value;
                byte[] fileData = File.ReadAllBytes(filePath);

                var fileContentStrByte = Encoding.UTF8.GetBytes($"\r\n--{boundary}\r\n" +
                    $"Content-Disposition: form-data; name=\"{item.Key}\"; filename=\"{Path.GetFileName(item.Value)}\"\r\n" +
                    $"Content-Type:application/octet-stream\r\n\r\n");
                memStream.Write(fileContentStrByte, 0, fileContentStrByte.Length);
                memStream.Write(fileData, 0, fileData.Length);
            }
            var endBoundary = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
            memStream.Write(endBoundary, 0, endBoundary.Length);
            byte[] tempBuffer = memStream.ToArray();
            request.ContentLength = tempBuffer.Length;
            request.ContentType = $"multipart/form-data;boundary={boundary}";
            using var reqStream = request.GetRequestStream();
            reqStream.Write(tempBuffer, 0, tempBuffer.Length);
        }

        /// <summary>
        /// Gets the response content as a string.
        /// </summary>
        /// <param name="response">The HTTP web response.</param>
        /// <returns>The response content.</returns>
        private string GetResponse(HttpWebResponse response)
        {
#if NET8_0_OR_GREATER
            if (response.ContentEncoding.Contains("br", StringComparison.OrdinalIgnoreCase) || response.ContentEncoding.Contains("brotli", StringComparison.OrdinalIgnoreCase))
#else
            if (response.ContentEncoding.ToLower().Contains("br") || response.ContentEncoding.ToLower().Contains("brotli"))
#endif
            {
                using var brotliStream = new Brotli.BrotliStream(response.GetResponseStream(), CompressionMode.Decompress);
                using var brotliReader = new StreamReader(brotliStream, RequestEncoding);
                return brotliReader.ReadToEnd();
            }
#if NET8_0_OR_GREATER
            else if (response.ContentEncoding.Contains("deflate", StringComparison.OrdinalIgnoreCase))
#else
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
#endif
            {
                using var inflater = new InflaterInputStream(response.GetResponseStream());
                using var deflateReader = new StreamReader(inflater, RequestEncoding);
                return deflateReader.ReadToEnd();
            }

            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream, RequestEncoding);
            return reader.ReadToEnd();
        }

        /// <inheritdoc/>
        public override async Task<string> SendAsync(CancellationToken cancellationToken = default)
        {
            var html = string.Empty;
            var requestId = Guid.NewGuid().ToString();
            try
            {
                Initialize();
                if (_logFilter.LoggingFields != JfYuLoggingFields.None)
                    _logger?.LogRequestWithFilter(_logFilter.LoggingFields, requestId, Url, Method.ToString(), JsonConvert.SerializeObject(_request!.Headers.AllKeys.ToDictionary(header => header, header => _request.Headers.GetValues(header)!.ToList())), _logFilter.RequestFilter.Invoke(RequestData));
#if NET8_0_OR_GREATER
                HttpWebResponse response = (HttpWebResponse)await _request!.GetResponseAsync().ConfigureAwait(false);
#else
                // NOSONAR: GetResponseAsync timeout bug on Windows, use sync version for reliability
                HttpWebResponse response = (HttpWebResponse)await Task.FromResult(_request!.GetResponse());
#endif
                StatusCode = response.StatusCode;
                html = GetResponse(response);
                ResponseCookies = _request!.CookieContainer?.GetCookies(_request.RequestUri) ?? [];
                ResponseHeader = response.Headers.AllKeys.ToDictionary(header => header, header => response.Headers.GetValues(header)!.ToList());
                response.Close();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    var response = (HttpWebResponse)e.Response;
                    html = GetResponse(response);
                    StatusCode = response.StatusCode;
                }
                else
                {
                    _logger?.LogError(e, "An error occurred while sending request.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while sending request.");
                throw;
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = _oldCallback;
            }
            if (_logFilter.LoggingFields != JfYuLoggingFields.None)
                _logger?.LogResponseWithFilter(_logFilter.LoggingFields, requestId, StatusCode.ToString(), _logFilter.ResponseFilter.Invoke(html));
            return html;
        }

        /// <inheritdoc/>
        public override async Task<bool> DownloadFileAsync(string path, Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            Initialize();
            try
            {
#if NET8_0_OR_GREATER
                HttpWebResponse response = (HttpWebResponse)await _request!.GetResponseAsync().ConfigureAwait(false);
#else
                // NOSONAR: GetResponseAsync timeout bug on Windows, use sync version for reliability
                HttpWebResponse response = (HttpWebResponse)await Task.FromResult(_request!.GetResponse());
#endif
                StatusCode = response.StatusCode;
                using Stream responseStream = response.GetResponseStream();
                ResponseCookies = _request!.CookieContainer?.GetCookies(_request.RequestUri) ?? [];
                var filesize = (decimal)response.ContentLength;
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, FileOptions.Asynchronous);
                await DownloadFileInternalAsync(fileStream, responseStream, filesize, progress, cancellationToken).ConfigureAwait(false);
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while downloading the file.");
                throw;
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = _oldCallback;
            }
        }

        /// <inheritdoc/>
        public override async Task<MemoryStream?> DownloadFileAsync(Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default)
        {
            Initialize();
            try
            {
#if NET8_0_OR_GREATER
                HttpWebResponse response = (HttpWebResponse)await _request!.GetResponseAsync().ConfigureAwait(false);
#else
                // NOSONAR: GetResponseAsync timeout bug on Windows, use sync version for reliability
                HttpWebResponse response = (HttpWebResponse)await Task.FromResult(_request!.GetResponse());
#endif
                StatusCode = response.StatusCode;
                using Stream responseStream = response.GetResponseStream();
                ResponseCookies = _request!.CookieContainer?.GetCookies(_request.RequestUri) ?? [];
                var filesize = decimal.Parse(response.ContentLength.ToString());
                var memoryStream = new MemoryStream();
                await DownloadFileInternalAsync(memoryStream, responseStream, filesize, progress, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while downloading the file.");
                throw;
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = _oldCallback;
            }
        }
    }
}