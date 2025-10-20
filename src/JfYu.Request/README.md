

Injection
``` 
//Register default HTTP request service
services.AddJfYuHttpRequest();
//Register with logging disabled
services.AddJfYuHttpRequest(q=>q.LoggingFields= JfYu.Request.Enum.JfYuLoggingFields.None); 
//Register with logging enabled and identity filters
services.AddJfYuHttpRequest(q=>{ q.LoggingFields = JfYu.Request.Enum.JfYuLoggingFields.All;q.RequestFilter = x => x; q.ResponseFilter = x => x; });
//Register custom HttpClient with cookie handling disabled
services.AddJfYuHttpClient(() => { return new HttpClientHandler() { UseCookies = false }; }, q => { q.LoggingFields = JfYu.Request.Enum.JfYuLoggingFields.All; q.RequestFilter = x => x; q.ResponseFilter = x => x; });

```

Usage

```
//master
var req = provider.GetRequiredService<IJfYuRequest>(); 

//Get
_jfYuRequest.Url = $"https://xxx.com?id={t.Tid}&page={page}";
_jfYuRequest.RequestEncoding = Encoding.GetEncoding("GB18030");
_jfYuRequest.RequestCookies.Add(new Cookie() { Name = "c1", Value = "v1", Domain = ".xxx.com", Path = "/" }); 
_jfYuRequest.Timeout = 60;
var html = await _jfYuRequest.SendAsync(); 

//Post 
_jfYuRequest.Url = "https://xxx.com";
_jfYuRequest.Method = HttpMethod.Post;
_jfYuRequest.RequestEncoding = Encoding.GetEncoding("GB18030");
_jfYuRequest.Timeout = 30;
_jfYuRequest.ContentType = RequestContentType.FormData; //RequestContentType.FormUrlEncoded
_jfYuRequest.Files = new Dictionary<string, string> { { "file", "code.png" } };
_jfYuRequest.RequestData = $"name=xiaoyu&password=123456&rid={randomString}";

_jfYuRequest.ContentType = RequestContentType.Json; 
_jfYuRequest.RequestData = $"{\"username\":\"testUser\"}";

_jfYuRequest.ContentType = RequestContentType.Xml; 
_jfYuRequest.RequestData = $"<user><username>testUser</username></user>";

_jfYuRequest.RequestCookies.Add(new Cookie() { Name ="c1", Value = "v1", Domain = ".xxx.com", Path = "/" });
var html = await _jfYuRequest.SendAsync();
 
//download file
_jfYuRequest.Url = "https://www.xxx.com/1.jpg"
var jpg = await jfYu.DownloadFileAsync("path/2.jpg"); 

//donwload file with stream
var stream = await jfYu.DownloadFileAsync();
using FileStream fs = File.Create("path/3.jpg");
await fs.WriteAsync(stream?.ToArray()); 
```
