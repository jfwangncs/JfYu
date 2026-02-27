using Microsoft.EntityFrameworkCore;

namespace JfYu.Benchmark.Models;

public class BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : DbContext(options)
{
    public DbSet<BenchmarkUser> BenchmarkUsers { get; set; }
}
