# JfYu.Data.MySql

JfYu.Data provider for MySQL and MariaDB.

```csharp
services.AddJfYuMySql();
services.AddJfYuDbContext<MyDbContext>(options =>
{
    options.DatabaseType = DatabaseType.MySql; // or MariaDB
    options.ConnectionString = "...";
});
```
