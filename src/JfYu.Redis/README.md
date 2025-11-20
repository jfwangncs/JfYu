
### redis

Install

```
Install-Package JfYu.Redis
```


Configure

```
"Redis": {
    "EndPoints": [
      {
        "Host": "127.0.0.1",
        "Port": 6379
      }
    ],
    "Password": "Password",
    "DbIndex": 0,
    "Timeout": 5000,
    "Ssl": false
  }
```


```
//injection
 services.AddRedisService(options =>
    {
        options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
        options.SSL = false;
        options.DbIndex = 1;
        options.Prefix = "Mytest:";
        options.EnableLogs = true;
        options.UsingNewtonsoft(options =>
        {
            options.MaxDepth = 12;
        });
    });

services.AddRedisService(options =>
    {
        configuration.GetSection("Redis").Bind(options);
        options.UsingNewtonsoft(options =>
        {
            options.MaxDepth = 12;
        });
    });
await _redisService.AddAsync("key", "value");
await _redisService.GetAsync("key");
await _redisService.RemoveAsync("key");
```


