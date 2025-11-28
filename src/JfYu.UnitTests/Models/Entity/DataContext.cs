#if NET8_0_OR_GREATER 
using Microsoft.EntityFrameworkCore;

namespace JfYu.UnitTests.Models.Entity
{

    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
    }
}
#endif