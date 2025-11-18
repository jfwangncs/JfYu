#if NET8_0_OR_GREATER
using JfYu.Data.Constant;
using JfYu.Data.Extension;
using JfYu.Data.Service;
using JfYu.UnitTests.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Entity;

namespace JfYu.UnitTests.Data
{
    [Collection("Data")]
    public class BaseTest
    {
        [Fact]
        public void AddService_SetupActionIsNull_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddJfYuDbContext<DataContext>(null!));
        }
        [Fact]
        public void AddService_SetupActionIsEmpty_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddJfYuDbContext<DataContext>(q => { }));
        }
        [Fact]
        public void AddService_ConnectionStringIsEmpty_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddJfYuDbContext<DataContext>(q => { q.ConnectionString = ""; }));
        }
        [Fact]
        public void AddService_ConnectionStringIsNull_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddJfYuDbContext<DataContext>(q => { q.ConnectionString = null!; }));
        }

        [Fact]
        public void AddService_ConnectionStringIsWhiteSpace_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddJfYuDbContext<DataContext>(q => { q.ConnectionString = "   "; }));
        }

        [Fact]
        public void AddService_ReadOnlyConnectionStringIsEmpty_ThrowsException()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddJfYuDbContext<DataContext>(q => { q.ConnectionString = "test"; q.ReadOnlyDatabases = [new() { ConnectionString = "" }]; }));
        }

        [Fact]
        public void AddService_WithExtraConfigure_Correctly()
        {
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
            }, q => q.EnableDetailedErrors().EnableSensitiveDataLogging());
            var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<DataContext>();
            var useService = serviceProvider.GetService<IService<User, DataContext>>();

            Assert.NotNull(dbContext);
            Assert.NotNull(useService);
        }

        [Fact]
        public void AddService_MainDatabase_Correctly()
        {
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
            });
            var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<DataContext>();
            var useService = serviceProvider.GetService<IService<User, DataContext>>();

            Assert.NotNull(dbContext);
            Assert.NotNull(useService);
        }

        [Fact]
        public void AddService_MainDatabase1_Correctly()
        {
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
                q.ReadOnlyDatabases = [];
            });
            var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<DataContext>();
            var useService = serviceProvider.GetService<IService<User, DataContext>>();

            Assert.NotNull(dbContext);
            Assert.NotNull(useService);
            Assert.Contains("127.0.0.1", useService.Context.Database.GetConnectionString());
            Assert.Contains("127.0.0.1", useService.ReadonlyContext.Database.GetConnectionString());
        }

        [Fact]
        public void AddService_MainDatabaseWithReadOnly1_Correctly()
        {
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
                q.ReadOnlyDatabases = [new() { ConnectionString = "server=127.0.0.2;Database=Test;uid=Test;pwd=test;" }];
            });
            var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<DataContext>();
            var dbContextReadOnly0 = serviceProvider.GetKeyedService<DataContext>($"JfYuReadOnly0");
            var useService = serviceProvider.GetService<IService<User, DataContext>>();
            Assert.NotNull(dbContext);
            Assert.NotNull(useService);
            Assert.NotNull(dbContextReadOnly0);
            Assert.Contains("127.0.0.2", dbContextReadOnly0.Database.GetConnectionString());

            Assert.Contains("127.0.0.1", useService.Context.Database.GetConnectionString());
            Assert.Contains("127.0.0.2", useService.ReadonlyContext.Database.GetConnectionString());
        }

        [Fact]
        public void AddService_MainDatabaseWithReadOnly2_Correctly()
        {
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
                q.ReadOnlyDatabases = [
                    new () { DatabaseType = DatabaseType.SqlServer, ConnectionString = "server=127.0.0.2;Database=Test;uid=Test;pwd=test;" },
                    new () { DatabaseType = DatabaseType.SqlServer, ConnectionString = "server=127.0.0.3;Database=Test;uid=Test;pwd=test;" } ];
            });
            var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<DataContext>();
            var dbContextReadOnly0 = serviceProvider.GetKeyedService<DataContext>("JfYuReadOnly0");
            var dbContextReadOnly1 = serviceProvider.GetKeyedService<DataContext>("JfYuReadOnly1");
            var useService = serviceProvider.GetService<IService<User, DataContext>>();
            Assert.NotNull(dbContext);
            Assert.NotNull(useService);
            Assert.NotNull(dbContextReadOnly0);
            Assert.Contains("127.0.0.2", dbContextReadOnly0.Database.GetConnectionString());
            Assert.NotNull(dbContextReadOnly1);
            Assert.Contains("127.0.0.3", dbContextReadOnly1.Database.GetConnectionString());

            Assert.Contains("127.0.0.1", useService.Context.Database.GetConnectionString());
            Assert.True(useService.ReadonlyContext.Database.GetConnectionString()!.Contains("127.0.0.2") || useService.ReadonlyContext.Database.GetConnectionString()!.Contains("127.0.0.3"));
        }

        [Fact]
        public void AddService_MainDatabaseWithReadOnly3_Correctly()
        {
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.ConnectionString = "server=127.0.0.1;Database=Test;uid=Test;pwd=test;";
                q.ReadOnlyDatabases = [
                    new () { DatabaseType = DatabaseType.SqlServer, ConnectionString = "server=127.0.0.2;Database=Test;uid=Test;pwd=test;" },
                    new () { DatabaseType = DatabaseType.SqlServer, ConnectionString = "server=127.0.0.3;Database=Test;uid=Test;pwd=test;" },
                    new () { DatabaseType = DatabaseType.Sqlite, ConnectionString = "server=127.0.0.4;Database=Test;uid=Test;pwd=test;" }];
            });
            var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<DataContext>();
            var dbContextReadOnly0 = serviceProvider.GetKeyedService<DataContext>("JfYuReadOnly0");
            var dbContextReadOnly1 = serviceProvider.GetKeyedService<DataContext>("JfYuReadOnly1");
            var dbContextReadOnly2 = serviceProvider.GetKeyedService<DataContext>("JfYuReadOnly2");
            var useService = serviceProvider.GetService<IService<User, DataContext>>();
            Assert.NotNull(dbContext);
            Assert.NotNull(useService);
            Assert.NotNull(dbContextReadOnly0);
            Assert.Contains("127.0.0.2", dbContextReadOnly0.Database.GetConnectionString());
            Assert.NotNull(dbContextReadOnly1);
            Assert.Contains("127.0.0.3", dbContextReadOnly1.Database.GetConnectionString());
            Assert.NotNull(dbContextReadOnly2);
            Assert.Contains("127.0.0.4", dbContextReadOnly2.Database.GetConnectionString());

            Assert.Contains("127.0.0.1", useService.Context.Database.GetConnectionString());
            Assert.True(useService.ReadonlyContext.Database.GetConnectionString()!.Contains("127.0.0.2") || useService.ReadonlyContext.Database.GetConnectionString()!.Contains("127.0.0.3") || useService.ReadonlyContext.Database.GetConnectionString()!.Contains("127.0.0.4"));
        }

        [Fact]
        public void AddService_UseMySql_Branch()
        {
            // Act
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.DatabaseType = DatabaseType.MySql;
                q.ConnectionString = "Server=test.com;Database=Fake;Uid=root;Pwd=Pwd;";
                q.Version = "1.2.3";
            });
            var serviceProvider = services.BuildServiceProvider();
            // Assert
            var dbContext = serviceProvider.GetService<DataContext>();

            Assert.NotNull(dbContext);
        }

        [Fact]
        public void AddService_UseMariaDB_Branch()
        {
            // Act
            var services = new ServiceCollection();
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.DatabaseType = DatabaseType.MariaDB;
                q.ConnectionString = "Server=test.com;Database=Fake;Uid=root;Pwd=Pwd;";
                q.Version = "1.2.3";
            });
            var serviceProvider = services.BuildServiceProvider();
            // Assert
            var dbContext = serviceProvider.GetService<DataContext>();

            Assert.NotNull(dbContext);
        }

        [Fact]
        public void AddService_UseMySql_WithoutVersion_AutoDetect()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            
            // This will trigger ServerVersion.AutoDetect(config.ConnectionString)
            // because Version is not provided (null/empty)
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.DatabaseType = DatabaseType.MySql;
                q.ConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
                // Version is intentionally not set to trigger AutoDetect branch
            });
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Assert
            // This will fail to connect to actual MySQL, but it will execute
            // the ServerVersion.AutoDetect code path, increasing coverage
            var exception = Assert.Throws<MySqlConnector.MySqlException>(() =>
            {
                var dbContext = serviceProvider.GetRequiredService<DataContext>();
                // Accessing the database will trigger connection attempt
                _ = dbContext.Database;
            });
            
            // The exception is expected since we don't have a real MySQL server
            Assert.NotNull(exception);
        }

        [Fact]
        public void AddService_UseMariaDB_WithoutVersion_AutoDetect()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            
            // This will trigger ServerVersion.AutoDetect(config.ConnectionString)
            // for MariaDB because Version is not provided (null/empty)
            services.AddJfYuDbContext<DataContext>(q =>
            {
                q.DatabaseType = DatabaseType.MariaDB;
                q.ConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
                // Version is intentionally not set to trigger AutoDetect branch
            });
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Assert
            // This will fail to connect to actual MariaDB, but it will execute
            // the ServerVersion.AutoDetect code path, increasing coverage
            var exception = Assert.Throws<MySqlConnector.MySqlException>(() =>
            {
                var dbContext = serviceProvider.GetRequiredService<DataContext>();
                // Accessing the database will trigger connection attempt
                _ = dbContext.Database;
            });
            
            // The exception is expected since we don't have a real MariaDB server
            Assert.NotNull(exception);
        }
    }
}
#endif