# JfYu.Data

EF Core read-write separation with multi-database support and simple CRUD services.

Supported databases: SqlServer, MySql, MariaDB, Sqlite, InMemory

## Install

```powershell
Install-Package JfYu.Data
```

## Configuration (appsettings.json)

Unit tests bind configuration from section `JfYuConnectionStrings`:

```json
"JfYuConnectionStrings": {
 "DatabaseType": "SqlServer", // SqlServer | MySql | MariaDB | Sqlite | Memory
 "ConnectionString": "Data Source=127.0.0.1,9004;database=dbtest;User Id=sa;Password=123456;Encrypt=True;TrustServerCertificate=True;",
 "JfYuReadOnly": "JfYuReadOnly", // IOC key prefix for readonly contexts
 "ReadOnlyDatabases": [
 {
 "DatabaseType": "MySql",
 "ConnectionString": "server=127.0.0.1;userid=root;pwd=123456;port=9001;database=dbtest;",
 "Version": "8.0.36" // optional for MySql/MariaDB; if missing uses AutoDetect
 },
 {
 "DatabaseType": "Sqlite",
 "ConnectionString": "Data Source=data/m2.db;Password=123456;"
 },
 {
 "DatabaseType": "Memory",
 "ConnectionString": "MemoryDbName"
 }
 ]
}
```

Tips
- When no `ReadOnlyDatabases` configured, read operations fall back to master.

## Dependency Injection

Minimal

```csharp
services.AddJfYuDbContext<DataContext>(o =>
{
 o.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
});
```

With readonly replicas and extra EF options

```csharp
services.AddJfYuDbContext<DataContext>(o =>
{
 o.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
 o.ReadOnlyDatabases =
 [
 new() { DatabaseType = DatabaseType.SqlServer, ConnectionString = "server=127.0.0.2;Database=Test;uid=Test;pwd=test;" },
 new() { DatabaseType = DatabaseType.SqlServer, ConnectionString = "server=127.0.0.3;Database=Test;uid=Test;pwd=test;" }
 ];
}, db => db.EnableDetailedErrors().EnableSensitiveDataLogging());
```

Bind from configuration

```csharp
services.AddJfYuDbContext<DataContext>(options =>
{
 configuration.GetSection("JfYuConnectionStrings").Bind(options);
 // In tests we set an in-memory database name here when needed
 // options.ConnectionString = "TestDb_...";
});
```

Resolve registered services

```csharp
// Master context
var master = provider.GetRequiredService<DataContext>();

// Random readonly context (falls back to master if none configured)
var readonlyContext = provider.GetRequiredService<ReadonlyDBContext<DataContext>>().Current;

// Access specific readonly replica by key
var ro0 = provider.GetKeyedService<DataContext>("JfYuReadOnly0");
```

## Service usage (`IService<T, TContext>`)

```csharp
var svc = provider.GetRequiredService<IService<User, DataContext>>();

// Create
await svc.AddAsync(new User { UserName = "u1", NickName = "n1" });
await svc.AddAsync(new List<User> { new() { UserName = "u2" }, new() { UserName = "u3" } });

// Update single entity
var one = await svc.GetOneAsync();
one!.UserName = "updated";
await svc.UpdateAsync(one);

// Update a range
var some = await svc.GetListAsync();
some = [.. some.Take(3)];
await svc.UpdateAsync(some);

// Update by predicate with selector
await svc.UpdateAsync(q => some.Select(x => x.Id).Contains(q.Id), (i, e) =>
{
 e.UserName = $"U{i}";
});

// Soft delete (sets Status = Disable)
await svc.RemoveAsync(q => some.Select(x => x.Id).Contains(q.Id));

// Hard delete
await svc.HardRemoveAsync(q => some.Select(x => x.Id).Contains(q.Id));

// Query
var first = await svc.GetOneAsync(q => q.UserName == "updated");
var list = await svc.GetListAsync();
var projected = await svc.GetSelectListAsync(x => new { x.Id, x.UserName });
```

## Paging extensions

IQueryable paging

```csharp
var q = users.AsQueryable();
var page1 = q.ToPaged(); // default pageIndex=0, pageSize=10
var page2 = q.ToPaged(1,5); // index=1, size=5

// Convert items while paging
var converted = q.ToPaged(src => src.Select(x => new TestSubModel
{
 Id = x.Id,
 CardNum = x.Sub!.CardNum,
 ExpiresIn = x.Sub.ExpiresIn
}).ToList(),1,10);
```

EF Core async paging

```csharp
var p1 = await db.Users.ToPagedAsync();
var p2 = await db.Users.ToPagedAsync(1,5);
var p3 = await db.Users.ToPagedAsync(2,10);

var converted = await db.Users.ToPagedAsync(src => src.Select(x => new TestSubModel
{
 Id = x.Id,
 CardNum = x.UserName,
 ExpiresIn = x.CreatedTime
}).ToList(),1,10);
```

Validation behavior from tests
- Null source throws `ArgumentNullException`.
- Negative `pageIndex` or `pageSize` throws `ArgumentOutOfRangeException`.

## Models and DbContext

```csharp
public class User : BaseEntity
{
 [Required, MaxLength(100)] public string UserName { get; set; } = "";
 public string? NickName { get; set; }
 public int? DepartmentId { get; set; }
 public virtual Department? Department { get; set; }
}

public class Department : BaseEntity
{
 [Required] public string Name { get; set; } = "";
 [Required] public string SubName { get; set; } = "";
 public int? SuperiorId { get; set; }
 public virtual Department? Superior { get; set; }
 public virtual List<User>? Users { get; set; }
}

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
 public DbSet<User> Users { get; set; }
 public DbSet<Department> Departments { get; set; }
}
```

## EF Core Migrations

```powershell
# set connection string for design-time factory (example)
$env:EFConString = "Data Source=...;Initial Catalog=Test;User Id=sa;Password=...;";

# create migration
dotnet ef migrations add init --project <YourProject>

# update database
dotnet ef database update --project <YourProject>
```

If tooling is missing

```powershell
Install-Package Microsoft.EntityFrameworkCore.Tools