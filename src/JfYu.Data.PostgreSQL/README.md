# JfYu.Data.PostgreSQL

JfYu.Data provider for PostgreSQL.

```csharp
services.AddJfYuPostgreSql();
services.AddJfYuDbContext<MyDbContext>(options =>
{
    options.DatabaseType = DatabaseType.PostgreSQL;
    options.ConnectionString = "...";
});
```
