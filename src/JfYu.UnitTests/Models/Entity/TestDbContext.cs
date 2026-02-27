#if NET8_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.SQLite;
#endif
namespace JfYu.UnitTests.Models.Entity
{
#if NET8_0_OR_GREATER
    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestModel> TestModels { get; set; }
        public DbSet<TestSubModel> TestSubModels { get; set; }
    }
#else
    public class TestDbContext(string connectionString) : DbContext(new SQLiteConnection(connectionString), contextOwnsConnection: true)
    {
        public DbSet<TestModel> TestModels { get; set; } = null!;
        public DbSet<TestSubModel> TestSubModels { get; set; } = null!;
    }
#endif
}

