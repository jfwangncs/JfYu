using BenchmarkDotNet.Attributes;
using JfYu.Benchmark.Models;
using JfYu.Data.Context;
using JfYu.Data.Service;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class SelectBenchmark
{
    [Params(1, 10, 100, 1000, 10000)]
    public int Count;

    private BenchmarkDbContext _serviceContext = null!;
    private BenchmarkDbContext _efCoreContext = null!;
    private Service<BenchmarkUser, BenchmarkDbContext> _service = null!;

    [GlobalSetup]
    public void Setup()
    {
        var faker = new BenchmarkUserFaker();
        var dbName = $"Select_{Count}_{Guid.NewGuid():N}";

        // Seed context
        var seedOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        using (var seedContext = new BenchmarkDbContext(seedOptions))
        {
            seedContext.BenchmarkUsers.AddRange(faker.Generate(Count));
            seedContext.SaveChanges();
        }

        // Service context
        var serviceOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        _serviceContext = new BenchmarkDbContext(serviceOptions);
        _service = new Service<BenchmarkUser, BenchmarkDbContext>(
            _serviceContext, new ReadonlyDBContext<BenchmarkDbContext>(_serviceContext));

        // EF Core context
        var efCoreOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        _efCoreContext = new BenchmarkDbContext(efCoreOptions);
    }

    [Benchmark]
    public async Task<IList<BenchmarkUser>> JfYuService()
    {
        return await _service.GetListAsync().ConfigureAwait(false);
    }

    [Benchmark(Baseline = true)]
    public async Task<List<BenchmarkUser>> EfCore()
    {
        return await _efCoreContext.Set<BenchmarkUser>().ToListAsync().ConfigureAwait(false);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceContext.Dispose();
        _efCoreContext.Dispose();
    }
}
