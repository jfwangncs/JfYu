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
    /// Interface for JfYuRequest.
    /// </summary>
    public interface IJfYuRequest
    {
        /// <summary>
        /// Url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// ContentType,default:Josn <see cref="RequestContentType"/> class.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Http Method <see cref="HttpMethod"/> class.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Authorization.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Authorization.
        /// </summary>
        public string AuthorizationScheme { get; set; }

        /// <summary>
        /// Encoding default:utf8 <see cref="Encoding"/> class.
        /// </summary>
        public Encoding RequestEncoding { get; set; }

        /// <summary>
        /// Request cookies <see cref="CookieContainer"/> class.
        /// </summary>
        public CookieContainer RequestCookies { get; set; }

        /// <summary>
        /// Return cookies <see cref="CookieCollection"/> class.
        /// </summary>
        public CookieCollection ResponseCookies { get; set; }

        /// <summary>
        /// Proxy only for JfYuHttpRequest <see cref="WebProxy"/> class.
        /// For Httpclient please use HttpClientHandler set UseProxy=True and set Proxy property.
        /// </summary>
        public WebProxy? Proxy { get; set; }

        /// <summary>
        /// Upload files.
        /// </summary>
        public Dictionary<string, string> Files { get; set; }

        /// <summary>
        /// Header <see cref="RequestHeaders"/> class.
        /// </summary>
        public RequestHeaders RequestHeader { get; set; }

        /// <summary>
        /// Timeout default:30 seconds.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Custom headers.
        /// </summary>
        public Dictionary<string, string> RequestCustomHeaders { get; set; }

        /// <summary>
        /// Request payload data in various formats.
        /// </summary>
        /// <remarks>
        /// <para><b>JSON format:</b> <c>{"username":"testUser"}</c></para>
        /// <para><b>Form Data format:</b> <c>username=testUser</c></para>
        /// <para><b>XML format:</b> <c>&lt;user&gt;&lt;username&gt;testUser&lt;/username&gt;&lt;/user&gt;</c></para>
        /// <para>Format is determined by <see cref="ContentType"/>.</para>
        /// </remarks>
        public string RequestData { get; set; }

        /// <summary>
        /// SSL only for JfYuHttpRequest <see cref="X509Certificate2"/> class.
        /// For Httpclient please use HttpClientHandler and set ClientCertificates property.
        /// </summary>
        public X509Certificate2? Certificate { get; set; }

        /// <summary>
        /// CertificateValidation default=false only for JfYuHttpRequest.
        /// </summary>
        public bool CertificateValidation { get; set; }

        /// <summary>
        /// Custom init.for jfYuHttpRequest:object as HttpWebRequest,for jfYuHttpClient:object as HttpClient.
        /// </summary>
        Action<object>? CustomInit { get; set; }

        /// <summary>
        /// Response Header.
        /// </summary>
        public Dictionary<string, List<string>> ResponseHeader { get; }

        /// <summary>
        /// HttpStatusCode.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Sends the HTTP request asynchronously.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The response content.</returns>
        Task<string> SendAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file asynchronously.
        /// </summary>
        /// <param name="path">The file save location.</param>
        /// <param name="progress">The progress delegate function.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>True if the download is successful, otherwise false.</returns>
        Task<bool> DownloadFileAsync(string path, Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file to a memory stream asynchronously.
        /// </summary>
        /// <param name="progress">The progress delegate function.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The memory stream containing the downloaded file.</returns>
        Task<MemoryStream?> DownloadFileAsync(Action<decimal, decimal, decimal>? progress = null, CancellationToken cancellationToken = default);
    }
}