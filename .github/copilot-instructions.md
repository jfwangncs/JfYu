# JfYu Project Instructions

## Project Overview

Multi-targeted .NET toolkit library providing reusable components for data access, HTTP requests, and message queuing. Published as NuGet packages (`JfYu.Data`, `JfYu.Request`, `JfYu.RabbitMQ`) with automated CI/CD via GitHub Actions.

## Architecture

### Component Structure

- **JfYu.Data**: EF Core read-write separation with multi-database support (SqlServer, MySql, Sqlite, InMemory)
- **JfYu.Request**: HTTP request abstraction supporting HttpClient/HttpWebRequest with configurable logging
- **JfYu.RabbitMQ**: RabbitMQ client wrapper with async message publishing/consuming, automatic retry, and dead letter queue support
- **JfYu.WeChat**: WeChat Mini Program integration with typed APIs for authentication, access token management, and phone number retrieval
- **JfYu.UnitTests**: Multi-framework tests (net481, net8.0, net9.0) with xUnit

### Multi-Targeting Strategy

- **JfYu.Request**: `netstandard2.0;net8.0` for broad compatibility
- **JfYu.RabbitMQ**: `netstandard2.0;net8.0` for broad compatibility
- **JfYu.WeChat**: `netstandard2.0;net8.0` for broad compatibility
- **JfYu.Data**: `net8.0` only (requires modern EF Core features)
- **JfYu.UnitTests**: `net481;net8.0;net9.0` for comprehensive testing
- Use `#if NET8_0_OR_GREATER` preprocessor directives to conditionally compile Data-dependent code

## Key Patterns

### Dependency Injection Registration

All libraries use extension methods for service registration:

```csharp
// JfYu.Data - Read/Write separation with random load balancing
services.AddJfYuDbContextService<DataContext>(options => {
    options.ConnectionString = "...";  // Master DB
    options.ReadOnlyDatabases = [new DatabaseConfig { ... }];  // Slave DBs
});

// JfYu.Request - HTTP client with optional logging filters
services.AddJfYuHttpRequest(q => {
    q.LoggingFields = JfYuLoggingFields.All;
    q.RequestFilter = x => x;  // Transform before logging
    q.ResponseFilter = x => x; // Transform after logging
});

// JfYu.RabbitMQ - Message queue with retry policy
services.AddRabbitMQ((factory, options) => {
    factory.HostName = "localhost";
    factory.UserName = "guest";
    factory.Password = "guest";
    factory.VirtualHost = "/";
    factory.Port = 5672;
    factory.DispatchConsumersAsync = true;

    options.MaxRetryCount = 3;           // Retry before DLQ
    options.RetryDelayMilliseconds = 5000;
    options.MaxOutstandingConfirms = 1000;
    options.BatchSize = 20;
});

// JfYu.WeChat - WeChat Mini Program integration
services.AddWeChat(q => {
    q.AppId = "wx1234567890abcdef";      // Mini Program AppId
    q.AppSecret = "secret123...";;       // Mini Program AppSecret
});
```

### Read-Write Separation Pattern

Service classes (`IService<T, TContext>`) automatically provide:

- `Context`: Master database for writes
- `ReadonlyContext`: Randomly selected slave for reads
- If no read replicas configured, falls back to master

### Configuration Binding

Tests use layered JSON configuration with `appsettings.local.json` overriding `appsettings.json`. In CI/CD, secrets are injected by modifying appsettings.json before tests run.

## Development Workflows

### Building and Testing

```cmd
cd src
dotnet restore
dotnet build --configuration Release
dotnet test -f net8.0  # Or net481, net9.0
```

### Running Tests with Coverage

```cmd
# Generate OpenCover format (net8.0)
dotnet test -f net8.0 /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=opencover

# Generate LCOV format (net9.0)
dotnet test -f net9.0 /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=lcov
```

### CI/CD Pipeline (gate.yml)

- **ubuntu**: Tests net8.0/9.0, generates coverage for SonarCloud
- **win**: Tests net481/8.0/9.0, uploads to Coveralls
- **mac**: Tests net8.0/9.0
- **sonarcloud**: Aggregates test results and coverage analysis
- **Secrets injection**: Before tests run, RabbitMQ credentials are injected into `appsettings.json` using platform-specific sed/PowerShell commands

### Publishing (deploy.yml)

Triggered on master branch push. Automatically publishes all `*.nupkg` files to NuGet.org with `--skip-duplicate` flag.

## Code Conventions

### Entity Models

- Inherit from `BaseEntity` which provides `Id`, `CreatedTime`, `UpdatedTime`, `Status`
- Use `required` keyword for non-nullable properties
- Leverage EF Core navigation properties with `virtual` for lazy loading

### Test Organization

- Collections: Use `[Collection("Data")]`, `[Collection("JfYuRequest")]`, `[Collection("RabbitMQ")]`, and `[Collection("WeChat")]` to prevent parallel test conflicts
- Conditional compilation: Wrap Data tests in `#if NET8_0_OR_GREATER`
- Test database isolation: Use `DataContext.Clear<T>()` extension method or unique in-memory DB names
- RabbitMQ tests: Use unique queue/exchange names to avoid conflicts

### Logging and Filtering

JfYu.Request provides `LogFilter` with:

- `LoggingFields`: Control what gets logged (None/All/selective)
- `RequestFilter`/`ResponseFilter`: Sanitize sensitive data before logging

## Common Pitfalls

- **Don't** reference JfYu.Data in net481 projects - it's net8.0 only
- **Don't** forget `CopyToOutputDirectory` for test assets (appsettings, test files)
- **Remember** xUnit collections share fixture state - use isolation techniques
- **Use** `ReadonlyContext` for queries to leverage read replicas
- **Set** `NoWarn` for SYSLIB0014 in JfYu.Request (intentional use of obsolete APIs for netstandard2.0 compat)

## Project Files Reference

- Service registration: `src/JfYu.Data/Extension/ContainerBuilderExtensions.cs`, `src/JfYu.Request/Extension/ContainerBuilderExtensions.cs`, `src/JfYu.RabbitMQ/ContainerBuilderExtensions.cs`, `src/JfYu.WeChat/ContainerBuilderExtensions.cs`
- Core service: `src/JfYu.Data/Service/Service.cs`
- RabbitMQ service: `src/JfYu.RabbitMQ/RabbitMQService.cs`
- WeChat service: `src/JfYu.WeChat/MiniProgram.cs`
- Test utilities: `src/JfYu.UnitTests/Common.cs`
- CI workflows: `.github/workflows/gate.yml`, `.github/workflows/deploy.yml`
