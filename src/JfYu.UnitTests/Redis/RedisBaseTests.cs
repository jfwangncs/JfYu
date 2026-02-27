using JfYu.Redis.Extensions;
using JfYu.Redis.Implementation;
using JfYu.Redis.Interface;
using JfYu.Redis.Options;
using JfYu.Redis.Serializer;
using JfYu.Redis.Serializer.MessagePack;
using JfYu.Redis.Serializer.Newtonsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class RedisBaseTests
    {
        #region TestDatas

        public class NullkeyAndKeyAndValueExpectData : TheoryData<string?, string?, string?>
        {
            public NullkeyAndKeyAndValueExpectData()
            {
                Add(null, "key", "value");
                Add("", "key", "value");
                Add("   ", "key", "value");
                Add("         ", "key", "value");
                Add("key", null, "value");
                Add("key", "", "value");
                Add("key", "    ", "value");
                Add("key", "       ", "value");
                Add("key", "key", null);
            }
        }

        public class NullKeyAndValueExpectData : TheoryData<string?, string?>
        {
            public NullKeyAndValueExpectData()
            {
                Add(null, "key");
                Add("", "key");
                Add("   ", "key");
                Add("         ", "key");
                Add("key", null);
            }
        }

        public class NullKeyAndValuesExpectData : TheoryData<string?, string?[]>
        {
            public NullKeyAndValuesExpectData()
            {
                Add(null, ["v"]);
                Add("", ["v"]);
                Add("   ", ["v"]);
                Add("         ", ["v"]);
                Add("key", null!);
                Add("key", []);
                Add("key", ["v", null!]);
            }
        }

        public class NullKeyExpectData : TheoryData<string?>
        {
            public NullKeyExpectData()
            {
                Add(null);
                Add("");
                Add("   ");
                Add("         ");
            }
        }

        public class NullKeysExpectData : TheoryData<string?[]?>
        {
            public NullKeysExpectData()
            {
                Add(null);
                Add([]);
            }
        }

        #endregion TestDatas

        #region AddRedisService

        [Fact]
        public void AddRedisService_WhenSetupActionIsNull_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddRedisService(null));
        }

        [Fact]
        public void AddRedisService_WhenEndpointsAreEmpty_ThrowsException()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddRedisService(options =>
            {
                options.EndPoints = []; // Empty list
            }));
        }

        [Fact]
        public void AddRedisService_WhenEndpointsAreNotConfigured_ThrowsException()
        {
            var services = new ServiceCollection();

            var ex = Record.Exception(() => services.AddRedisService(options =>
            {
                // No endpoints configured
            }));

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void AddRedisService_RegistersConnectionMultiplexerAndRedisServiceWithNoPrefix()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "";
                options.EnableLogs = true;
            });

            var serviceProvider = services.BuildServiceProvider();

            var connectionMultiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
            var redisService = serviceProvider.GetService<IRedisService>();

            Assert.NotNull(connectionMultiplexer);
            Assert.NotNull(redisService);
            Assert.IsType<RedisService>(redisService);
        }

        [Fact]
        public void AddRedisService_RegistersConnectionMultiplexerAndRedisService()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
            });

            var serviceProvider = services.BuildServiceProvider();

            var connectionMultiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
            var redisService = serviceProvider.GetService<IRedisService>();

            Assert.NotNull(connectionMultiplexer);
            Assert.NotNull(redisService);
            Assert.NotNull(redisService.Database);
            Assert.IsType<RedisService>(redisService);
        }

        #endregion AddRedisService

        #region UsingNewtonsoft

        [Fact]
        public void UsingNewtonsoft_RegistersNewtonsoftSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
                options.UsingNewtonsoft();
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<NewtonsoftSerializer>(serializer);
        }

        [Fact]
        public void UsingNewtonsoft_WithOptions_RegistersNewtonsoftSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
                options.UsingNewtonsoft(q => q.MaxDepth = 12);
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<NewtonsoftSerializer>(serializer);
        }

        [Fact]
        public void UsingNewtonsoft_WithNullOptions_RegistersNewtonsoftSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
                options.UsingNewtonsoft(null);
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<NewtonsoftSerializer>(serializer);
        }

        [Fact]
        public void UsesNewtonsoft_WhenNoSerializerSpecified_RegistersNewtonsoftSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<NewtonsoftSerializer>(serializer);
        }

        #endregion UsingNewtonsoft

        #region UsingMsgPack

        [Fact]
        public void UsingMsgPack_RegistersMsgPackSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
                options.UsingMsgPack();
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<MsgPackObjectSerializer>(serializer);
        }

        [Fact]
        public void UsingMsgPack_WithNullOptions_RegistersMsgPackSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
                options.UsingMsgPack(null);
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<MsgPackObjectSerializer>(serializer);
        }

        [Fact]
        public void UsingMsgPack_WithOptions_RegistersMsgPackSerializer()
        {
            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Password = "password";
                options.Timeout = 5000;
                options.SSL = false;
                options.UsingMsgPack(q => q.WithSuggestedContiguousMemorySize(1000));
            });

            var serviceProvider = services.BuildServiceProvider();

            var serializer = serviceProvider.GetService<ISerializer>();

            Assert.NotNull(serializer);
            Assert.IsType<MsgPackObjectSerializer>(serializer);
        }

        #endregion UsingMsgPack

        #region Logs

        [Fact]
        public void Logs_LogsEnabled_MethodHaveBeenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>(); 
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();
            // Act
            redisService.Log(nameof(Logs_LogsEnabled_MethodHaveBeenCalled), "key");

            // Assert
            loggerMock.Verify(logger => logger.Log(
               It.Is<LogLevel>(l => l == LogLevel.Trace),
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(Logs_LogsEnabled_MethodHaveBeenCalled)) && v.ToString()!.Contains("key")),
               It.IsAny<Exception?>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Logs_LogsEnabledButNoLogger_MethodHaveNeverBeenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
            });
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();
            // Act
            redisService.Log(nameof(Logs_LogsEnabledButNoLogger_MethodHaveNeverBeenCalled), "key");

            // Assert
            loggerMock.Verify(logger => logger.Log(LogLevel.Trace, It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((o, t) => o.ToString() == $"Redis {nameof(Logs_LogsEnabledButNoLogger_MethodHaveNeverBeenCalled)} - Key: key"),
               null,
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        [Fact]
        public void Logs_LogsDisabled_MethodHaveNeverBeenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();
            // Act
            redisService.Log(nameof(Logs_LogsEnabledButNoLogger_MethodHaveNeverBeenCalled), "key");

            // Assert
            loggerMock.Verify(logger => logger.Log(LogLevel.Trace, It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((o, t) => o.ToString() == $"Redis {nameof(Logs_LogsEnabledButNoLogger_MethodHaveNeverBeenCalled)} - Key: key"),
               null,
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        [Fact]
        public void Log_ValueIsNull_LogsEmptyString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();

            // Act
            redisService.Log(nameof(Log_ValueIsNull_LogsEmptyString), "testKey", null);

            // Assert
            loggerMock.Verify(logger => logger.Log(
               LogLevel.Trace,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(Log_ValueIsNull_LogsEmptyString)) 
                   && v.ToString()!.Contains("testKey") 
                   && v.ToString()!.Contains("Value: ")),
               It.IsAny<Exception?>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Log_ValueIsEmpty_LogsEmptyString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();

            // Act
            redisService.Log(nameof(Log_ValueIsEmpty_LogsEmptyString), "testKey", "");

            // Assert
            loggerMock.Verify(logger => logger.Log(
               LogLevel.Trace,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(Log_ValueIsEmpty_LogsEmptyString)) 
                   && v.ToString()!.Contains("testKey") 
                   && v.ToString()!.Contains("Value: ")),
               It.IsAny<Exception?>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Log_ValueFilterIsNull_LogsOriginalValue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
                // ValueFilter is null by default
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();

            // Act
            redisService.Log(nameof(Log_ValueFilterIsNull_LogsOriginalValue), "testKey", "testValue");

            // Assert
            loggerMock.Verify(logger => logger.Log(
               LogLevel.Trace,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(Log_ValueFilterIsNull_LogsOriginalValue)) 
                   && v.ToString()!.Contains("testKey") 
                   && v.ToString()!.Contains("testValue")),
               It.IsAny<Exception?>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Log_ValueFilterIsNotNull_LogsFilteredValue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
                options.ValueFilter = (value) => value.Replace("sensitive", "***");
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();

            // Act
            redisService.Log(nameof(Log_ValueFilterIsNotNull_LogsFilteredValue), "testKey", "sensitive data");

            // Assert
            loggerMock.Verify(logger => logger.Log(
               LogLevel.Trace,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(Log_ValueFilterIsNotNull_LogsFilteredValue)) 
                   && v.ToString()!.Contains("testKey") 
                   && v.ToString()!.Contains("*** data")
                   && !v.ToString()!.Contains("sensitive")),
               It.IsAny<Exception?>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Log_ValueIsNullAndValueFilterIsNotNull_LogsEmptyString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedisService>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var services = new ServiceCollection();
            services.AddRedisService(options =>
            {
                options.EndPoints.Add(new RedisEndPoint { Host = "localhost" });
                options.Timeout = 5000;
                options.SSL = false;
                options.DbIndex = 1;
                options.Prefix = "Mytest:";
                options.EnableLogs = true;
                options.ValueFilter = (value) => string.IsNullOrEmpty(value) ? "[NULL]" : value;
            });
            services.AddSingleton(loggerMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            var redisService = serviceProvider.GetRequiredService<IRedisService>();

            // Act
            redisService.Log(nameof(Log_ValueIsNullAndValueFilterIsNotNull_LogsEmptyString), "testKey", null);

            // Assert
            loggerMock.Verify(logger => logger.Log(
               LogLevel.Trace,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nameof(Log_ValueIsNullAndValueFilterIsNotNull_LogsEmptyString)) 
                   && v.ToString()!.Contains("testKey") 
                   && v.ToString()!.Contains("[NULL]")),
               It.IsAny<Exception?>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion Logs
    }
}