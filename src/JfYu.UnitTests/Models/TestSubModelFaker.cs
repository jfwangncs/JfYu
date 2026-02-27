using Bogus;
namespace JfYu.UnitTests.Models
{


    public class TestSubModelFaker : Faker<TestSubModel>
    {
        public TestSubModelFaker()
        {
            RuleFor(o => o.Id, f => f.Random.Number(1, 100000));
            RuleFor(o => o.CardNum, f => f.Random.Number(100000000, 900000000).ToString());
            RuleFor(o => o.ExpiresIn, f => f.Date.Recent().ToUniversalTime());
        }
    }
}