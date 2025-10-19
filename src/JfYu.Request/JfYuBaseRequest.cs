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
    ///Implement the basic properties and methods of the IJfYuRequest
    /// </summary>
    public abstract class JfYuBaseRequest : IJfYuRequest
    {
        /// <inheritdoc/>
        public string Url { get; set; } = "";

        /// <inheritdoc/>
        public string ContentType { get; set; } = RequestContentType.Json;

        /// <inheritdoc/>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <inheritdoc/>
        public string Authorization { get; set; } = "";

        /// <inheritdoc/>
        public string AuthorizationScheme { get; set; } = "Bearer";

        /// <inheritdoc/>
        public Encoding RequestEncoding { get; set; } = Encoding.UTF8;

        /// <inheritdoc/>
        public CookieContainer RequestCookies { get; set; } = new();

        /// <inheritdoc/>
        public CookieCollection ResponseCookies { get; set; } = [];

        /// <inheritdoc/>
        public WebProxy? Proxy { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, string> Files { get; set; } = [];

        /// <inheritdoc/>
        public RequestHeaders RequestHeader { get; set; } = new();

        /// <inheritdoc/>
        public int Timeout { get; set; } = 30;

        /// <inheritdoc/>
        public Dictionary<string, string> RequestCustomHeaders { get; set; } = [];

        /// <inheritdoc/>
        public string RequestData { get; set; } = "";

        /// <inheritdoc/>
        public X509Certificate2? Certificate { get; set; }

        /// <inheritdoc/>
        public bool CertificateValidation { get; set; }

        /// <inheritdoc/>
        public Action<object>? CustomInit { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, List<string>> ResponseHeader { get; protected set; } = [];

        /// <inheritdoc/>
        public HttpStatusCode StatusCode { get; protected set; }

        /// <inheritdoc/>
        protected const int DefaultBufferSize = 8192;

        /// <summary>
        /// Downloads a file asynchronously to a target stream.
        /// </summary>
        /// <param name="targetStream">The stream to write the downloaded file to.</param>
        /// <param name="responseStream">The stream to read the file from.</param>
        /// <param name="fileSize">The size of the file being downloaded.</param>
        /// <param name="progress">The progress delegate function.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The target stream containing the downloaded file.</returns>
        protected static async Task<Stream> DownloadFileInternalAsync(Stream targetStream, Stream responseStream, decimal fileSize, Action<decimal, decimal, decimal>? progress, CancellationToken cancellationToken = default)
        {
            decimal lastSaveSize = 0;
            byte[] buffer = new byte[DefaultBufferSize];
            int bytesRead;
            do
            {
#if NET8_0_OR_GREATER
                bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                await targetStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
#else
                bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                await targetStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
#endif
                if (progress != null && fileSize > 0)
                {
                    decimal currentLength = targetStream.Length;
                    decimal percentage = currentLength / fileSize * 100;
                    decimal speed = (currentLength - lastSaveSize) / 1024;
                    decimal remain = speed == 0 ? 0 : (fileSize - currentLength) / (speed * 1024);
                    progress.Invoke(percentage, speed, remain);
                    lastSaveSize = currentLength;
                }
            }
            while (bytesRead > 0);
            await targetStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            progress?.Invoke(100M, 0M, 0M);
            return targetStream;
        }

        /// <inheritdoc/>
        public abstract Task<string> SendAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<bool> DownloadFileAsync(string path, Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default);

        /// <inheritdoc/>

        public abstract Task<MemoryStream?> DownloadFileAsync(Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default);
    }
}