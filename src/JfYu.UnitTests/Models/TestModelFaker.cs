using Bogus;

namespace JfYu.UnitTests.Models
{
    public class TestModelFaker : Faker<TestModel>
    {
        public TestModelFaker()
        {
            RuleFor(o => o.Id, f => f.Random.Number(1, 10000000));
            RuleFor(o => o.Age, f => f.Random.Number(1, 100));
            RuleFor(o => o.Name, f => f.Name.FirstName());
            RuleFor(o => o.Address, f => f.Address.FullAddress());
            RuleFor(o => o.DateTime, f => f.Date.Recent().ToUniversalTime());
            RuleFor(u => u.Sub, f => new TestSubModelFaker().Generate());
            RuleFor(u => u.Items, f => [.. new TestSubModelFaker().Generate(f.Random.Number(1, 10))]);
        }
    }
}