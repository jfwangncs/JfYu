#if NET8_0_OR_GREATER
using JfYu.Data.Context;
using JfYu.Data.Service;
using JfYu.UnitTests.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace JfYu.UnitTests.Models.Service
{
    public class UserService(DataContext context, ReadonlyDBContext<DataContext> readonlyDBContext)
    : Service<User, DataContext>(context, readonlyDBContext), IUserService
    {
        public async Task<User?> GetByNickNameAsync(string nickName)
        {
            return await Context.Set<User>().FirstOrDefaultAsync(u => u.NickName == nickName).ConfigureAwait(true);
        }
    }
}
#endif