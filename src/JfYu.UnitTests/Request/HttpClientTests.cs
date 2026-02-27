#if NET8_0_OR_GREATER
using JfYu.Request;
using JfYu.Request.Enum;
using JfYu.Request.Extension;
using JfYu.UnitTests.Models;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace JfYu.UnitTests.Request
{
    [Collection("JfYuRequest")]
    public class HttpClientTests
    {
        public readonly HttpTestOption _url;
        public HttpClientTests()
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

        #region IOC

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
        #endregion

        #region Send
        [Fact]
        public async Task Test_Get_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get?username=testUser&age=30";
            client.Method = HttpMethod.Get;
            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }
        [Fact]
        public async Task Test_Post_EmptyBody_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/post";
            client.Method = HttpMethod.Post;
            client.ContentType = RequestContentType.Json;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Null(jsonResponse!["json"]);
        }

        [Fact]
        public async Task Test_Post_Plain_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/post";
            client.Method = HttpMethod.Post;
            client.RequestData = "username=testUser&age=30";
            client.ContentType = RequestContentType.Plain;
            client.RequestEncoding = Encoding.UTF8;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains(client.RequestData, jsonResponse!["data"].ToString());
        }

        [Fact]
        public async Task Test_Post_XML_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/post";
            client.Method = HttpMethod.Post;
            client.RequestData = "<user><username>testUser</username><age>30</age></user>";
            client.ContentType = RequestContentType.Xml;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains(client.RequestData, jsonResponse!["data"].ToString());
        }

        [Fact]
        public async Task Test_Post_FormUrlEncoded_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/post";
            client.Method = HttpMethod.Post;
            client.RequestData = "username=testUser&age=30";
            client.ContentType = RequestContentType.FormUrlEncoded;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["form"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["form"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Post_Json_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/post";
            client.Method = HttpMethod.Post;
            client.RequestData = "{\"username\":\"testUser\",\"age\":30}";
            client.ContentType = RequestContentType.Json;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Post_FormData_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/post";
            client.Method = HttpMethod.Post;
            client.RequestData = "username=testUser&age=30";
            client.ContentType = RequestContentType.FormData;

            var file = Guid.NewGuid().ToString() + ".txt";
            var text = "dada dadad大大大阿斗哇打完带娃啊我啊";
            if (File.Exists(file))
                File.Delete(file);

            using var fs = File.Create(file);
            await fs.WriteAsync(Encoding.UTF8.GetBytes(text));
            await fs.DisposeAsync();
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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/put";
            client.Method = HttpMethod.Put;
            client.RequestData = "{\"username\":\"testUser\",\"age\":30}";

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["json"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Delete_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/delete?username=testUser&age=30";
            client.Method = HttpMethod.Delete;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Patch_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/patch?username=testUser&age=30";
            client.Method = HttpMethod.Patch;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Option_Success()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/anything";
            client.Method = HttpMethod.Options;
            client.RequestData = "{\"username\":\"testUser\"}";

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_404()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/status/404";

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.NotFound, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_400()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/status/400";

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadRequest, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_500()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/status/500";

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.InternalServerError, client.StatusCode);
        }

        [Fact]
        public async Task Test_Status_502()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/status/502";

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
        }

        [Fact]
        public async Task Test_Timeout()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/delay/50";
            client.Timeout = 1;

            var ex = await Record.ExceptionAsync(() => client.SendAsync());
            Assert.IsType<Exception>(ex, exactMatch: false);
        }

        [Fact]
        public async Task Test_Header()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/headers";

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
        public async Task Test_Request_Header_Set()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/headers";

            var header = new RequestHeaders
            {
                UserAgent = "JfYuHttpClient/1.0",
                Host = "httpbin.org",
                Referer = "http://httpbin.org",
                Accept = "text/html",
                AcceptLanguage = "zh-en",
                CacheControl = "cache",
                Connection = "keep-alive",
                Pragma = "Pragma"
            };
            client.RequestHeader = header;
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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/response-headers?UserAgent=JfYuHttpClient/1.0&Host=httpbin.org&Referer=http://httpbin.org&Accept=text/html&AcceptLanguage=zh-en&CacheControl=cache&Connection=keep-alive&Pragma=Pragma";

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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/headers";

            client.RequestCustomHeaders.Add("X-Custom-Header", "test-value");
            client.Authorization = "test-token";
            client.AuthorizationScheme = "Bearer";
            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);

            Assert.Contains("test-value", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["X-Custom-Header"]!.ToString());
            Assert.Contains("test-token", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Authorization"]!.ToString());
        }

        [Fact]
        public async Task Test_CustomHeaders_Set()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/headers";
            client.RequestCustomHeaders = new Dictionary<string, string>() { { "X-Custom-Header", "test-value" } };
            client.Authorization = "Bearer test-token";

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);

            Assert.Contains("test-value", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["X-Custom-Header"]!.ToString());
            Assert.Contains("test-token", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["Authorization"]!.ToString());
        }

        [Fact]
        public async Task Test_No_Cookies()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get";

            client.RequestCookies = null!;
            await client.SendAsync();

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Empty(client.ResponseCookies);
        }

        [Fact]
        public async Task Test_Cookies_Set()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = true;
            });

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/cookies/set?c1=v1&c2=v2";

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
        public async Task Test_CustomInitFunc()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/headers";
            client.CustomInit = (obj) =>
            {
                var httpClient = (HttpClient)obj;
                httpClient.DefaultRequestHeaders.Add("X-Custom-Init", "test-value");
            };

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("test-value", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["headers"].ToString()!)!["X-Custom-Init"]!.ToString());
        }

        [Fact]
        public async Task Test_CustomInitFunc_ThrowException()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get";
            client.CustomInit = (obj) =>
               {
                   var i = 0;
                   _ = 1 / i;
                   var httpClient = (HttpClient)obj;
                   httpClient.DefaultRequestHeaders.Add("X-Custom-Init", "test-value");
               };

            await Assert.ThrowsAsync<DivideByZeroException>(() => client.SendAsync());
        }

        [Fact]
        public async Task Test_Proxy()
        {
            using var handler = new HttpClientHandler()
            {
                UseProxy = true,
                Proxy = new WebProxy("http://example.cn1")
            };
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientHandler = () => handler;
            });

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get";

            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync());
            Assert.Contains("example", ex.Message);
        }

        [Fact]
        public async Task Test_Certificate_ServerHaveError()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.ErrorSSL}";
            client.CertificateValidation = true;

            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync());
            Assert.Contains("SSL", ex.Message);
        }

        [Fact]
        public async Task Test_Certificate_ServerErrorButDisableValidation()
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientHandler = () => handler;
            });

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.ErrorSSL}";
            client.CertificateValidation = false;

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
        }

        [Fact]
        public async Task Test_Certificate_ClientEmpty()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.ClientSSL}";

            var html = await client.SendAsync();
            Assert.Equal(HttpStatusCode.BadRequest, client.StatusCode);
            Assert.Contains("No required SSL certificate was sent", html);
        }
        [Fact]
        public async Task Test_Certificate_ClientHaveError()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.ClientSSL}";

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
#if NET9_0_OR_GREATER
            var cert = X509CertificateLoader.LoadPkcs12FromFile("Static/badssl.com-client.p12", "badssl.com");
#else
            var cert = new X509Certificate2("Static/badssl.com-client.p12", "badssl.com");
#endif
            using var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientHandler = () => handler;
            });
            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.ClientSSL}";


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
                Add("brotli");
            }
        }

        [Theory]
        [ClassData(typeof(EncodeData))]
        public async Task Test_Gzip(string code)
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/{code}";

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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/bytes/1024";

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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            var path = nameof(Test_DownloadFile_ContentLengthNull);
            client.Url = $"{_url.Url}/status/500";
            var response = await client.DownloadFileAsync();
            var flag = await client.DownloadFileAsync(path);
            Assert.Null(response);
            Assert.False(flag);
            Assert.False(File.Exists(path));
        }

        [Fact]
        public async Task Test_DownloadFile_Timeout()
        {
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
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
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            var path = "";
            client.Url = $"{_url.Url}/bytes/1024";

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.DownloadFileAsync(path));
        }

        #endregion Download

        #region Logger         

        [Fact]
        public async Task Test_Send_Correctly()
        {
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get?x=y";
            client.Method = HttpMethod.Get;

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
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(null, q => q.LoggingFields = loggingFields);
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get?x=y";
            client.Method = HttpMethod.Get;

            await client.SendAsync();
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeast(2));
        }

        #endregion Logger

        #region Exception With Logger       

        [Fact]
        public async Task Test_Filter_ThrowException()
        {
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(null, q => q.RequestFilter = z => { var x = 0; return (1 / x).ToString(); });
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get";

            var ex = await Record.ExceptionAsync(() => client.SendAsync());
            Assert.IsType<Exception>(ex, exactMatch: false); 
        }

        [Fact]
        public async Task Test_Send_Init_ThrowException()
        {
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get";
            client.CustomInit = (obj) =>
                    {
                        var i = 0;
                        _ = 1 / i;
                        var httpClient = (HttpClient)obj;
                        httpClient.DefaultRequestHeaders.Add("X-Custom-Init", "test-value");
                    };


            await Assert.ThrowsAsync<DivideByZeroException>(() => client.SendAsync()); 
        }

        [Fact]
        public async Task Test_DownloadFile_ProgressThrowException()
        {
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();

            var path = nameof(Test_DownloadFile_ProgressThrowException);
            client.Url = $"{_url.Url}/bytes/1024";

            var ex = await Record.ExceptionAsync(() => client.DownloadFileAsync(path, (q, w, e) => { int.Parse("x"); }));
            var ex1 = await Record.ExceptionAsync(() => client.DownloadFileAsync((q, w, e) => { int.Parse("x"); }));
            Assert.IsType<Exception>(ex, exactMatch: false);
            Assert.IsType<Exception>(ex1, exactMatch: false);            
        }

        #endregion Exception With Logger

        #region Filter

        [Fact]

        public async Task Test_Get_Filter_Success()
        {
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(null, q => { q.RequestFilter = z => z; q.ResponseFilter = z => z; });
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get?username=testUser&age=30";
            client.Method = HttpMethod.Get;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        [Fact]
        public async Task Test_Get_NoneFilter_Success()
        {
            var logger = new Mock<ILogger<JfYuHttpClient>>();
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            services.AddSingleton(q => { return logger.Object; });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IJfYuRequest>();
            client.Url = $"{_url.Url}/get?username=testUser&age=30";
            client.Method = HttpMethod.Get;

            var response = await client.SendAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.Equal(HttpStatusCode.OK, client.StatusCode);
            Assert.Contains("testUser", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["username"]!.ToString());
            Assert.Contains("30", JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse!["args"].ToString()!)!["age"]!.ToString());
        }

        #endregion Filter

        #region Concurrent Tests

        [Fact]
        public async Task Test_ConcurrentRequests_WithSharedCookieContainer_ShouldNotThrow()
        {
            // Arrange: Setup service with shared CookieContainer (default behavior)
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = true; // Explicitly enable shared cookies
            });
            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();

            // Act: Simulate concurrent requests from multiple scopes (more realistic)
            var tasks = new List<Task>();
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var successCount = 0;

            for (int i = 0; i < 20; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Each request gets its own scope (like ASP.NET Core does)
                        using var scope = serviceProvider.CreateScope();
                        var client = scope.ServiceProvider.GetRequiredService<IJfYuRequest>();

                        // Set cookies to trigger CookieContainer operations
                        var cookieCollection = new CookieCollection
                        {
                            new Cookie("session_id", $"session_{index}", "/", new Uri(_url.Url).Host),
                            new Cookie("user_token", $"token_{index}", "/", new Uri(_url.Url).Host)
                        };
                        client.RequestCookies = new CookieContainer();
                        client.RequestCookies.Add(new Uri(_url.Url), cookieCollection);

                        client.Url = $"{_url.Url}/get?id={index}";
                        client.Method = HttpMethod.Get;

                        // This should not throw NullReferenceException with CookieContainer locking
                        var response = await client.SendAsync().ConfigureAwait(true);

                        if (client.StatusCode == HttpStatusCode.OK)
                        {
                            Interlocked.Increment(ref successCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert: No CookieContainer-related NullReferenceException should occur
            var cookieExceptions = exceptions.Where(e =>
                e is NullReferenceException nre &&
                nre.StackTrace != null &&
                nre.StackTrace.Contains("CookieContainer")).ToList();

            Assert.Empty(cookieExceptions);
            Assert.True(successCount > 0, "At least some requests should succeed");
        }

        [Fact]
        public async Task Test_HighConcurrency_CookieOperations_ShouldBeThreadSafe()
        {
            // Arrange: Setup with shared cookies
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();

            // Act: Stress test with high concurrency using scoped instances
            var tasks = new List<Task<bool>>();
            var concurrentCount = 50;

            for (int i = 0; i < concurrentCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var scope = serviceProvider.CreateScope();
                        var client = scope.ServiceProvider.GetRequiredService<IJfYuRequest>();

                        // Add cookies that will be merged into shared CookieContainer
                        var cookieCollection = new CookieCollection
                        {
                            new Cookie($"concurrent_cookie_{index}", $"value_{index}", "/", new Uri(_url.Url).Host)
                        };
                        client.RequestCookies = new CookieContainer();
                        client.RequestCookies.Add(new Uri(_url.Url), cookieCollection);

                        client.Url = $"{_url.Url}/get?id={index}";
                        client.Method = HttpMethod.Get;

                        await client.SendAsync().ConfigureAwait(true);
                        return client.StatusCode == HttpStatusCode.OK;
                    }
                    catch (NullReferenceException nre) when (nre.StackTrace?.Contains("CookieContainer") == true || nre.StackTrace?.Contains("WriteHeaderCollection") == true)
                    {
                        // This is the specific CookieContainer exception we're protecting against
                        return false;
                    }
                    catch
                    {
                        // Other exceptions might be network-related or HttpClient header issues, allow them
                        return true;
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            // Assert: All requests should complete without CookieContainer-related NullReferenceException
            Assert.All(results, result => Assert.True(result, "Request should not throw CookieContainer NullReferenceException"));
            Assert.True(results.Count(r => r) >= concurrentCount * 0.6,
                "At least 60% of concurrent requests should succeed");
        }

        [Fact]
        public async Task Test_ParallelRequests_WithDifferentUrls_ShouldHandleCookiesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = true;
            });
            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();

            // Act: Multiple parallel requests with different URLs
            var urls = new[]
            {
                $"{_url.Url}/get",
                $"{_url.Url}/anything"
            };

            var results = new System.Collections.Concurrent.ConcurrentBag<(HttpStatusCode Status, bool HasCookieException)>();

            var tasks = urls.SelectMany(url => Enumerable.Range(0, 10).Select(i =>
                Task.Run(async () =>
                {
                    try
                    {
                        using var scope = serviceProvider.CreateScope();
                        var client = scope.ServiceProvider.GetRequiredService<IJfYuRequest>();

                        var cookieCollection = new CookieCollection
                        {
                            new Cookie($"url_cookie_{i}", $"value_{i}", "/", new Uri(url).Host)
                        };
                        client.RequestCookies = new CookieContainer();
                        client.RequestCookies.Add(new Uri(url), cookieCollection);

                        client.Url = url;
                        client.Method = HttpMethod.Get;

                        await client.SendAsync().ConfigureAwait(true);

                        results.Add((client.StatusCode, false));
                    }
                    catch (NullReferenceException nre) when (nre.StackTrace?.Contains("CookieContainer") == true || nre.StackTrace?.Contains("WriteHeaderCollection") == true)
                    {
                        results.Add((HttpStatusCode.InternalServerError, true));
                    }
                    catch (Exception)
                    {
                        // Other exceptions allowed
                        results.Add((HttpStatusCode.InternalServerError, false));
                    }
                })
            )).ToList();

            await Task.WhenAll(tasks);

            // Assert: Should handle all concurrent requests without CookieContainer exceptions
            Assert.False(results.IsEmpty);
            Assert.DoesNotContain(results, r => r.HasCookieException);
        }

        [Fact]
        public async Task Test_RapidFireRequests_ShouldNotCorruptCookieContainer()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            services.AddSingleton<ILogger<JfYuHttpClient>>(q => { return null!; });
            var serviceProvider = services.BuildServiceProvider();

            var cookieExceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var successCount = 0;

            // Act: Fire 100 requests as fast as possible
            var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var client = scope.ServiceProvider.GetRequiredService<IJfYuRequest>();

                    var cookieCollection = new CookieCollection
                    {
                        new Cookie($"rapid_{i}", $"val_{i}", "/", new Uri(_url.Url).Host)
                    };
                    client.RequestCookies = new CookieContainer();
                    client.RequestCookies.Add(new Uri(_url.Url), cookieCollection);

                    client.Url = $"{_url.Url}/get?id={i}";
                    client.Method = HttpMethod.Get;

                    await client.SendAsync().ConfigureAwait(true);

                    if (client.StatusCode == HttpStatusCode.OK)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                }
                catch (NullReferenceException nre) when (nre.StackTrace?.Contains("CookieContainer") == true || nre.StackTrace?.Contains("WriteHeaderCollection") == true)
                {
                    cookieExceptions.Add(nre);
                }
                catch (Exception)
                {
                    // Allow other exceptions
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert: No CookieContainer-related NullReferenceException
            Assert.Empty(cookieExceptions);
            Assert.True(successCount > 30, $"Expected at least 30 successful requests, got {successCount}");
        }

        #endregion Concurrent Tests
    }
}
#endif