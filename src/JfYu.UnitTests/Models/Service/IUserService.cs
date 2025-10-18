#if NET8_0_OR_GREATER
using JfYu.Data.Service;
using JfYu.UnitTests.Models.Entity;

namespace JfYu.UnitTests.Models.Service
{
    public interface IUserService : IService<User, DataContext>
    {
        Task<User?> GetByNickNameAsync(string nickName);
    }
}
#endif