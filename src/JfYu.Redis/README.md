### Redis

A high-performance Redis client library for .NET with support for multiple serialization formats, distributed locking, pub/sub messaging, and comprehensive data structure operations.

## Features

- ✅ Support for all major Redis data structures (String, Hash, List, Set, Sorted Set)
- ✅ Pub/Sub messaging with pattern matching support
- ✅ Multiple serialization formats (Newtonsoft.Json, MessagePack)
- ✅ Distributed locking with unique token per instance
- ✅ Batch operations for improved performance
- ✅ Connection health monitoring and automatic recovery
- ✅ Key prefix support for multi-tenant scenarios
- ✅ High-performance logging with LoggerMessage source generators
- ✅ Custom value filtering for sensitive data
- ✅ Multi-targeting: .NET Standard 2.0, .NET 8.0, .NET 9.0, and .NET 10.0

## Installation

```bash
Install-Package JfYu.Redis
```

## Configuration

### appsettings.json

```json
{
  "Redis": {
    "EndPoints": [
      {
        "Host": "127.0.0.1",
        "Port": 6379
      }
    ],
    "Password": "YourPassword",
    "DbIndex": 0,
    "Timeout": 5000,
    "Ssl": false,
    "Prefix": "MyApp:",
    "EnableLogs": true
  }
}
```

## Usage

### Service Registration

#### Option 1: Using Configuration

```csharp
services.AddRedisService(options =>
{
    configuration.GetSection("Redis").Bind(options);
    options.UsingNewtonsoft(settings =>
    {
        settings.MaxDepth = 12;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
});
```

#### Option 2: Manual Configuration

```csharp
services.AddRedisService(options =>
{
    options.EndPoints.Add(new RedisEndPoint { Host = "localhost", Port = 6379 });
    options.Password = "password";
    options.SSL = false;
    options.DbIndex = 1;
    options.Prefix = "MyTest:";
    options.EnableLogs = true;

    // Optional: Custom value filter for logging (e.g., hide sensitive data)
    options.ValueFilter = value => value.Length > 100 ? value.Substring(0, 100) + "..." : value;

    // Choose serialization format
    options.UsingMessagePack(settings =>
    {
        // MessagePack configuration
    });
});
```

### Basic Operations

```csharp
public class MyService
{
    private readonly IRedisService _redisService;

    public MyService(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task BasicOperationsAsync()
    {
        // String operations
        await _redisService.AddAsync("user:1", new User { Name = "John", Age = 30 });
        var user = await _redisService.GetAsync<User>("user:1");

        // With expiration
        await _redisService.AddAsync("session:abc", "data", TimeSpan.FromMinutes(30));

        // Check existence
        bool exists = await _redisService.ExistsAsync("user:1");

        // Remove key
        await _redisService.RemoveAsync("user:1");

        // Increment/Decrement (Newtonsoft only)
        await _redisService.IncrementAsync("counter", 1);
        await _redisService.DecrementAsync("counter", 1);
    }
}
```

### Batch Operations

```csharp
// Batch get
var keys = new List<string> { "user:1", "user:2", "user:3" };
var users = await _redisService.GetBatchAsync<User>(keys);

// Batch set
var keyValues = new Dictionary<string, User>
{
    { "user:1", new User { Name = "Alice" } },
    { "user:2", new User { Name = "Bob" } }
};
await _redisService.AddBatchAsync(keyValues, TimeSpan.FromHours(1));
```

### Hash Operations

```csharp
// Hash set
await _redisService.HashSetAsync("user:1:profile", "name", "John");
await _redisService.HashSetAsync("user:1:profile", "email", "john@example.com");

// Hash get
var name = await _redisService.HashGetAsync<string>("user:1:profile", "name");

// Get all hash entries
var entries = await _redisService.HashGetAllAsync("user:1:profile");

// Check field existence
bool hasEmail = await _redisService.HashExistsAsync("user:1:profile", "email");

// Delete field
await _redisService.HashDeleteAsync("user:1:profile", "email");
```

### List Operations

```csharp
// Add to list (right/tail)
await _redisService.ListAddAsync("queue", "task1");
await _redisService.ListAddAsync("queue", "task2");

// Add to left (head)
await _redisService.ListAddToLeftAsync("queue", "urgent-task");

// Pop from right
var task = await _redisService.ListPopFromRightAsync<string>("queue");

// Pop from left
var urgentTask = await _redisService.ListPopFromLeftAsync<string>("queue");

// Get list length
var count = await _redisService.ListLengthAsync("queue");

// Get range
var tasks = await _redisService.ListGetRangeAsync("queue", 0, 10);

// Remove items
await _redisService.ListRemoveAsync("queue", "task1", 0); // Remove all occurrences
```

### Set Operations

```csharp
// Add to set
await _redisService.SetAddAsync("tags", "redis");
await _redisService.SetAddAsync("tags", "cache");

// Add multiple
await _redisService.SetAddAllAsync("tags", new List<string> { "nosql", "database" });

// Check membership
bool isMember = await _redisService.SetContainsAsync("tags", "redis");

// Get all members
var allTags = await _redisService.SetMembersAsync("tags");

// Get random member
var randomTag = await _redisService.SetRandomMemberAsync("tags");

// Remove
await _redisService.SetRemoveAsync("tags", "redis");

// Get set length
var tagCount = await _redisService.SetLengthAsync("tags");
```

### Sorted Set Operations

```csharp
// Add with score
await _redisService.SortedSetAddAsync("leaderboard", "player1", 100);
await _redisService.SortedSetAddAsync("leaderboard", "player2", 200);

// Add multiple
var scores = new Dictionary<string, double>
{
    { "player3", 150 },
    { "player4", 250 }
};
await _redisService.SortedSetAddAllAsync("leaderboard", scores);

// Increment score
await _redisService.SortedSetIncrementScoreAsync("leaderboard", "player1", 50);

// Get rank (0-based)
var rank = await _redisService.SortedSetRankAsync("leaderboard", "player1");

// Get range by rank
var topPlayers = await _redisService.SortedSetRangeByRankAsync("leaderboard", 0, 9);

// Get range by score
var midRange = await _redisService.SortedSetRangeByScoreAsync("leaderboard", 100, 200);

// Count by score range
var count = await _redisService.SortedSetCountAsync("leaderboard", 100, 200);

// Remove
await _redisService.SortedSetRemoveAsync("leaderboard", new List<string> { "player1" });
```

### Distributed Locking

```csharp
// Acquire lock
bool locked = await _redisService.LockTakeAsync("resource:lock", TimeSpan.FromSeconds(30));
if (locked)
{
    try
    {
        // Critical section - only one instance can execute this
        await DoSomethingCritical();
    }
    finally
    {
        // Always release the lock
        await _redisService.LockReleaseAsync("resource:lock");
    }
}
```

### Pub/Sub Messaging

```csharp
// Subscribe to a channel
await _redisService.SubscribeAsync<string>("notifications", (channel, message) =>
{
    Console.WriteLine($"Received on {channel}: {message}");
});

// Subscribe with pattern matching
await _redisService.SubscribePatternAsync<string>("events:*", (channel, message) =>
{
    Console.WriteLine($"Pattern matched {channel}: {message}");
});

// Publish a message
var subscriberCount = await _redisService.PublishAsync("notifications", "Hello World");
Console.WriteLine($"Message delivered to {subscriberCount} subscribers");

// Unsubscribe from specific channel
await _redisService.UnsubscribeAsync("notifications");

// Unsubscribe from pattern
await _redisService.UnsubscribePatternAsync("events:*");

// Unsubscribe from all channels
await _redisService.UnsubscribeAllAsync();
```

### Key Management

```csharp
// Set expiration
await _redisService.ExpireAsync("temp:data", TimeSpan.FromMinutes(5));

// Get time to live
var ttl = await _redisService.GetTimeToLiveAsync("temp:data");

// Remove expiration (persist)
await _redisService.PersistAsync("temp:data");

// Health check
var pingTime = await _redisService.PingAsync();
if (pingTime > TimeSpan.Zero)
{
    Console.WriteLine($"Redis is responding in {pingTime.TotalMilliseconds}ms");
}
```

## Serialization Options

### Newtonsoft.Json (Default)

```csharp
options.UsingNewtonsoft(settings =>
{
    settings.MaxDepth = 12;
    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    settings.NullValueHandling = NullValueHandling.Ignore;
});
```

### MessagePack

```csharp
options.UsingMsgPack(settings =>
{
    // MessagePack configuration
    settings = settings.WithCompression(MessagePackCompression.Lz4Block);
});
```

## Logging Configuration

### Enable Logging

```csharp
services.AddRedisService(options =>
{
    options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
    options.EnableLogs = true;  // Enable logging

    // Custom value filter to sanitize logged values
    options.ValueFilter = value =>
    {
        // Remove sensitive information
        if (value.Contains("password", StringComparison.OrdinalIgnoreCase))
            return "***FILTERED***";

        // Truncate long values
        return value.Length > 200 ? value.Substring(0, 200) + "..." : value;
    };
});
```

### Logging Output

When enabled, Redis operations are logged at `Trace` level:

```
[Trace] Redis GetAsync - Key: user:123, Value: {"Name":"John","Age":30}
[Trace] Redis SetAddAsync - Key: tags, Value:
[Trace] Redis PublishAsync - Key: notifications, Value: Hello World
```

## Best Practices

1. **Use Key Prefixes**: Organize keys with prefixes for different tenants or modules
2. **Set Expiration**: Always set TTL for temporary data to prevent memory leaks
3. **Batch Operations**: Use batch methods when working with multiple keys
4. **Connection Pooling**: The library uses singleton ConnectionMultiplexer for optimal performance
5. **Error Handling**: Always wrap Redis operations in try-catch blocks
6. **Distributed Locks**: Always release locks in finally blocks to prevent deadlocks
7. **Pub/Sub**: Use pattern matching for flexible message routing across multiple channels
8. **Logging**: Use custom value filters to sanitize sensitive data before logging

## Connection Events

The library automatically logs connection events:

- Connection failures
- Connection restoration
- Error messages

Monitor these logs to ensure Redis health.

## License

See LICENSE file in the project root.
