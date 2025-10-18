

###  EF Read-Write Separation

MySql,SqlServer,Sqlite 

```
Install-Package JfYu.Data
```

Configuration

```
 "ConnectionStrings": {
    "DatabaseType": "SqlServer",
    "ConnectionString": "Data Source = 127.0.0.1,9004; database = dbtest; User Id = sa; Password = 123456;Encrypt=True;TrustServerCertificate=True;",
    "JfYuReadOnly":"JfYuReadOnly",
    "ReadOnlyDatabases": [
      {
        "DatabaseType": "MySql", 
        "ConnectionString": "server=127.0.0.1;userid=root;pwd=123456;port=9001;database=dbtest;"
      },
      {
        "DatabaseType": "Sqlite",  
        "ConnectionString": "Data Source= data/m2.db;Password = 123456;"
      },
      {
        "DatabaseType": "Memory",  
        "ConnectionString": "Memory" //memory db name
      }
    ]   
  }
```

Create DbContext

```
    public class User : BaseEntity
    {
        /// <summary>
        /// UserName
        /// </summary>
        [DisplayName("UserName"), Required, MaxLength(100)]
        public required string UserName { get; set; }

        /// <summary>
        /// NickName
        /// </summary>
        [DisplayName("NickName"), Required, MaxLength(100)]
        public string? NickName { get; set; }

        /// <summary>
        /// DepartmentId
        /// </summary>
        [DisplayName("DepartmentId")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Department
        /// </summary>
        public virtual Department? Department { get; set; }

    }

    public class Department : BaseEntity
    {
        /// <summary>
        /// Name
        /// </summary>
        [DisplayName("Name"), Required]
        public required  string Name { get; set; }

        /// <summary>
        /// SubName
        /// </summary>
        [DisplayName("SubName"), Required]
        public required string SubName { get; set; }

        /// <summary>
        /// SuperiorId
        /// </summary>
        [DisplayName("SuperiorId")]
        public int? SuperiorId { get; set; }

        /// <summary>
        /// Superior
        /// </summary>
        [DisplayName("Superior")]
        public virtual Department? Superior { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        [DisplayName("Users")]
        public virtual List<User>? Users { get; set; }

    }

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }

    }

  //Design Factory
    class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("EFConString");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("The connection string was not set in the 'EFConString' environment variable.");
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(connectionString);
            //optionsBuilder.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString));
            return new DataContext(optionsBuilder.Options);
        }
    }
```

Migration

```
//set migration connection string
$env:EFConString="Data Source = xxx; database = test; User Id = sa; Password = xxx;";
//create migration
dotnet ef migrations add init --project XXXX  
//update database
dotnet ef  database update --project XXXX

```
if encountered runtime error add following package
```
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="x.x.x" />  

or

Install-Package Microsoft.EntityFrameworkCore.Tools

or

dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Injection

```

services.AddJfYuDbContextService<DataContext>(q =>
    {
        q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
        q.ReadOnlyDatabases = new List<DatabaseConfig>() { new DatabaseConfig() { ConnectionString = "server=127.0.0.2;Database=Test;uid=Test;pwd=test;" } };
    });

services.AddJfYuDbContextService<DataContext>(options =>
    {
        configuration.GetSection("ConnectionStrings").Bind(options);
    });
```

Usage

```
//master
var _masterContext = serviceProvider.GetService<DataContext>();

//readonly
serviceProvider.GetService<ReadonlyDBContext<DataContext>>();

//service
var _companyService=serviceProvider.GetService<IService<Company, DataContext>>();


await _companyService.AddAsync(new Company() { Age = 33, Name = "test" }
await _companyService.UpdateAsync(data)
await _companyService.RemoveAsync(q => q.ID.Equals(data.ID))
_companyService.GetList(q => q.Name == "test124")

```
Expand

```
    public interface IUserService : IService<User, DataContext>
    {
        Task<User?> GetByNickNameAsync(string nickName);
    }
    public class UserService(DataContext context, ReadonlyDBContext<DataContext> readonlyDBContext)
    : Service<User, DataContext>(context, readonlyDBContext), IUserService
    {
        public async Task<User?> GetByNickNameAsync(string nickName)
        {
            return await Context.Set<User>().FirstOrDefaultAsync(u => u.NickName == nickName);
        }
    }

//IOC
 services.AddScoped<IUserService, UserService>();
```