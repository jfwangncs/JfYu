using JfYu.Request;
using JfYu.Request.Enum;
using JfYu.Request.Extension;
using JfYu.Request.Logs;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace JfYu.UnitTests.Request
{
    [Collection("JfYuRequest")]
    public class HttpWebRequestTests
    {
        public readonly HttpTestOption _url;

        public HttpWebRequestTests()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
              .Build();
            services.Configure<HttpTestOption>(configuration.GetSection("HttpTestOption"));
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HttpTestOption>>();
            _url = options.Value;
        }

        [Fact]
        public void AddService_NoFilter()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpRequest();
            var serviceProvider = services.BuildServiceProvider();
            var req = serviceProvider.GetService<IJfYuRequest>();
            Assert.NotNull(req);
        }

        [Fact]
        public void AddService_WithFilter()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpRequest(q => q.RequestFilter = z => z);
            var serviceProvider = services.BuildServiceProvider();
            var req = serviceProvider.GetService<IJfYuRequest>();
            Assert.NotNull(req);
        }

        #region Send

        [Fact]
        public async Task Test_Get_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/get?username=testUser&age=30",
                Method = HttpMethod.Get
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Post_Plain_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/post",
                Method = HttpMethod.Post,
                RequestData = "username=testUser&age=30",
                ContentType = RequestContentType.Plain,
                RequestEncoding = Encoding.UTF8
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains(client.RequestData, jsonResponse!["data"].ToString());
        }

        [Fact]
        public async Task Test_Post_XML_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/post",
                Method = HttpMethod.Post,
                RequestData = "<user><username>testUser</username><age>30</age></user>",
                ContentType = RequestContentType.Xml
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains(client.RequestData, jsonResponse!["data"].ToString());
        }

        [Fact]
        public async Task Test_Post_FormUrlEncoded_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/post",
                Method = HttpMethod.Post,
                RequestData = "username=testUser&age=30",
                ContentType = RequestContentType.FormUrlEncoded
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["form"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["form"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Post_Json_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/post",
                Method = HttpMethod.Post,
                RequestData = "{\"username\":\"testUser\",\"age\":30}",
                ContentType = RequestContentType.Json
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Post_FormData_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/post",
                Method = HttpMethod.Post,
                RequestData = "username=testUser&age=30",
                ContentType = RequestContentType.FormData
            };
            var file = Guid.NewGuid().ToString() + ".txt";
            var text = "dada dadad大大大阿斗哇打完带娃啊我啊";
            if (File.Exists(file))
                File.Delete(file);

            using var fs = File.Create(file);
#if NET8_0_OR_GREATER
            await fs.WriteAsync(Encoding.UTF8.GetBytes(text));
            await fs.DisposeAsync();
#else
            await fs.WriteAsync(Encoding.UTF8.GetBytes(text), 0, Encoding.UTF8.GetBytes(text).Length);
            fs.Dispose();
#endif

            client.Files = new Dictionary<string, string>() { { "test.txt", file }, { "test1.txt", file } };
            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);

            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["form"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["form"].ToString()!)!["age"]!.ToString());
            Assert.Contains(text, JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["files"].ToString()!)!["test.txt"].ToString());
            Assert.Contains(text, JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["files"].ToString()!)!["test1.txt"].ToString());

            if (File.Exists(file))
                File.Delete(file);
        }

        [Fact]
        public async Task Test_Put_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/put",
                Method = HttpMethod.Put,
                RequestData = "{\"username\":\"testUser\",\"age\":30}"
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Delete_Success()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/delete?username=testUser&age=30",
                Method = HttpMethod.Delete
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Status_404()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/status/404"
            };
            await client.SendAsync();
            Assert.Equal(HttpStatusCode.NotFound, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_400()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/status/400"
            };
            await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadRequest, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_500()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/status/500"
            };

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.InternalServerError, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_502()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/status/502"
            };
            await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
        }

        [Fact]
        public async Task Test_Timeout()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/delay/50",
                Timeout = 1
            };

            var ex = await Record.ExceptionAsync(() => client.SendAsync());
            Assert.IsType<Exception>(ex, exactMatch: false);
        }

        [Fact]
        public async Task Test_Request_Header_Set()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/headers"
            };
            client.RequestHeader.UserAgent = "JfYuHttpClient/1.0";
            client.RequestHeader.Host = "httpbin.org";
            client.RequestHeader.Referer = "http://httpbin.org";
            client.RequestHeader.Accept = "text/html";
            client.RequestHeader.AcceptLanguage = "zh-en";
            client.RequestHeader.CacheControl = "cache";
            client.RequestHeader.Connection = "keep-alive";
            client.RequestHeader.Pragma = "Pragma";
            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("JfYuHttpClient/1.0", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["User-Agent"]!.ToString());
            Assert.Contains("http://httpbin.org", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Referer"]!.ToString());
            Assert.Contains("httpbin.org", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Host"]!.ToString());
            Assert.Contains("text/html", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Accept"]!.ToString());
            Assert.Contains("zh-en", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Accept-Language"]!.ToString());
            Assert.Contains("cache", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Cache-Control"]!.ToString());
            Assert.Contains("Pragma", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Pragma"]!.ToString());
        }

        [Fact]
        public async Task Test_Response_Header_Set()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/response-headers?UserAgent=JfYuHttpClient/1.0&Host=httpbin.org&Referer=http://httpbin.org&Accept=text/html&AcceptLanguage=zh-en&CacheControl=cache&Connection=keep-alive&Pragma=Pragma"
            };
            await client.SendAsync();

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            var headers = client.ResponseHeader.ToDictionary(k => k.Key.ToLower(), v => v.Value);

            Assert.Contains("JfYuHttpClient/1.0", headers["useragent"]);
            Assert.Contains("http://httpbin.org", headers["referer"]);
            Assert.Contains("httpbin.org", headers["host"]);
            Assert.Contains("text/html", headers["accept"]);
            Assert.Contains("zh-en", headers["acceptlanguage"]);
            Assert.Contains("cache", headers["cachecontrol"]);
            Assert.Contains("Pragma", headers["pragma"]);
        }

        [Fact]
        public async Task Test_CustomHeaders()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/headers"
            };
            client.RequestCustomHeaders.Add("X-Custom-Header", "test-value");
            client.Authorization = "Bearer test-token";

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);

            Assert.Contains("test-value", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["X-Custom-Header"]!.ToString());
            Assert.Contains("test-token", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Authorization"]!.ToString());
        }

        [Fact]
        public async Task Test_CustomHeaders_Set()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/headers",
                RequestCustomHeaders = new Dictionary<string, string>() { { "X-Custom-Header", "test-value" } },
                Authorization = "Bearer test-token"
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);

            Assert.Contains("test-value", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["X-Custom-Header"]!.ToString());
            Assert.Contains("test-token", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Authorization"]!.ToString());
        }

        [Fact]
        public async Task Test_No_Cookies()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/cookies/set?c1=v1&c2=v2",
                RequestCookies = null!
            };
            await client.SendAsync();

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Empty(client.ResponseCookies);

            client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/bytes/1024",
                RequestCookies = null!
            };
            using var stream = await client.DownloadFileAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Empty(client.ResponseCookies);

            client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/bytes/1024",
                RequestCookies = null!
            };
            var path = nameof(Test_DownloadFile);
            if (File.Exists(path))
                File.Delete(path);

            await client.DownloadFileAsync(path);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Empty(client.ResponseCookies);
            if (File.Exists(path))
                File.Delete(path);
        }

        [Fact]
        public async Task Test_Cookies_Set()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/cookies/set?c1=v1&c2=v2"
            };

            var cookies = new CookieContainer();
            cookies.Add(new Cookie("test", "value", "/", new Uri(_url.Url).Host));
            cookies.Add(new Cookie("test1", "value1", "/", new Uri(_url.Url).Host));
            client.RequestCookies = cookies;
            await client.SendAsync();

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);

            Assert.Equal("value", client.ResponseCookies["test"]!.Value);
            Assert.Equal("value1", client.ResponseCookies["test1"]!.Value);
            Assert.Equal("v1", client.ResponseCookies["c1"]!.Value);
            Assert.Equal("v2", client.ResponseCookies["c2"]!.Value);
        }

        [Fact]
        public async Task Test_EmptyCookieContainer_SendAsync()
        {
            // Test with empty CookieContainer (not null, but no cookies)
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/get",
                RequestCookies = new CookieContainer() // Empty container
            };
            await client.SendAsync();

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            // ResponseCookies should be empty since no cookies were set
            Assert.Empty(client.ResponseCookies);
        }

        [Fact]
        public async Task Test_EmptyCookieContainer_DownloadFileAsyncToPath()
        {
            // Test with empty CookieContainer for DownloadFileAsync(path)
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/bytes/1024",
                RequestCookies = new CookieContainer() // Empty container
            };

            var path = nameof(Test_EmptyCookieContainer_DownloadFileAsyncToPath);
            if (File.Exists(path))
                File.Delete(path);

            await client.DownloadFileAsync(path);
            
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Empty(client.ResponseCookies);
            Assert.True(File.Exists(path));
            
            if (File.Exists(path))
                File.Delete(path);
        }

        [Fact]
        public async Task Test_EmptyCookieContainer_DownloadFileAsyncToStream()
        {
            // Test with empty CookieContainer for DownloadFileAsync()
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/bytes/1024",
                RequestCookies = new CookieContainer() // Empty container
            };

            using var stream = await client.DownloadFileAsync();
            
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Empty(client.ResponseCookies);
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task Test_CustomInitFunc()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/headers",
                CustomInit = (obj) =>
                    {
                        var WebRequest = (HttpWebRequest)obj;
                        WebRequest.Headers.Add("X-Custom-Init", "test-value");
                    }
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("test-value", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["X-Custom-Init"]!.ToString());
        }

        [Fact]
        public async Task Test_CustomInitFunc_ThrowException()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/get",
                CustomInit = (obj) =>
                    {
                        var i = 0;
                        _ = 1 / i;
                        var httpClient = (HttpClient)obj;
                        httpClient.DefaultRequestHeaders.Add("X-Custom-Init", "test-value");
                    }
            };

            await Assert.ThrowsAsync<DivideByZeroException>(() => client.SendAsync());
        }

        [Fact]
        public async Task Test_InvalidProxy()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"https://httpbin.org/get",
                Proxy = new WebProxy("http://example.cn1", false, ["http://127.0.0.1:8080"]),
            };

            var ex = await Assert.ThrowsAsync<WebException>(() => client.SendAsync());
            Assert.Contains("example", ex.Message);
        }

        [Fact]
        public async Task Test_Certificate_ServerHaveError()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.ErrorSSL}",
                CertificateValidation = true
            };

            var ex = await Assert.ThrowsAsync<WebException>(() => client.SendAsync());
            Assert.Contains("SSL", ex.Message);
        }

        [Fact]
        public async Task Test_Certificate_ServerErrorButDisableValidation()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.ErrorSSL}",
                CertificateValidation = false
            };
            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
        }

        [Fact]
        public async Task Test_Certificate_ClientEmpty()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.ClientSSL}"
            };

            var html = await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadRequest, client.StatusCode);
            Assert.Contains("No required SSL certificate was sent", html);
        }

        [Fact]
        public async Task Test_Certificate_ClientHaveError()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.ClientSSL}"
            };
            static X509Certificate2 GenerateRandomCertificate()
            {
                var subjectName = $"CN=TestCert-{Guid.NewGuid()}";
                using RSA rsa = RSA.Create(2048);
                var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
                DateTimeOffset startTime = DateTimeOffset.UtcNow;
                DateTimeOffset endTime = startTime.AddSeconds(1);
                var certificate = request.CreateSelfSigned(startTime, endTime);

#if NET9_0_OR_GREATER
                var certBytes = certificate.Export(X509ContentType.Cert);   // 导出证书为 DER 格式
                return X509CertificateLoader.LoadCertificate(certBytes);
#else
                return new X509Certificate2(certificate.Export(X509ContentType.Pfx, "password123"), "password123");
#endif
            }
            client.Certificate = GenerateRandomCertificate();
            var html = await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadRequest, client.StatusCode);
            Assert.Contains("SSL certificate", html);
        }

        [Fact]
        public async Task Test_Certificate_ClientSuccess()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.ClientSSL}",
            };
#if NET9_0_OR_GREATER
            var cert = X509CertificateLoader.LoadPkcs12FromFile("Static/badssl.com-client.p12", "badssl.com");
#else
            var cert = new X509Certificate2("Static/badssl.com-client.p12", "badssl.com");
#endif
            client.Certificate = cert;
            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
        }

        public class EncodeData : TheoryData<string>
        {
            public EncodeData()
            {
                Add("gzip");
                Add("deflate");
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    Add("brotli");
            }
        }

        [Theory]
        [ClassData(typeof(EncodeData))]
        public async Task Test_Gzip(string code)
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/{code}"
            };
            client.RequestHeader.AcceptEncoding = code;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains(code, JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Accept-Encoding"]!.ToString());
        }

        #endregion Send

        #region Download

        [Fact]
        public async Task Test_DownloadStream()
        {
            var client = new JfYuHttpRequest
            {
                Url = $"{_url.Url}/bytes/1024"
            };

            using var stream = await client.DownloadFileAsync((p, s, r) =>
            {
                Assert.True(p >= 0);
                Assert.True(s >= 0);
                Assert.True(r >= 0);
            });

            Assert.NotNull(stream);
            Assert.Equal(1024, stream.Length);
        }

        [Fact]
        public async Task Test_DownloadFile()
        {
            var client = new JfYuHttpRequest();
            var path = nameof(Test_DownloadFile);
            if (File.Exists(path))
                File.Delete(path);
            client.Url = $"{_url.Url}/bytes/1024";

            var flag = await client.DownloadFileAsync(path, (p, s, r) =>
            {
                Assert.True(p >= 0);
                Assert.True(s >= 0);
                Assert.True(r >= 0);
            });

            Assert.True(flag);
            Assert.True(File.Exists(path));
            Assert.Equal(1024, new FileInfo(path).Length);
            if (File.Exists(path))
                File.Delete(path);
        }

        [Fact]
        public async Task Test_DownloadFile_Dic()
        {
            var client = new JfYuHttpRequest();
            var path = $"download/{nameof(Test_DownloadFile_Dic)}";
            if (File.Exists(path))
                File.Delete(path);
            client.Url = $"{_url.Url}/bytes/1024";

            var flag = await client.DownloadFileAsync(path, (p, s, r) =>
            {
                Assert.True(p >= 0);
                Assert.True(s >= 0);
                Assert.True(r >= 0);
            });

            Assert.True(flag);
            Assert.True(File.Exists(path));
            Assert.Equal(1024, new FileInfo(path).Length);
            if (File.Exists(path))
                File.Delete(path);
        }

        [Fact]
        public async Task Test_DownloadFile_ContentLengthNull()
        {
            var client = new JfYuHttpRequest();
            var path = nameof(Test_DownloadFile_ContentLengthNull);
            if (File.Exists(path))
                File.Delete(path);
            client.Url = "http://httpbin.org/status/204";

            using var stream = await client.DownloadFileAsync();
            var flag = await client.DownloadFileAsync(path);

            Assert.NotNull(stream);
            Assert.True(0 <= stream.Length);

            Assert.True(flag);
            Assert.True(File.Exists(path));
            Assert.Equal(0, new FileInfo(path).Length);

            if (File.Exists(path))
                File.Delete(path);
        }

        [Fact]
        public async Task Test_DownloadFile_Get500()
        {
            var client = new JfYuHttpRequest();
            var path = nameof(Test_DownloadFile_ContentLengthNull);
            client.Url = $"{_url.Url}/status/500";
            var ex = await Record.ExceptionAsync(() => client.DownloadFileAsync());
            Assert.IsType<Exception>(ex, exactMatch: false);
            var ex1 = await Record.ExceptionAsync(() => client.DownloadFileAsync(path));
            Assert.IsType<Exception>(ex1, exactMatch: false);
            Assert.False(File.Exists(path));
        }

        [Fact]
        public async Task Test_DownloadFile_Timeout()
        {
            var client = new JfYuHttpRequest();
            var path = nameof(Test_DownloadFile_Timeout);
            client.Url = $"{_url.Url}/delay/5";
            client.Timeout = 1;

            var ex = await Record.ExceptionAsync(() => client.DownloadFileAsync());
            Assert.IsType<Exception>(ex, exactMatch: false);

            var ex1 = await Record.ExceptionAsync(() => client.DownloadFileAsync(path));
            Assert.IsType<Exception>(ex1, exactMatch: false);
        }

        [Fact]
        public async Task Test_DownloadFile_PathIsEmpty()
        {
            var client = new JfYuHttpRequest();
            var path = "";
            client.Url = $"{_url.Url}/bytes/1024";

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.DownloadFileAsync(path));
        }

        #endregion Download

        #region Logger

        [Fact]
        public async Task Test_Send_Correctly()
        {
            var logger = new Mock<ILogger<JfYuHttpRequest>>();
            var client = new JfYuHttpRequest(new LogFilter() { LoggingFields = JfYuLoggingFields.All }, logger.Object)
            {
                Url = $"{_url.Url}/get?x=y",
                Method = HttpMethod.Get
            };
            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(2));
        }

        public class LoggingFieldsData : TheoryData<JfYuLoggingFields>
        {
            public LoggingFieldsData()
            {
                Add(JfYuLoggingFields.All);
                Add(JfYuLoggingFields.RequestData);
                Add(JfYuLoggingFields.RequestHeaders);
                Add(JfYuLoggingFields.RequestMethod);
                Add(JfYuLoggingFields.RequestPath);
                Add(JfYuLoggingFields.RequestAll);
                Add(JfYuLoggingFields.Response);
                Add(JfYuLoggingFields.ResponseStatus);
                Add(JfYuLoggingFields.ResponseAll);
            }
        }

        [Theory]
        [ClassData(typeof(LoggingFieldsData))]
        public async Task Test_Send_WithLoggingFields(JfYuLoggingFields loggingFields)
        {
            var logger = new Mock<ILogger<JfYuHttpRequest>>();
            var client = new JfYuHttpRequest(new LogFilter() { LoggingFields = loggingFields }, logger.Object)
            {
                Url = $"{_url.Url}/get?x=y",
                Method = HttpMethod.Get
            };

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task Test_InvalidProxy_Logger()
        {
            var logger = new Mock<ILogger<JfYuHttpRequest>>();
            var client = new JfYuHttpRequest(null, logger.Object)
            {
                Url = $"https://httpbin.org/get",
                Proxy = new WebProxy("http://example.cn1"),
            };

            var ex = await Assert.ThrowsAsync<WebException>(() => client.SendAsync());
            Assert.Contains("example", ex.Message);
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
        }

        #endregion Logger

        #region Exception With Logger

        [Fact]
        public async Task Test_Filter_ThrowException()
        {
            var logger = new Mock<ILogger<JfYuHttpRequest>>();
            var client = new JfYuHttpRequest(new LogFilter() { RequestFilter = z => { var x = 0; return (1 / x).ToString(); }, ResponseFilter = z => z }, logger.Object)
            {
                Url = $"{_url.Url}/get"
            };

            var ex = await Record.ExceptionAsync(() => client.SendAsync());
            Assert.IsType<Exception>(ex, exactMatch: false);
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Test_Send_Init_ThrowException()
        {
            var logger = new Mock<ILogger<JfYuHttpRequest>>();
            var client = new JfYuHttpRequest(null, logger.Object)
            {
                Url = $"{_url.Url}/get",
                CustomInit = (obj) =>
                        {
                            var i = 0;
                            _ = 1 / i;
                            var httpClient = (HttpClient)obj;
                            httpClient.DefaultRequestHeaders.Add("X-Custom-Init", "test-value");
                        }
            };

            await Assert.ThrowsAsync<DivideByZeroException>(() => client.SendAsync());
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Test_DownloadFile_ProgressThrowException()
        {
            var logger = new Mock<ILogger<JfYuHttpRequest>>();
            var client = new JfYuHttpRequest(null, logger.Object);
            var path = nameof(Test_DownloadFile_ProgressThrowException);
            client.Url = $"{_url.Url}/bytes/1024";

            var ex = await Record.ExceptionAsync(() => client.DownloadFileAsync(path, (q, w, e) => { int.Parse("x"); }));
            var ex1 = await Record.ExceptionAsync(() => client.DownloadFileAsync((q, w, e) => { int.Parse("x"); }));
            Assert.IsType<Exception>(ex, exactMatch: false);
            Assert.IsType<Exception>(ex1, exactMatch: false);
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
        }

        #endregion Exception With Logger

        #region Filter

        [Fact]
        public async Task Test_Get_Filter_Success()
        {
            var client = new JfYuHttpRequest(new LogFilter() { RequestFilter = z => z, ResponseFilter = z => z })
            {
                Url = $"{_url.Url}/get?username=testUser&age=30",
                Method = HttpMethod.Get
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Get_NoneFilter_Success()
        {
            var client = new JfYuHttpRequest(null)
            {
                Url = $"{_url.Url}/get?username=testUser&age=30",
                Method = HttpMethod.Get
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Use_Constructor()
        {
            var request = new JfYuHttpRequest
            {
                Method = HttpMethod.Get,
                Url = $"{_url.Url}/get?username=testUser&age=30"
            };
            var response = await request.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        #endregion Filter
    }
}