#if NET8_0_OR_GREATER
using JfYu.Data.Model;
using JfYu.UnitTests.Models;
using JfYu.UnitTests.Models.Entity;
using JfYu.UnitTests.Models.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Entity;

namespace JfYu.UnitTests.Data
{

    [Collection("Data")]
    public class ServiceTest
    {
        private readonly UserService _userService;
        private readonly DataContext _context;
        public ServiceTest()
        {
            var services = new ServiceCollection();
            services.AddScoped<IUserService, UserService>();
            var serviceProvider = services.AddDataContextServices().BuildServiceProvider();
            _userService = (UserService)serviceProvider.GetRequiredService<IUserService>();
            _context = serviceProvider.GetRequiredService<DataContext>();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetByNickNameAsync_Correctly()
        {
            var user = new User
            {
                UserName = "Test",
                NickName = "Test"
            };
            var result = await _userService.AddAsync(user).ConfigureAwait(true);
            Assert.Equal(1, result);
            var data = await _userService.GetByNickNameAsync(user.NickName).ConfigureAwait(true);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task AddAsync_Correctly()
        {
            var user = new User
            {
                UserName = "Test",
                NickName = "Test"
            };
            var result = await _userService.AddAsync(user).ConfigureAwait(true);
            Assert.Equal(1, result);
            var data = await _userService.GetOneAsync(q => q.Id.Equals(user.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task AddAsync_Range_Correctly()
        {
            _context.Clear<User>();
            var users = new EFUserFaker().Generate(8);
            var result = await _userService.AddAsync(users).ConfigureAwait(true);
            Assert.Equal(8, result);
            var data = await _userService.GetListAsync().ConfigureAwait(true);
            Assert.Equal(8, data.Count);
        }

        [Fact]
        public async Task UpdateAsync_Id_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(1)).ConfigureAwait(true);
            _context.Departments.Add(new Department() { Id = 213, Name = "test" });
            await _context.SaveChangesAsync();
            var user = await _userService.GetOneAsync().ConfigureAwait(true);
            user!.Status = (int)DataStatus.Disable;
            user.DepartmentId = 213;
            user.UserName = "Test";
            var result = await _userService.UpdateAsync(user).ConfigureAwait(true);
            Assert.Equal(1, result);
            var data = await _userService.GetOneAsync(q => q.Id.Equals(user.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
            Assert.Equal(data.Status, user.Status);
            Assert.Equal(data.DepartmentId, user.DepartmentId);
            Assert.Equal(data.UserName, user.UserName);
            Assert.Equal(data.NickName, user.NickName);
        }

        [Fact]
        public async Task UpdateAsync_Again_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(1)).ConfigureAwait(true);
            _context.Departments.Add(new Department() { Id = 213, Name = "test" });
            _context.Departments.Add(new Department() { Id = 567, Name = "test1" });
            await _context.SaveChangesAsync();
            var user = await _userService.GetOneAsync().ConfigureAwait(true);
            user!.Status = (int)DataStatus.Disable;
            user.DepartmentId = 213;
            user.UserName = "Test";
            var result = await _userService.UpdateAsync(user).ConfigureAwait(true);
            Assert.Equal(1, result);
            var data = await _userService.GetOneAsync(q => q.Id.Equals(user.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
            Assert.Equal(data.Status, user.Status);
            Assert.Equal(data.DepartmentId, user.DepartmentId);
            Assert.Equal(data.UserName, user.UserName);
            Assert.Equal(data.NickName, user.NickName);

            user.Status = (int)DataStatus.Active;
            user.DepartmentId = 567;
            user.UserName = "Test213";
            result = await _userService.UpdateAsync(user).ConfigureAwait(true);
            Assert.Equal(1, result);
            data = await _userService.GetOneAsync(q => q.Id.Equals(user.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
            Assert.Equal(data.Status, user.Status);
            Assert.Equal(data.DepartmentId, user.DepartmentId);
            Assert.Equal(data.UserName, user.UserName);
            Assert.Equal(data.NickName, user.NickName);
        }

        [Fact]
        public async Task UpdateAsync_Range_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(12)).ConfigureAwait(true);
            var users = await _userService.GetListAsync().ConfigureAwait(true);
            users = [.. users.Take(7).OrderBy(q => q.Id)];
            for (int i = 0; i < users.Count; i++)
            {
                _context.Departments.Add(new Department() { Id = i + 1, Name = "test" });
                var user = users[i];
                user!.Status = (int)DataStatus.Disable;
                user.DepartmentId = i + 1;
                user.UserName = $"Test{i}";
            }
            await _context.SaveChangesAsync();
            var result = await _userService.UpdateAsync([.. users]).ConfigureAwait(true);
            Assert.Equal(7, result);
            var data = await _userService.GetListAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
            data = [.. data.OrderBy(q => q.Id)];
            for (int i = 0; i < data.Count; i++)
            {
                Assert.Equal(data[i].Status, users[i].Status);
                Assert.Equal(data[i].DepartmentId, users[i].DepartmentId);
                Assert.Equal(data[i].UserName, users[i].UserName);
                Assert.Equal(data[i].NickName, users[i].NickName);
            }
        }

        [Fact]
        public async Task UpdateAsync_Predicate_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(12)).ConfigureAwait(true);

            var users = await _userService.GetListAsync().ConfigureAwait(true);
            users = [.. users.Take(6).OrderBy(q => q.Id)];

            for (int i = 0; i < users.Count; i++)
            {
                _context.Departments.Add(new Department() { Id = i + 1, Name = "test" });
            }
            await _context.SaveChangesAsync();
            var result = await _userService.UpdateAsync(q => users.Select(q => q.Id).Contains(q.Id), (i, q) =>
            {
                q.Status = (int)DataStatus.Disable;
                q.DepartmentId = i + 1;
                q.UserName = $"Test{i}";
            }).ConfigureAwait(true);
            Assert.Equal(6, result);
            var data = await _userService.GetListAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
            data = [.. data.OrderBy(q => q.Id)];
            for (int i = 0; i < data.Count; i++)
            {
                Assert.Equal(data[i].Status, users[i].Status);
                Assert.Equal(data[i].DepartmentId, users[i].DepartmentId);
                Assert.Equal(data[i].UserName, users[i].UserName);
                Assert.Equal(data[i].NickName, users[i].NickName);
            }
        }

        [Fact]
        public async Task UpdateAsync_PredicateReturnEmpty_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(12)).ConfigureAwait(true);

            var users = new List<User>();

            var result = await _userService.UpdateAsync(q => users.Select(q => q.Id).Contains(q.Id), (i, q) =>
            {
                q.Status = (int)DataStatus.Disable;
                q.DepartmentId = i + 1;
                q.UserName = $"Test{i}";
            }).ConfigureAwait(true);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task UpdateAsync_PredicateOrScalarIsNull_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(12)).ConfigureAwait(true);

            var users = new List<User>();

            var result = await _userService.UpdateAsync(null!, (i, q) =>
            {
                q.Status = (int)DataStatus.Disable;
                q.DepartmentId = i + 1;
                q.UserName = $"Test{i}";
            }).ConfigureAwait(true);
            Assert.Equal(0, result);

            result = await _userService.UpdateAsync(q => users.Select(q => q.Id).Contains(q.Id), null!).ConfigureAwait(true);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task RemoveAsync_PredicateReturnEmptyOrNull_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var users = new List<User>();

            var result = await _userService.RemoveAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.Equal(0, result);

            result = await _userService.RemoveAsync(null!).ConfigureAwait(true);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task RemoveAsync_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var users = await _userService.GetListAsync().ConfigureAwait(true);
            users = [.. users.Take(6).OrderBy(q => q.Id)];

            var result = await _userService.RemoveAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.Equal(6, result);

            var data = await _userService.GetListAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.NotNull(data);
            for (int i = 0; i < data.Count; i++)
            {
                Assert.Equal((int)DataStatus.Disable, users[i].Status);
            }
        }

        [Fact]
        public async Task HardRemoveAsync_PredicateReturnEmptyOrNull_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var users = new List<User>() { new() { Id = 9999, UserName = "ex" } };

            var result = await _userService.HardRemoveAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.Equal(0, result);

            result = await _userService.HardRemoveAsync(null!).ConfigureAwait(true);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task HardRemoveAsync_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var users = await _userService.GetListAsync().ConfigureAwait(true);
            users = [.. users.Take(6).OrderBy(q => q.Id)];

            var result = await _userService.HardRemoveAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.Equal(6, result);

            var data = await _userService.GetListAsync(q => users.Select(q => q.Id).Contains(q.Id)).ConfigureAwait(true);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetListAsync_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var result = (await _userService.GetListAsync().ConfigureAwait(true)).Select(q => new TestSubModel()
            {
                Id = q.Id,
                CardNum = q.UserName,
                ExpiresIn = q.CreatedTime
            }).ToList();

            Assert.Equal(10, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                var user = await _userService.GetOneAsync(q => q.Id.Equals(result[i].Id)).ConfigureAwait(true);
                Assert.Equal(result[i].CardNum, user!.UserName);
                Assert.Equal(result[i].ExpiresIn, user.UpdatedTime);
            }
        }

        [Fact]
        public async Task GetListAsync_WithPredicate_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var result = (await _userService.GetListAsync(q => true).ConfigureAwait(true)).Select(q => new TestSubModel() { Id = q.Id, CardNum = q.UserName, ExpiresIn = q.CreatedTime }).ToList();

            Assert.Equal(10, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                var user = await _userService.GetOneAsync(q => q.Id.Equals(result[i].Id)).ConfigureAwait(true);
                Assert.Equal(result[i].CardNum, user!.UserName);
                Assert.Equal(result[i].ExpiresIn, user.CreatedTime);
            }
        }

        [Fact]
        public async Task GetSelectListAsyncAsync_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var result = await _userService.GetSelectListAsync(q => new TestSubModel()
            {
                Id = q.Id,
                CardNum = q.UserName,
                ExpiresIn = q.CreatedTime
            }).ConfigureAwait(true);

            Assert.Equal(10, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                var user = await _userService.GetOneAsync(q => q.Id.Equals(result[i].Id)).ConfigureAwait(true);
                Assert.Equal(result[i].CardNum, user!.UserName);
            }
        }

        [Fact]
        public async Task GetSelectListAsync_WithPredicate_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var result = await _userService.GetSelectListAsync(q => new TestSubModel() { Id = q.Id, CardNum = q.UserName }, q => true).ConfigureAwait(true);

            Assert.Equal(10, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                var user = await _userService.GetOneAsync(q => q.Id.Equals(result[i].Id)).ConfigureAwait(true);
                Assert.Equal(result[i].CardNum, user!.UserName);
            }
        }

        [Fact]
        public async Task GetSelectListAsync_ScalarIsNull_Correctly()
        {
            _context.Clear<User>();
            await _userService.AddAsync(new EFUserFaker().Generate(10)).ConfigureAwait(true);

            var result = await _userService.GetSelectListAsync<TestSubModel>(null!).ConfigureAwait(true);

            Assert.Empty(result);
        }
    }
}
#endif