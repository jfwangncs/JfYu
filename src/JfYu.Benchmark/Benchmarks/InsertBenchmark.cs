using BenchmarkDotNet.Attributes;
using JfYu.Benchmark.Models;
using JfYu.Data.Context;
using JfYu.Data.Service;
using Microsoft.EntityFrameworkCore;

namespace JfYu.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class InsertBenchmark
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
        _serviceUsers = faker.Generate(Count);
        _efCoreUsers = faker.Generate(Count);

        var serviceOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase($"Insert_Service_{Guid.NewGuid():N}")
            .Options;
        _serviceContext = new BenchmarkDbContext(serviceOptions);
        _service = new Service<BenchmarkUser, BenchmarkDbContext>(
            _serviceContext, new ReadonlyDBContext<BenchmarkDbContext>(_serviceContext));

        var efCoreOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase($"Insert_EfCore_{Guid.NewGuid():N}")
            .Options;
        _efCoreContext = new BenchmarkDbContext(efCoreOptions);
    }

    [Benchmark]
    public async Task<int> JfYuService()
    {
        return await _service.AddAsync(_serviceUsers).ConfigureAwait(false);
    }

    [Benchmark(Baseline = true)]
    public async Task<int> EfCore()
    {
        var now = DateTime.UtcNow;
        for (int i = 0; i < _efCoreUsers.Count; i++)
        {
            _efCoreUsers[i].CreatedTime = _efCoreUsers[i].UpdatedTime = now;
        }
        await _efCoreContext.AddRangeAsync(_efCoreUsers).ConfigureAwait(false);
        return await _efCoreContext.SaveChangesAsync().ConfigureAwait(false);
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _serviceContext.Dispose();
        _efCoreContext.Dispose();
    }
}
