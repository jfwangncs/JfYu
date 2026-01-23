# JfYu.WeChat

A .NET library for WeChat Mini Program (小程序) API integration, providing simplified access to login, phone number retrieval, and access token management.

## Features

- **User Authentication**: Login with WeChat authorization code
- **Phone Number Retrieval**: Get user's phone number with authorization
- **Access Token Management**: Automatic token retrieval for server API calls
- **Multi-Target Support**: Compatible with `netstandard2.0` and `net8.0`
- **Dependency Injection**: Easy service registration with ASP.NET Core

## Installation

```bash
dotnet add package JfYu.WeChat
```

## Quick Start

### 1. Register Services

In `Startup.cs` or `Program.cs`:

```csharp
using JfYu.WeChat;

// Configure Mini Program services with AppId and Secret
services.AddMiniProgram(options =>
{
    options.AppId = "your_mini_program_appid";
    options.Secret = "your_mini_program_secret";
    
    // Optional: Enable HTTP request/response logging for debugging
    options.EnableHttpLogging = true;  // Default: false
});
```

**Configuration Options:**

- `AppId`: WeChat Mini Program Application ID (required)
- `Secret`: WeChat Mini Program Secret Key (required)
- `EnableHttpLogging`: Enable HTTP request/response logging (optional, default: `false`)
  - When `true`: Logs all HTTP requests and responses with full details
  - When `false`: No HTTP logging (recommended for production)

### 2. Inject and Use

```csharp
using JfYu.WeChat;

public class UserController : ControllerBase
{
    private readonly IMiniProgram _miniProgram;

    public UserController(IMiniProgram miniProgram)
    {
        _miniProgram = miniProgram;
    }

    // User login with WeChat authorization code
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] string code)
    {
        var loginResult = await _miniProgram.LoginAsync(code);

        if (loginResult?.ErrorCode == 0)
        {
            // Success: loginResult.OpenId, loginResult.SessionKey, loginResult.UnionId
            return Ok(new { openId = loginResult.OpenId });
        }

        return BadRequest(loginResult?.ErrorMessage);
    }

    // Get user's phone number
    [HttpPost("phone")]
    public async Task<IActionResult> GetPhone([FromBody] string code)
    {
        var phoneResult = await _miniProgram.GetPhoneAsync(code);

        if (phoneResult?.ErrorCode == 0 && phoneResult.PhoneInfo != null)
        {
            return Ok(new
            {
                phone = phoneResult.PhoneInfo.PhoneNumber,
                countryCode = phoneResult.PhoneInfo.CountryCode
            });
        }

        return BadRequest(phoneResult?.ErrorMessage);
    }
   
}
```

## API Reference

### IMiniProgram Interface

#### LoginAsync(string code)

Authenticates user login using authorization code from `wx.login()`.

**Parameters:**

- `code`: Authorization code from Mini Program

**Returns:**

- `WechatLoginResponse` with OpenId, UnionId, and SessionKey

**Example:**

```csharp
var result = await _miniProgram.LoginAsync(code);
Console.WriteLine($"OpenId: {result.OpenId}");
```

#### GetPhoneAsync(string code)

Retrieves user's phone number using authorization code from button callback.

**Parameters:**

- `code`: Phone authorization code from `getPhoneNumber` button

**Returns:**

- `GetPhoneResponse` with phone number information

**Example:**

```csharp
var result = await _miniProgram.GetPhoneAsync(code);
Console.WriteLine($"Phone: {result.PhoneInfo?.PhoneNumber}");
```

#### GetAccessTokenAsync()

Obtains access token for server-to-server API calls (valid for 2 hours).

**Returns:**

- `AccessTokenResponse` with token and expiration time

**Example:**

```csharp
var result = await _miniProgram.GetAccessTokenAsync();
Console.WriteLine($"Token: {result.AccessToken}, Expires in: {result.Expires}s");
```

## Response Models

### WechatLoginResponse

```csharp
public class WechatLoginResponse
{
    public string? OpenId { get; set; }           // User's unique ID in this Mini Program
    public string? UnionId { get; set; }          // User's unique ID across WeChat apps
    public string? SessionKey { get; set; }       // Session key for decryption
    public int ErrorCode { get; set; }            // 0 = success
    public string? ErrorMessage { get; set; }     // Error description
}
```

### GetPhoneResponse

```csharp
public class GetPhoneResponse
{
    public PhoneInfo? PhoneInfo { get; set; }     // Phone details
    public int ErrorCode { get; set; }            // 0 = success
    public string? ErrorMessage { get; set; }     // Error description
}

public class PhoneInfo
{
    public string? PhoneNumber { get; set; }      // Full phone with country code
    public string? PurePhoneNumber { get; set; }  // Phone without country code
    public string? CountryCode { get; set; }      // Country code (e.g., "86")
    public WaterMark? Watermark { get; set; }     // Security watermark
}
```

### AccessTokenResponse

```csharp
public class AccessTokenResponse
{
    public string AccessToken { get; set; }       // Access token string
    public int Expires { get; set; }              // Expiration time in seconds (7200)
}
``` 

## Configuration Options

### MiniProgramOptions

```csharp
public class MiniProgramOptions
{
    public string AppId { get; set; }      // Mini Program AppId (required)
    public string Secret { get; set; }     // Mini Program AppSecret (required)
}
```

Configure in `appsettings.json`:

```json
{
  "MiniProgramOptions": {
    "AppId": "wx1234567890abcdef",
    "Secret": "your_app_secret_here"
  }
}
```

Then bind in `Startup.cs`:

```csharp
services.AddMiniProgram(options =>
{
    Configuration.GetSection("MiniProgramOptions").Bind(options);
});
```

## Mini Program Integration

### Client-Side (Mini Program)

**Login Flow:**

```javascript
// In Mini Program code
wx.login({
  success: (res) => {
    if (res.code) {
      // Send code to your backend
      wx.request({
        url: "https://yourapi.com/api/user/login",
        method: "POST",
        data: { code: res.code },
        success: (response) => {
          console.log("OpenId:", response.data.openId);
        },
      });
    }
  },
});
```

**Get Phone Number:**

```xml
<!-- In Mini Program WXML -->
<button open-type="getPhoneNumber" bindgetphonenumber="getPhoneNumber">
  Get Phone Number
</button>
```

```javascript
// In Mini Program JS
Page({
  getPhoneNumber: function (e) {
    if (e.detail.code) {
      // Send code to your backend
      wx.request({
        url: "https://yourapi.com/api/user/phone",
        method: "POST",
        data: { code: e.detail.code },
        success: (response) => {
          console.log("Phone:", response.data.phone);
        },
      });
    }
  },
});
```

## Dependencies

- **JfYu.Request**: HTTP request abstraction layer
- **Newtonsoft.Json**: JSON serialization
- **Microsoft.Extensions.Options**: Configuration binding
- **Microsoft.Extensions.DependencyInjection**: Service registration

## WeChat API Endpoints

The library uses the following WeChat API endpoints:

| Endpoint                          | Purpose          | Documentation                                                                                                                  |
| --------------------------------- | ---------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `sns/jscode2session`              | User login       | [Login API](https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html)                |
| `cgi-bin/token`                   | Get access token | [Token API](https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/access-token/auth.getAccessToken.html)       |
| `wxa/business/getuserphonenumber` | Get phone number | [Phone API](https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/phonenumber/phonenumber.getPhoneNumber.html) |

## Security Considerations

1. **Never expose AppSecret**: Store AppSecret securely in environment variables or secure configuration
2. **Validate on server side**: Always verify WeChat responses on your backend
3. **Use HTTPS**: Ensure all API calls use secure connections
4. **Token caching**: Cache access tokens to avoid exceeding rate limits (2000 calls/day)
5. **Session management**: Properly manage SessionKey and avoid storing it in insecure locations

## Best Practices

### Access Token Caching

Access tokens are valid for 2 hours. Implement caching to avoid unnecessary API calls:

```csharp
public class CachedMiniProgram
{
    private readonly IMiniProgram _miniProgram;
    private readonly IMemoryCache _cache;

    public async Task<string> GetCachedAccessTokenAsync()
    {
        return await _cache.GetOrCreateAsync("wechat_access_token", async entry =>
        {
            var result = await _miniProgram.GetAccessTokenAsync();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.Expires - 60);
            return result.AccessToken;
        });
    }
}
```

### Error Retry Logic

Implement retry logic for transient failures:

```csharp
public async Task<WechatLoginResponse?> LoginWithRetryAsync(string code, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var result = await _miniProgram.LoginAsync(code);
            if (result?.ErrorCode == 0 || result?.ErrorCode != -1)
            {
                return result; // Success or permanent error
            }
        }
        catch (Exception ex)
        {
            if (i == maxRetries - 1) throw;
        }

        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
    }

    return null;
}
```

## Troubleshooting

### Common Issues

**1. "Invalid code" error (40029)**

- Code can only be used once
- Code expires after 5 minutes
- Ensure code is sent to backend immediately after receiving

**2. "Rate limit exceeded" (45011)**

- Implement access token caching
- Avoid calling GetAccessTokenAsync for every request
- WeChat allows 2000 token requests per day

**3. Phone number returns null**

- Ensure Mini Program has phone number permission enabled
- User must explicitly authorize via button click
- Code from `getPhoneNumber` is different from login code

## License

This project is part of the JfYu toolkit library. See the root LICENSE file for details.

## Contributing

Contributions are welcome! Please ensure all changes include appropriate tests and documentation.

## Related Projects

- **JfYu.Request**: HTTP request abstraction layer
- **JfYu.Data**: EF Core data access with read-write separation
- **JfYu.RabbitMQ**: RabbitMQ message queue client

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/jfwang/JfYu).
