using Bogus;

namespace JfYu.Benchmark.Models;

public class BenchmarkUserFaker : Faker<BenchmarkUser>
{
    public BenchmarkUserFaker()
    {
        RuleFor(o => o.UserName, f => f.Internet.UserName());
        RuleFor(o => o.Email, f => f.Internet.Email());
    }
}
