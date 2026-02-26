# JfYu.Request

HTTP request abstraction with `HttpClient`, rich headers, logging filters, cookies, proxy, SSL, file download, and multipart upload. Supports multiple named HttpClients for multi-tenant or multi-configuration scenarios. The README reflects unit-test proven usage.

## Features

- **HttpClientFactory Integration** - Proper connection pooling and lifecycle management
- **Multiple Named Clients** - Configure different HttpClients with distinct settings
- **Factory Pattern** - `IJfYuRequestFactory` for runtime client selection
- **Rich Configuration** - Custom headers, timeouts, handlers, base addresses
- **Logging & Filtering** - Configurable logging with request/response filters
- **Cookie Management** - Shared or isolated cookie containers
- **Proxy Support** - Configure per-client proxies
- **SSL/Certificates** - Client certificates and custom validation
- **File Operations** - Download with progress tracking, multipart upload
- **Backward Compatible** - Existing `IJfYuRequest` injection still works

## Quick Start

### Single Client (Simple)

```csharp
// Default registration (uses HttpClientFactory and CookieContainer)
services.AddJfYuHttpClient();

// Use via dependency injection
var client = provider.GetRequiredService<IJfYuRequest>();
client.Url = "https://api.example.com/users";
client.Method = HttpMethod.Get;
var response = await client.SendAsync();
```

### Multiple Clients (Advanced)

```csharp
// Register multiple named clients with different configurations
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "ApiClient";
    options.ConfigureClient = client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
        client.Timeout = TimeSpan.FromMinutes(5);
        client.DefaultRequestHeaders.Add("X-API-Key", "your-key");
    };
});

services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "PaymentClient";
    options.HttpClientHandler = () => new HttpClientHandler
    {
        Proxy = new WebProxy("http://proxy.internal:8080")
    };
    options.ConfigureClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    };
});

// Use via factory pattern
var factory = provider.GetRequiredService<IJfYuRequestFactory>();
var apiRequest = factory.CreateRequest("ApiClient");
var paymentRequest = factory.CreateRequest("PaymentClient");
```

## Configuration Options

### JfYuHttpClientOptions

```csharp
services.AddJfYuHttpClient(options =>
{
    // Client name (required for multi-client scenarios)
    options.HttpClientName = "MyClient"; // Default: "JfYuHttpClient"

    // Custom HttpClientHandler factory
    options.HttpClientHandler = () => new HttpClientHandler
    {
        UseProxy = true,
        Proxy = new WebProxy("http://proxy.example.com:8080"),
        ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
    };

    // Configure HttpClient directly
    options.ConfigureClient = client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
        client.Timeout = TimeSpan.FromMinutes(10);
        client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    };

    // Cookie container sharing (default: false for thread safety)
    options.UseSharedCookieContainer = false; // Each request gets isolated cookies
    // WARNING: Setting to true enables a shared CookieContainer that is NOT thread-safe.
    // Concurrent requests may lead to race conditions. Use with caution in multi-threaded scenarios.

}, filter =>
{
    // Logging configuration
    filter.LoggingFields = JfYuLoggingFields.All;
    filter.RequestFilter = req => req.Replace("password", "***");
    filter.ResponseFilter = resp => resp.Replace("token", "***");
});
```

## Usage Patterns

### Pattern 1: Direct Injection (Single Client)

```csharp
public class UserService
{
    private readonly IJfYuRequest _request;

    public UserService(IJfYuRequest request)
    {
        _request = request;
    }

    public async Task<string> GetUserAsync(int userId)
    {
        _request.Url = $"https://api.example.com/users/{userId}";
        _request.Method = HttpMethod.Get;
        return await _request.SendAsync();
    }
}
```

### Pattern 2: Factory Pattern (Multiple Clients)

```csharp
public class MultiTenantService
{
    private readonly IJfYuRequestFactory _factory;

    public MultiTenantService(IJfYuRequestFactory factory)
    {
        _factory = factory;
    }

    public async Task<string> GetDataAsync(string tenantId)
    {
        // Select client based on tenant
        var clientName = tenantId == "premium" ? "PremiumClient" : "StandardClient";
        var request = _factory.CreateRequest(clientName);

        request.Url = $"https://api.example.com/data";
        request.Method = HttpMethod.Get;
        return await request.SendAsync();
    }
}
```

## Basic Send

```csharp
// Option 1: Direct injection
var client = provider.GetRequiredService<IJfYuRequest>();

// Option 2: Factory pattern
var factory = provider.GetRequiredService<IJfYuRequestFactory>();
var client = factory.CreateRequest(); // Default client
// var client = factory.CreateRequest("ApiClient"); // Named client

// GET
client.Url = $"{baseUrl}/get?username=testUser&age=30";
client.Method = HttpMethod.Get;
var text = await client.SendAsync();
Assert.Equal(HttpStatusCode.OK, client.StatusCode);

// POST (empty JSON body)
client.Url = $"{baseUrl}/post";
client.Method = HttpMethod.Post;
client.ContentType = RequestContentType.Json;
var resp = await client.SendAsync();
```

## Content types

```csharp
// text/plain
client.Url = $"{baseUrl}/post";
client.Method = HttpMethod.Post;
client.ContentType = RequestContentType.Plain;
client.RequestEncoding = Encoding.UTF8;
client.RequestData = "username=testUser&age=30";
await client.SendAsync();

// application/xml
client.ContentType = RequestContentType.Xml;
client.RequestData = "<user><username>testUser</username><age>30</age></user>";
await client.SendAsync();

// application/x-www-form-urlencoded
client.ContentType = RequestContentType.FormUrlEncoded;
client.RequestData = "username=testUser&age=30";
await client.SendAsync();

// application/json
client.ContentType = RequestContentType.Json;
client.RequestData = "{\"username\":\"testUser\",\"age\":30}";
await client.SendAsync();

// multipart/form-data (files + fields)
client.ContentType = RequestContentType.FormData;
client.RequestData = "username=testUser&age=30";
client.Files = new Dictionary<string,string>
{
 {"test.txt", "path/to/file1.txt"},
 {"test1.txt", "path/to/file2.txt"}
};
await client.SendAsync();
```

## Methods

```csharp
// PUT
client.Url = $"{baseUrl}/put";
client.Method = HttpMethod.Put;
client.RequestData = "{\"username\":\"testUser\",\"age\":30}";
await client.SendAsync();

// DELETE
client.Url = $"{baseUrl}/delete?username=testUser&age=30";
client.Method = HttpMethod.Delete;
await client.SendAsync();

// PATCH
client.Url = $"{baseUrl}/patch?username=testUser&age=30";
client.Method = HttpMethod.Patch;
await client.SendAsync();

// OPTIONS
client.Url = $"{baseUrl}/anything";
client.Method = HttpMethod.Options;
await client.SendAsync();
```

## Headers and auth

```csharp
// Strongly-typed headers
client.RequestHeader = new RequestHeaders
{
 UserAgent = "JfYuHttpClient/1.0",
 Host = "httpbin.org",
 Referer = "http://httpbin.org",
 Accept = "text/html",
 AcceptLanguage = "zh-en",
 CacheControl = "cache",
 Connection = "keep-alive",
 Pragma = "Pragma",
 AcceptEncoding = "gzip" // or deflate, br
};

// Custom headers and Authorization
client.RequestCustomHeaders.Add("X-Custom-Header", "test-value");
client.AuthorizationScheme = "Bearer";
client.Authorization = "test-token"; // sends: Authorization: Bearer test-token
```

## Cookies

> **⚠️ WARNING: Thread Safety**
> When `UseSharedCookieContainer = true`, the shared CookieContainer is NOT thread-safe.
> Concurrent requests in multi-threaded environments may lead to race conditions and unpredictable behavior.
> Use `UseSharedCookieContainer = false` (default) for thread-safe, isolated cookie management per request.

```csharp
// No cookies
client.RequestCookies = null!;

// Set cookies
var jar = new CookieContainer();
jar.Add(new Cookie("c1", "v1", "/", new Uri(baseUrl).Host));
jar.Add(new Cookie("c2", "v2", "/", new Uri(baseUrl).Host));
client.RequestCookies = jar;
var html = await client.SendAsync();
var set = client.ResponseCookies; // read response cookies
```

## Custom initialization

```csharp
client.CustomInit = o =>
{
 var http = (HttpClient)o;
 http.DefaultRequestHeaders.Add("X-Custom-Init", "test-value");
};
```

## Proxy

```csharp
// Register client with proxy
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "ProxyClient";
    options.HttpClientHandler = () => new HttpClientHandler
    {
        UseProxy = true,
        Proxy = new WebProxy("http://proxy.example.com:8080")
    };
});

// Use proxied client
var factory = provider.GetRequiredService<IJfYuRequestFactory>();
var client = factory.CreateRequest("ProxyClient");
```

## SSL / Certificates

```csharp
// Accept all server certs (test/development only)
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "InsecureClient";
    options.HttpClientHandler = () => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (a, b, c, d) => true
    };
});

// Client certificate authentication
var cert = new X509Certificate2("path/to/client-cert.p12", "password");
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "SecureClient";
    options.HttpClientHandler = () =>
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert);
        return handler;
    };
});

// Server validation error (set CertificateValidation = true to enforce)
var client = factory.CreateRequest("SecureClient");
client.Url = "https://expired.badssl.com";
client.CertificateValidation = true; // throws HttpRequestException on cert error
client.Certificate = cert; // optional
```

## Timeouts and errors

```csharp
client.Url = $"{baseUrl}/delay/50";
client.Timeout =1; // seconds; will throw
await Assert.ThrowsAsync<Exception>(() => client.SendAsync());
```

## Downloading files

```csharp
// To stream
client.Url = $"{baseUrl}/bytes/1024";
using var stream = await client.DownloadFileAsync((p, s, r) =>
{
 // p: percentage, s: KB/s, r: seconds remaining
});

// To path (auto creates directory)
var ok = await client.DownloadFileAsync("download/file.bin", (p, s, r) => { /* progress */ });

// Empty path throws
await Assert.ThrowsAsync<ArgumentNullException>(() => client.DownloadFileAsync(""));

// Non-success status returns null/false
client.Url = $"{baseUrl}/status/500";
var mem = await client.DownloadFileAsync(); // null
var flag = await client.DownloadFileAsync("file.bin"); // false
```

## Logging

```csharp
// Configure logging fields and filters
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "LoggedClient";
}, filter =>
{
    filter.LoggingFields = JfYuLoggingFields.All;
    filter.RequestFilter = req => req.Replace("password=", "password=***");
    filter.ResponseFilter = resp => resp.Replace("\"token\":", "\"token\":\"***\"");
});

// Filters are applied before logging
// If filter throws, it's logged as error
```

## Multi-Tenant Scenarios

```csharp
// Register different clients for different tenants
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "TenantA";
    options.ConfigureClient = client =>
    {
        client.BaseAddress = new Uri("https://tenant-a.api.example.com");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-a");
    };
});

services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "TenantB";
    options.ConfigureClient = client =>
    {
        client.BaseAddress = new Uri("https://tenant-b.api.example.com");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-b");
    };
});

// Select client at runtime based on tenant context
public class TenantService
{
    private readonly IJfYuRequestFactory _factory;

    public TenantService(IJfYuRequestFactory factory)
    {
        _factory = factory;
    }

    public async Task<string> GetDataAsync(string tenantId)
    {
        var request = _factory.CreateRequest(tenantId);
        request.Url = "/api/data";
        request.Method = HttpMethod.Get;
        return await request.SendAsync();
    }
}
```

## Migration from Old API

If you're using the old API with `Func<HttpClientHandler>`:

```csharp
// OLD (deprecated)
services.AddJfYuHttpClient(() => new HttpClientHandler { ... });

// NEW
services.AddJfYuHttpClient(options =>
{
    options.HttpClientHandler = () => new HttpClientHandler { ... };
});
```

See [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) for complete migration instructions.

## Advanced Examples

See [MULTIPLE_CLIENTS_GUIDE.md](MULTIPLE_CLIENTS_GUIDE.md) for comprehensive examples including:

- Multi-tenant configurations
- Different proxies per client
- SSL certificate management
- Timeout strategies
- Cookie isolation

## API Reference

### IJfYuRequest (Single Client)

Direct injection interface for simple scenarios.

```csharp
public interface IJfYuRequest
{
    string Url { get; set; }
    HttpMethod Method { get; set; }
    HttpStatusCode StatusCode { get; }
    Task<string> SendAsync(CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFileAsync(Action<int, double, long>? progress = null, CancellationToken cancellationToken = default);
    // ... more properties and methods
}
```

### IJfYuRequestFactory (Multiple Clients)

Factory interface for multi-client scenarios.

```csharp
public interface IJfYuRequestFactory
{
    IJfYuRequest CreateRequest(); // Creates request with default client
    IJfYuRequest CreateRequest(string httpClientName); // Creates request with named client
}
```

## Testing

```csharp
// In tests, register clients normally
var services = new ServiceCollection();
services.AddJfYuHttpClient(options =>
{
    options.HttpClientName = "TestClient";
});

var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IJfYuRequestFactory>();
var request = factory.CreateRequest("TestClient");

// Or use direct injection for simple tests
var request = provider.GetRequiredService<IJfYuRequest>();
```

## Best Practices

1. **Use Factory for Multiple Clients**: When you need different configurations, use `IJfYuRequestFactory`
2. **Use Direct Injection for Single Client**: For simple scenarios, `IJfYuRequest` is sufficient
3. **Name Your Clients**: Use descriptive names like "ApiClient", "PaymentClient", "ReportClient"
4. **Configure Timeouts**: Set appropriate timeouts per client based on expected response times
5. **Isolate Cookies When Needed**: Set `UseSharedCookieContainer = false` for independent sessions
6. **Filter Sensitive Data**: Use `RequestFilter` and `ResponseFilter` to sanitize logs
7. **Reuse Clients**: HttpClientFactory manages pooling, don't create new registrations per request

## Performance

- Uses `HttpClientFactory` for proper connection pooling
- Handlers are reused across requests
- Supports HTTP/2 and HTTP/3
- Proper DNS refresh and connection lifecycle
- No socket exhaustion issues
