
### RabbitMQ

```
Install-Package jfYu.Core.RabbitMQ
``` 

config

```
 "RabbitMQ": {
    "HostName": "127.0.0.1",
    "UserName": "xxx",
    "Password": "123456",
    "VirtualHost": "/",
    "HeartBeat": "60",
    "Port": "9808",
    "DispatchConsumersAsync":true //if you want to use async consumer
  }
```

injection

```
 services.AddRabbitMQService(config);
```

send

```

mq.Send("test", "xxxxxx");
mq.Send("test", "test");
mq.Send("test", new TestModel() { Id = 122, Name = "name" });

mq.QueueBind("q12", "ex12", ExchangeType.Topic);
mq.Send("ex12", ExchangeType.Fanout, "Fanout", "123");
```

Receive
```
mq.Receive("topic", q =>
{
    Assert.NotEmpty(q);
    Thread.Sleep(1000);
});

mq.ReceiveAsync("topic", async q =>
{

    Assert.NotEmpty(q);
    await Task.Delay(5000);
});
```
