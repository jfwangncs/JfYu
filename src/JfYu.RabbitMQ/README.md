## JfYu.RabbitMQ

Lightweight RabbitMQ client wrapper with async publishing/consuming, retry with dead-letter support, and DI integration.

Features
- Async publish with publisher confirms and throttling
- Batch publishing
- Async consumer with prefetch control
- Automatic retry via message headers; overflow to DLQ when configured
- Queue/Exchange declare and bind helpers
- DI via Microsoft.Extensions.DependencyInjection

Install
```
Install-Package JfYu.RabbitMQ
```

Configuration (appsettings.json)
```
{
  "RabbitMQ": {
    "HostName": "127.0.0.1",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "MessageOption": {
      "MaxRetryCount": 3,
      "RetryDelayMilliseconds": 5000,
      "MaxOutstandingConfirms": 1000,
      "BatchSize": 20
    }
  }
}
```

Dependency Injection
```
// using JfYu.RabbitMQ;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;

services.AddRabbitMQ((factory, options) =>
{
    configuration.GetSection("RabbitMQ").Bind(factory);
    configuration.GetSection("RabbitMQ:MessageOption").Bind(options);
});
```

Declare and Bind
```
// using RabbitMQ.Client;
var mq = serviceProvider.GetRequiredService<IRabbitMQService>();

// Optional: configure DLQ for the queue via arguments
var args = new Dictionary<string, object?>
{
    ["x-dead-letter-exchange"] = "dlx",
    ["x-dead-letter-routing-key"] = "orders.dlq"
};

await mq.QueueDeclareAsync(
    queueName: "q.orders",
    exchangeName: "ex.orders",
    exchangeType: ExchangeType.Direct,
    routingKey: "orders",
    headers: args);

// Bind a DLX queue
await mq.QueueDeclareAsync("q.orders.dlq");
```

Publish
```
// Single
await mq.SendAsync("ex.orders", new { Id = 1, Name = "test" }, routingKey: "orders");

// Batch
await mq.SendBatchAsync("ex.orders", new[]
{
    new { Id = 2 },
    new { Id = 3 }
}, routingKey: "orders");
```

Consume (async)
```
using var cts = new CancellationTokenSource();
var channel = await mq.ReceiveAsync<string>(
    queueName: "q.orders",
    func: async msg =>
    {
        // return true to ACK; false to trigger retry/DLQ
        Console.WriteLine($"received: {msg}");
        return true;
    },
    prefetchCount: 10,
    cancellationToken: cts.Token);

// later: cts.Cancel(); // gracefully cancels consumer and disposes channel
```

Retry and Dead Letter
- On failure (handler returns false or throws), the library increments header `x-retry-count` and republishes to the original exchange/routing key until `MaxRetryCount` is reached.
- When retries exceed `MaxRetryCount`, the message is rejected without requeue. Configure your queue with `x-dead-letter-exchange` and `x-dead-letter-routing-key` to route to a DLQ.

Targets
- netstandard2.0, net8.0
