#if NET8_0_OR_GREATER
using JfYu.Request;
using JfYu.Request.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using JfYu.UnitTests.Models;
using JfYu.Request.Enum;

namespace JfYu.UnitTests.Request
{
    /// <summary>
    /// Unit tests for multiple named HttpClient configurations and IJfYuRequestFactory.
    /// </summary>
    [Collection("JfYuRequest")]
    public class HttpClientFactoryTests
    {
        private readonly HttpTestOption _url;

        public HttpClientFactoryTests()
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

        #region Factory Registration Tests

        [Fact]
        public void RegisterFactory_SingleClient_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IJfYuRequestFactory>();

            // Assert
            Assert.NotNull(factory);
        }

        [Fact]
        public void RegisterFactory_WithOptions_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "TestClient";
                options.UseSharedCookieContainer = true;
            });

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IJfYuRequestFactory>();

            // Assert
            Assert.NotNull(factory);
        }

        [Fact]
        public void RegisterFactory_MultipleClients_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "Client1");
            services.AddJfYuHttpClient(options => options.HttpClientName = "Client2");
            services.AddJfYuHttpClient(options => options.HttpClientName = "Client3");

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IJfYuRequestFactory>();

            // Assert
            Assert.NotNull(factory);
        }

        #endregion

        #region Factory CreateRequest Tests

        [Fact]
        public async Task CreateRequest_DefaultClient_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest();
            request.Url = $"{_url.Url}/get";
            request.Method = HttpMethod.Get;
            await request.SendAsync();

            // Assert
            Assert.NotNull(request);
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        }

        [Fact]
        public async Task CreateRequest_NamedClient_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "ApiClient";
                options.ConfigureClient = client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                };
            });
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("ApiClient");
            request.Url = $"{_url.Url}/get?test=value";
            request.Method = HttpMethod.Get;
            var response = await request.SendAsync();

            // Assert
            Assert.NotNull(request);
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            Assert.Contains("value", response);
        }

        [Fact]
        public async Task CreateRequest_MultipleDifferentClients_Success()
        {
            // Arrange
            var services = new ServiceCollection();

            // Client 1: Short timeout
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "FastClient";
                options.ConfigureClient = client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                };
            });

            // Client 2: Long timeout
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "SlowClient";
                options.ConfigureClient = client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var fastRequest = factory.CreateRequest("FastClient");
            var slowRequest = factory.CreateRequest("SlowClient");

            fastRequest.Url = $"{_url.Url}/get?client=fast";
            fastRequest.Method = HttpMethod.Get;

            slowRequest.Url = $"{_url.Url}/get?client=slow";
            slowRequest.Method = HttpMethod.Get;

            var fastResponse = await fastRequest.SendAsync();
            var slowResponse = await slowRequest.SendAsync();

            // Assert
            Assert.NotNull(fastRequest);
            Assert.NotNull(slowRequest);
            Assert.Equal(HttpStatusCode.OK, fastRequest.StatusCode);
            Assert.Equal(HttpStatusCode.OK, slowRequest.StatusCode);
            Assert.Contains("fast", fastResponse);
            Assert.Contains("slow", slowResponse);
        }

        #endregion

        #region Named Client Configuration Tests

        [Fact]
        public async Task NamedClient_WithCustomHeaders_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "CustomHeaderClient";
                options.ConfigureClient = client =>
                {
                    client.DefaultRequestHeaders.Add("X-Custom-Header", "TestValue");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "v1");
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("CustomHeaderClient");
            request.Url = $"{_url.Url}/headers";
            request.Method = HttpMethod.Get;
            var response = await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse?["headers"].ToString()!);
            Assert.Contains("X-Custom-Header", headers!.Keys);
            Assert.Contains("X-Api-Version", headers.Keys);
        }

        [Fact]
        public async Task NamedClient_WithBaseAddress_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "BaseAddressClient";
                options.ConfigureClient = client =>
                {
                    client.BaseAddress = new Uri(_url.Url);
                    client.Timeout = TimeSpan.FromMinutes(5);
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("BaseAddressClient");
            request.Url = $"{_url.Url}/get?base=address"; // Use full URL (JfYuHttpClient doesn't support relative URLs)
            request.Method = HttpMethod.Get;
            var response = await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            Assert.Contains("address", response);
        }

        [Fact]
        public void NamedClient_WithProxy_CanCreate()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "ProxyClient";
                options.HttpClientHandler = () => new HttpClientHandler
                {
                    Proxy = new WebProxy("http://proxy.example.com:8080"),
                    UseProxy = false // Don't actually use proxy in tests
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("ProxyClient");

            // Assert
            Assert.NotNull(request);
        }

        [Fact]
        public void NamedClient_IsolatedCookies_Success()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "SharedCookieClient";
                options.UseSharedCookieContainer = true;
            });

            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "IsolatedCookieClient";
                options.UseSharedCookieContainer = false;
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var sharedRequest = factory.CreateRequest("SharedCookieClient");
            var isolatedRequest = factory.CreateRequest("IsolatedCookieClient");

            // Assert
            Assert.NotNull(sharedRequest);
            Assert.NotNull(isolatedRequest);
        }

        #endregion

        #region Multiple Requests with Same Client Tests

        [Fact]
        public async Task CreateRequest_SameClientMultipleTimes_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "ReusableClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request1 = factory.CreateRequest("ReusableClient");
            var request2 = factory.CreateRequest("ReusableClient");
            var request3 = factory.CreateRequest("ReusableClient");

            request1.Url = $"{_url.Url}/get?request=1";
            request2.Url = $"{_url.Url}/get?request=2";
            request3.Url = $"{_url.Url}/get?request=3";

            request1.Method = HttpMethod.Get;
            request2.Method = HttpMethod.Get;
            request3.Method = HttpMethod.Get;

            var response1 = await request1.SendAsync();
            var response2 = await request2.SendAsync();
            var response3 = await request3.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, request2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, request3.StatusCode);
            Assert.Contains("request=1", response1);
            Assert.Contains("request=2", response2);
            Assert.Contains("request=3", response3);
        }

        #endregion

        #region POST/PUT/DELETE with Named Clients

        [Fact]
        public async Task NamedClient_Post_WithJsonBody_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "JsonClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("JsonClient");
            request.Url = $"{_url.Url}/post";
            request.Method = HttpMethod.Post;
            request.ContentType = RequestContentType.Json;
            request.RequestData = "{\"username\":\"testUser\",\"age\":30}";

            var response = await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            Assert.NotNull(jsonResponse!["json"]);
        }

        [Fact]
        public async Task NamedClient_Put_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "PutClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("PutClient");
            request.Url = $"{_url.Url}/put";
            request.Method = HttpMethod.Put;
            request.ContentType = RequestContentType.Json;
            request.RequestData = "{\"data\":\"updated\"}";

            await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        }

        [Fact]
        public async Task NamedClient_Delete_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "DeleteClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("DeleteClient");
            request.Url = $"{_url.Url}/delete";
            request.Method = HttpMethod.Delete;

            await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        }

        #endregion

        #region Logging with Named Clients

        [Fact]
        public async Task NamedClient_WithLoggingFilter_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "LoggingClient";
            }, filter =>
            {
                filter.LoggingFields = JfYuLoggingFields.All;
                filter.RequestFilter = req => req.Replace("sensitive", "***");
                filter.ResponseFilter = resp => resp.Replace("secret", "***");
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("LoggingClient");
            request.Url = $"{_url.Url}/get";
            request.Method = HttpMethod.Get;
            await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        }

        #endregion

        #region Timeout Configuration Tests

        [Fact]
        public async Task NamedClient_CustomTimeout_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "LongTimeoutClient";
                options.ConfigureClient = client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("LongTimeoutClient");
            request.Url = $"{_url.Url}/delay/1"; // 1 second delay
            request.Method = HttpMethod.Get;
            await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        }

        #endregion

        #region Download File Tests with Named Clients

        [Fact]
        public async Task NamedClient_DownloadFile_ToStream_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "DownloadClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("DownloadClient");
            request.Url = $"{_url.Url}/bytes/1024";
            request.Method = HttpMethod.Get;

            using var stream = await request.DownloadFileAsync();

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream!.Length > 0);
        }

        [Fact]
        public async Task NamedClient_DownloadFile_ToPath_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "FileDownloadClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_download_{Guid.NewGuid()}.bin");

            try
            {
                // Act
                var request = factory.CreateRequest("FileDownloadClient");
                request.Url = $"{_url.Url}/bytes/2048";
                request.Method = HttpMethod.Get;

                var success = await request.DownloadFileAsync(tempPath);

                // Assert
                Assert.True(success);
                Assert.True(File.Exists(tempPath));
                var fileInfo = new FileInfo(tempPath);
                Assert.True(fileInfo.Length > 0);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        #endregion

        #region Multi-Tenant Scenario Tests

        [Fact]
        public async Task MultiTenant_DifferentClientsForDifferentTenants_Success()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "TenantA";
                options.ConfigureClient = client =>
                {
                    client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-a");
                };
            });

            services.AddJfYuHttpClient(options =>
            {
                options.HttpClientName = "TenantB";
                options.ConfigureClient = client =>
                {
                    client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-b");
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var tenantARequest = factory.CreateRequest("TenantA");
            var tenantBRequest = factory.CreateRequest("TenantB");

            tenantARequest.Url = $"{_url.Url}/headers";
            tenantARequest.Method = HttpMethod.Get;

            tenantBRequest.Url = $"{_url.Url}/headers";
            tenantBRequest.Method = HttpMethod.Get;

            var tenantAResponse = await tenantARequest.SendAsync();
            var tenantBResponse = await tenantBRequest.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, tenantARequest.StatusCode);
            Assert.Equal(HttpStatusCode.OK, tenantBRequest.StatusCode);

            var tenantAHeaders = JsonSerializer.Deserialize<Dictionary<string, object>>(tenantAResponse);
            var tenantBHeaders = JsonSerializer.Deserialize<Dictionary<string, object>>(tenantBResponse);

            var aHeaders = JsonSerializer.Deserialize<Dictionary<string, string>>(tenantAHeaders!["headers"].ToString()!);
            var bHeaders = JsonSerializer.Deserialize<Dictionary<string, string>>(tenantBHeaders!["headers"].ToString()!);

            Assert.Equal("tenant-a", aHeaders!["X-Tenant-Id"]);
            Assert.Equal("tenant-b", bHeaders!["X-Tenant-Id"]);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task NamedClient_InvalidUrl_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "ErrorClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act & Assert
            var request = factory.CreateRequest("ErrorClient");
            request.Url = "http://invalid-domain-that-does-not-exist-12345.com/api";
            request.Method = HttpMethod.Get;

            await Assert.ThrowsAsync<HttpRequestException>(async () => await request.SendAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task NamedClient_404NotFound_ReturnsNotFoundStatus()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options => options.HttpClientName = "NotFoundClient");
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();

            // Act
            var request = factory.CreateRequest("NotFoundClient");
            request.Url = $"{_url.Url}/status/404";
            request.Method = HttpMethod.Get;

            try
            {
                await request.SendAsync();
            }
            catch
            {
                // Some servers throw on 404, some don't
            }

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, request.StatusCode);
        }

        #endregion

        #region Backward Compatibility Tests

        [Fact]
        public async Task DirectInjection_StillWorks_Success()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Act - Using old pattern (direct IJfYuRequest injection)
            var request = serviceProvider.GetRequiredService<IJfYuRequest>();
            request.Url = $"{_url.Url}/get";
            request.Method = HttpMethod.Get;
            await request.SendAsync();

            // Assert
            Assert.NotNull(request);
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        }

        [Fact]
        public void BothInjectionPatterns_CanCoexist()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var directRequest = serviceProvider.GetService<IJfYuRequest>();
            var factory = serviceProvider.GetService<IJfYuRequestFactory>();
            var factoryRequest = factory?.CreateRequest();

            // Assert
            Assert.NotNull(directRequest);
            Assert.NotNull(factory);
            Assert.NotNull(factoryRequest);
        }

        #endregion

        #region CookieContainer DI Resolution Tests (UseSharedCookieContainer=false)

        [Fact]
        public void Resolve_IJfYuRequest_WithoutSharedCookie_ShouldNotThrow()
        {
            // Arrange: Default registration (UseSharedCookieContainer=false)
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert: Should resolve without InvalidOperationException
            var request = serviceProvider.GetRequiredService<IJfYuRequest>();
            Assert.NotNull(request);
        }

        [Fact]
        public void Resolve_IJfYuRequestFactory_WithoutSharedCookie_ShouldNotThrow()
        {
            // Arrange: Default registration (UseSharedCookieContainer=false)
            var services = new ServiceCollection();
            services.AddJfYuHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert: Should resolve without InvalidOperationException
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void Resolve_IJfYuRequest_WithExplicitFalse_ShouldNotThrow()
        {
            // Arrange: Explicitly set UseSharedCookieContainer=false
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert: Should resolve without InvalidOperationException
            var request = serviceProvider.GetRequiredService<IJfYuRequest>();
            Assert.NotNull(request);
        }

        [Fact]
        public void Resolve_IJfYuRequestFactory_WithExplicitFalse_ShouldNotThrow()
        {
            // Arrange: Explicitly set UseSharedCookieContainer=false
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert: Should resolve without InvalidOperationException
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();
            Assert.NotNull(factory);
            var request = factory.CreateRequest();
            Assert.NotNull(request);
        }

        [Fact]
        public async Task Request_WithoutSharedCookie_SendAsync_Success()
        {
            // Arrange: No shared cookie container
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var request = serviceProvider.GetRequiredService<IJfYuRequest>();
            request.Url = $"{_url.Url}/get?test=nocookie";
            request.Method = HttpMethod.Get;
            var response = await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            Assert.Contains("nocookie", response);
        }

        [Fact]
        public async Task Factory_WithoutSharedCookie_SendAsync_Success()
        {
            // Arrange: No shared cookie container
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var factory = serviceProvider.GetRequiredService<IJfYuRequestFactory>();
            var request = factory.CreateRequest();
            request.Url = $"{_url.Url}/get?test=nocookie";
            request.Method = HttpMethod.Get;
            var response = await request.SendAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);
            Assert.Contains("nocookie", response);
        }

        [Fact]
        public async Task Request_WithoutSharedCookie_CookiesShouldNotPersistAcrossScopes()
        {
            // Arrange: No shared cookie container - cookies should be isolated per request
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act: First request sets cookies
            using (var scope1 = serviceProvider.CreateScope())
            {
                var request1 = scope1.ServiceProvider.GetRequiredService<IJfYuRequest>();
                request1.Url = $"{_url.Url}/cookies/set?session=abc123";
                request1.Method = HttpMethod.Get;
                await request1.SendAsync();
                Assert.Equal(HttpStatusCode.OK, request1.StatusCode);
            }

            // Act: Second request should NOT have the cookies from first request
            using (var scope2 = serviceProvider.CreateScope())
            {
                var request2 = scope2.ServiceProvider.GetRequiredService<IJfYuRequest>();
                request2.Url = $"{_url.Url}/cookies";
                request2.Method = HttpMethod.Get;
                await request2.SendAsync();

                // Assert: ResponseCookies should be empty since no shared CookieContainer
                Assert.Empty(request2.ResponseCookies);
            }
        }

        [Fact]
        public void Resolve_WithSharedCookie_CookieContainerIsSingleton()
        {
            // Arrange: Enable shared cookie container
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = true;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert: CookieContainer should be registered and resolvable
            var container = serviceProvider.GetService<CookieContainer>();
            Assert.NotNull(container);

            // Verify it's a singleton
            var container2 = serviceProvider.GetService<CookieContainer>();
            Assert.Same(container, container2);
        }

        [Fact]
        public void Resolve_WithoutSharedCookie_CookieContainerNotRegistered()
        {
            // Arrange: Disable shared cookie container
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert: CookieContainer should NOT be registered
            var container = serviceProvider.GetService<CookieContainer>();
            Assert.Null(container);
        }

        [Fact]
        public async Task Scoped_WithoutSharedCookie_MultipleScopes_ShouldNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act: Create multiple scopes (simulating multiple HTTP requests in ASP.NET Core)
            for (int i = 0; i < 5; i++)
            {
                using var scope = serviceProvider.CreateScope();
                var request = scope.ServiceProvider.GetRequiredService<IJfYuRequest>();
                request.Url = $"{_url.Url}/get?scope={i}";
                request.Method = HttpMethod.Get;
                var response = await request.SendAsync();

                Assert.Equal(HttpStatusCode.OK, request.StatusCode);
                Assert.Contains(i.ToString(), response);
            }
        }

        [Fact]
        public async Task Factory_WithoutSharedCookie_MultipleScopes_ShouldNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJfYuHttpClient(options =>
            {
                options.UseSharedCookieContainer = false;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act: Create multiple scoped factory instances
            for (int i = 0; i < 5; i++)
            {
                using var scope = serviceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IJfYuRequestFactory>();
                var request = factory.CreateRequest();
                request.Url = $"{_url.Url}/get?scope={i}";
                request.Method = HttpMethod.Get;
                var response = await request.SendAsync();

                Assert.Equal(HttpStatusCode.OK, request.StatusCode);
                Assert.Contains(i.ToString(), response);
            }
        }

        #endregion
    }
}
#endif
