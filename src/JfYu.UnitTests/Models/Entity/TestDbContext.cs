#if NET8_0_OR_GREATER
using Microsoft.EntityFrameworkCore;

namespace JfYu.UnitTests.Models.Entity
{
    public class TestDbContext : DbContext
    {
        public DbSet<TestModel> TestModels { get; set; }
        public DbSet<TestSubModel> TestSubModels { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }        
    }
}
#endif