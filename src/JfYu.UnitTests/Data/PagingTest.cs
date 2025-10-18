#if NET8_0_OR_GREATER
using JfYu.Data.Extension;
using JfYu.UnitTests.Models;
using JfYu.UnitTests.Models.Entity;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.DependencyInjection;

namespace JfYu.UnitTests.Data
{
    [Collection("Data")]
    public class PagingTest
    {
        private readonly DataContext _context;

        public PagingTest()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddDataContextServices().BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<DataContext>();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task NullSource_ThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await PagingExtensions.ToPagedAsync<TestModel>(null!).ConfigureAwait(true)).ConfigureAwait(true);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await PagingExtensions.ToPagedAsync<TestModel, TestSubModel>(null!, q => { return []; }).ConfigureAwait(true)).ConfigureAwait(true);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await PagingExtensions.ToPagedAsync<TestModel, TestSubModel>(new List<TestModel>().AsQueryable(), null!).ConfigureAwait(true)).ConfigureAwait(true);

            Assert.Throws<ArgumentNullException>(() => PagingExtensions.ToPaged<TestModel>(null!));
            Assert.Throws<ArgumentNullException>(() => PagingExtensions.ToPaged<TestModel, TestSubModel>(null!, q => { return []; }));
            Assert.Throws<ArgumentNullException>(() => PagingExtensions.ToPaged<TestModel, TestSubModel>(new List<TestModel>().AsQueryable(), null!));
        }

        [Fact]
        public async Task PageIndexLessThan0_ThrowException()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await PagingExtensions.ToPagedAsync(new List<TestModel>().AsQueryable(), -1).ConfigureAwait(true)).ConfigureAwait(true);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await PagingExtensions.ToPagedAsync(new List<TestModel>().AsQueryable(), q => { return new List<TestSubModel>(); }, -1).ConfigureAwait(true)).ConfigureAwait(true);

            Assert.Throws<ArgumentOutOfRangeException>(() => PagingExtensions.ToPaged(new List<TestModel>().AsQueryable(), -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => PagingExtensions.ToPaged(new List<TestModel>().AsQueryable(), q => { return new List<TestSubModel>(); }, -1));
        }

        [Fact]
        public async Task PageSizeLessThan0_ThrowException()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await PagingExtensions.ToPagedAsync(new List<TestModel>().AsQueryable(), 1, -1).ConfigureAwait(true)).ConfigureAwait(true);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await PagingExtensions.ToPagedAsync(new List<TestModel>().AsQueryable(), q => { return new List<TestSubModel>(); }, 1, -1).ConfigureAwait(true)).ConfigureAwait(true);

            Assert.Throws<ArgumentOutOfRangeException>(() => PagingExtensions.ToPaged(new List<TestModel>().AsQueryable(), 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => PagingExtensions.ToPaged(new List<TestModel>().AsQueryable(), q => { return new List<TestSubModel>(); }, 1, -1));
        }

        #region ToPaged

        [Fact]
        public void ToPaged_Page1_Correctly()
        {
            var data = new TestModelFaker().Generate(8).AsQueryable();
            var result = data.ToPaged();

            Assert.Equal(8, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(8, result.Data.Count);
        }

        [Fact]
        public void ToPaged_Page2_Correctly()
        {
            var data = new TestModelFaker().Generate(8).AsQueryable();
            var result = data.ToPaged(1, 5);

            Assert.Equal(8, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public void ToPaged_Page22_Correctly()
        {
            var data = new TestModelFaker().Generate(13).AsQueryable();
            var result = data.ToPaged(2, 10);

            Assert.Equal(13, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public void ToPaged_PageConvert_Correctly()
        {
            var data = new TestModelFaker().Generate(13).AsQueryable();
            var result = data.ToPaged(q =>
            {
                var x = new List<TestSubModel>();
                foreach (var item in q)
                {
                    x.Add(new TestSubModel() { Id = item.Id, CardNum = item.Sub!.CardNum, ExpiresIn = item.Sub.ExpiresIn });
                }
                return x;
            }, 2, 10);

            Assert.Equal(13, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.Data.Count);

            foreach (var item in result.Data)
            {
                var model = data.First(q => q.Id == item.Id);
                Assert.Equal(item.CardNum, model.Sub!.CardNum);
                Assert.Equal(item.ExpiresIn, model.Sub.ExpiresIn);
            }
        }

        #endregion ToPaged

        #region ToPagedAsync

        [Fact]
        public async Task ToPagedAsync_Page1_Correctly()
        {
            _context.Clear<User>();
            _context.Users.AddRange(new EFUserFaker().Generate(8));
            await _context.SaveChangesAsync();

            var result = await _context.Users.ToPagedAsync().ConfigureAwait(true);

            Assert.Equal(8, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(8, result.Data.Count);
        }

        [Fact]
        public async Task ToPagedAsync_Page2_Correctly()
        {
            _context.Clear<User>();
            _context.Users.AddRange(new EFUserFaker().Generate(8));
            await _context.SaveChangesAsync();
            var result = await _context.Users.ToPagedAsync(1, 5).ConfigureAwait(true);

            Assert.Equal(8, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task ToPagedAsync_Page22_Correctly()
        {
            _context.Clear<User>();
            _context.Users.AddRange(new EFUserFaker().Generate(13));
            await _context.SaveChangesAsync();
            var result = await _context.Users.ToPagedAsync(2, 10).ConfigureAwait(true);

            Assert.Equal(13, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task ToPagedAsync_PageConvert_Correctly()
        {
            _context.Clear<User>();
            _context.Users.AddRange(new EFUserFaker().Generate(13));
            await _context.SaveChangesAsync();

            var result = await _context.Users.ToPagedAsync(q =>
            {
                var x = new List<TestSubModel>();
                foreach (var item in q)
                {
                    x.Add(new TestSubModel() { Id = item.Id, CardNum = item.UserName, ExpiresIn = item.CreatedTime });
                }
                return x;
            }, 2, 10).ConfigureAwait(true);

            Assert.Equal(13, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.Data.Count);

            foreach (var item in result.Data)
            {
                var model = _context.Users.First(q => q.Id == item.Id);
                Assert.Equal(item.CardNum, model.UserName);
                Assert.Equal(item.ExpiresIn, model.CreatedTime);
            }
        }
       
        #endregion ToPagedAsync
    }
}
#endif