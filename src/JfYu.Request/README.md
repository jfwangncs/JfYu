# JfYu.Request

HTTP request abstraction with `HttpClient`, rich headers, logging filters, cookies, proxy, SSL, file download, and multipart upload. The README reflects unit-test proven usage.

## Dependency Injection

```csharp
// Default registration (uses HttpClientFactory and CookieContainer)
services.AddJfYuHttpRequest();

// With filters and logging fields
services.AddJfYuHttpClient(null, o =>
{
 o.LoggingFields = JfYu.Request.Enum.JfYuLoggingFields.All;
 o.RequestFilter = z => z; // sanitize before logging
 o.ResponseFilter = z => z; // sanitize before logging
});

// Provide a custom HttpClientHandler
services.AddJfYuHttpClient(() => new HttpClientHandler
{
 UseCookies = true,
 // ServerCertificateCustomValidationCallback = (m, c, ch, e) => true // e.g., disable validation for test
});
```

## Basic Send

```csharp
var client = provider.GetRequiredService<IJfYuRequest>();

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
using var handler = new HttpClientHandler
{
 UseProxy = true,
 Proxy = new WebProxy("http://example.cn1")
};
services.AddJfYuHttpClient(() => handler);
```

## SSL / Certificates

```csharp
// Server validation error (set CertificateValidation = true to enforce server validation)
client.Url = errorSslUrl;
client.CertificateValidation = true; // throws HttpRequestException

// Accept all server certs (test only)
services.AddJfYuHttpClient(() => new HttpClientHandler
{
 ServerCertificateCustomValidationCallback = (a,b,c,d) => true
});

// Client certificate
var cert = new X509Certificate2("Static/badssl.com-client.p12", "badssl.com");
using var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);
services.AddJfYuHttpClient(() => handler);
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
// Configure logging fields
services.AddJfYuHttpClient(null, o => o.LoggingFields = JfYuLoggingFields.All);

// Filters are applied around logging
services.AddJfYuHttpClient(null, o =>
{
 o.RequestFilter = z => z; // can throw; logged as error
 o.ResponseFilter = z => z;
});
