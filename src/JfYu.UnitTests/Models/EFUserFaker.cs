#if NET8_0_OR_GREATER
using Bogus;
using JfYu.UnitTests.Models.Entity;

namespace JfYu.UnitTests.Models
{
    public class EFUserFaker : Faker<User>
    {
        public EFUserFaker()
        {
            RuleFor(o => o.NickName, f => f.Name.FirstName());
            RuleFor(o => o.UserName, f => f.Name.FirstName());
        }
    }
}
#endif