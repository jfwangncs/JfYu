using BenchmarkDotNet.Attributes;
using JfYu.Benchmark.Models;
using JfYu.Data.Context;
using JfYu.Data.Service;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class UpdateBenchmark
{
    [Params(1, 10, 100, 1000, 10000)]
    public int Count;

    private List<BenchmarkUser> _serviceUsers = null!;
    private List<BenchmarkUser> _efCoreUsers = null!;
    private BenchmarkDbContext _serviceContext = null!;
    private BenchmarkDbContext _efCoreContext = null!;
    private Service<BenchmarkUser, BenchmarkDbContext> _service = null!;

    [IterationSetup]
    public void Setup()
    {
        var faker = new BenchmarkUserFaker();

        // Service context -- seed data
        var serviceOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase($"Update_Service_{Guid.NewGuid():N}")
            .Options;
        _serviceContext = new BenchmarkDbContext(serviceOptions);
        _serviceContext.BenchmarkUsers.AddRange(faker.Generate(Count));
        _serviceContext.SaveChanges();

        _service = new Service<BenchmarkUser, BenchmarkDbContext>(
            _serviceContext, new ReadonlyDBContext<BenchmarkDbContext>(_serviceContext));

        _serviceUsers = _serviceContext.BenchmarkUsers.ToList();
        for (int i = 0; i < _serviceUsers.Count; i++)
        {
            _serviceUsers[i].UserName = $"Updated_{i}";
        }

        // EF Core context -- seed data
        var efCoreOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase($"Update_EfCore_{Guid.NewGuid():N}")
            .Options;
        _efCoreContext = new BenchmarkDbContext(efCoreOptions);
        _efCoreContext.BenchmarkUsers.AddRange(faker.Generate(Count));
        _efCoreContext.SaveChanges();

        _efCoreUsers = _efCoreContext.BenchmarkUsers.ToList();
        for (int i = 0; i < _efCoreUsers.Count; i++)
        {
            _efCoreUsers[i].UserName = $"Updated_{i}";
        }
    }

    [Benchmark]
    public async Task<int> JfYuService()
    {
        return await _service.UpdateAsync(_serviceUsers).ConfigureAwait(false);
    }

    [Benchmark(Baseline = true)]
    public async Task<int> EfCore()
    {
        var now = DateTime.UtcNow;
        for (int i = 0; i < _efCoreUsers.Count; i++)
        {
            _efCoreUsers[i].UpdatedTime = now;
        }
        _efCoreContext.UpdateRange(_efCoreUsers);
        return await _efCoreContext.SaveChangesAsync().ConfigureAwait(false);
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _serviceContext.Dispose();
        _efCoreContext.Dispose();
    }
}
