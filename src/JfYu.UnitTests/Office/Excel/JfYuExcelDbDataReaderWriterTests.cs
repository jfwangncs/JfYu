#if NET8_0_OR_GREATER
using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.UnitTests.Models;
using JfYu.UnitTests.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Globalization;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelDbDataReaderWriterTests
    {
        public IJfYuExcel _jfYuExcel;
        public TestDbContext _db;
        private readonly string _dbPath;

        public JfYuExcelDbDataReaderWriterTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"localdatabase_{Guid.NewGuid():N}.db");

            var services = new ServiceCollection();
            services.AddJfYuExcel();
            services.AddDbContext<TestDbContext>(options =>
              options.UseSqlite($"Data Source={_dbPath}"));
            var serviceProvider = services.BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
            _db = serviceProvider.GetRequiredService<TestDbContext>();
            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();
        }

        [Fact]
        public void DbDataReaderWriter_NullSource_ThrowException()
        {
            var filePath = "DbDataReaderWriter_NullSource_ThrowException.xlsx";

            IDataReader? dataReader = null;
            var ex = Record.Exception(() => _jfYuExcel.Write(dataReader!, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>()));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_NullTitles_ThrowException()
        {
            var filePath = "DbDataReaderWriter_NullTitles_ThrowException.xlsx";
            using var command = _db.Database.GetDbConnection().CreateCommand();
            _db.Database.OpenConnection();
            command.CommandText = "SELECT * FROM sqlite_master";
            using var reader = command.ExecuteReader();
            var ex = Record.Exception(() => _jfYuExcel.Write(reader, filePath));
            Assert.IsAssignableFrom<ArgumentNullException>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_EmptyTitles_ThrowException()
        {
            var filePath = "DbDataReaderWriter_EmptyTitles_ThrowException.xlsx";
            using var command = _db.Database.GetDbConnection().CreateCommand();
            _db.Database.OpenConnection();
            command.CommandText = "SELECT * FROM sqlite_master";
            using var reader = command.ExecuteReader();
            var ex = Record.Exception(() => _jfYuExcel.Write(reader, filePath, []));
            Assert.IsAssignableFrom<ArgumentNullException>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_ClosedSource_ThrowException()
        {
            var filePath = "DbDataReaderWriter_ClosedSource_ThrowException.xlsx";
            using var command = _db.Database.GetDbConnection().CreateCommand();
            _db.Database.OpenConnection();
            command.CommandText = "SELECT * FROM sqlite_master";

            var reader = command.ExecuteReader();
            _db.Database.CloseConnection();
            var ex = Record.Exception(() => _jfYuExcel.Write(reader, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>()));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_ReturnCorrectly()
        {
            var filePath = "DbDataReaderWriter_ReturnCorrectly.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            _db.TestModels.RemoveRange(_db.TestModels);
            var source = new TestModelFaker().Generate(166);
            source.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            _db.AddRange(source);
            _db.SaveChanges();
            // Act 
            using var command = _db.Database.GetDbConnection().CreateCommand();

            _db.Database.OpenConnection();
            command.CommandText = "SELECT * FROM TestModels";
            using var reader = command.ExecuteReader();
            _jfYuExcel.Write(reader, filePath, JfYuExcelExtension.GetTitles<TestModel>(), new JfYuExcelOptions() { SheetMaxRecord = 1000 }, callback);

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.Equal(JsonConvert.SerializeObject(_db.TestModels.ToList()), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }

        [Fact]
        public void DbDataReaderWriter_WithMultipleSheet_ReturnCorrectly()
        {
            var filePath = "DbDataReaderWriter_WithMultipleSheet_ReturnCorrectly.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new TestModelFaker().Generate(18);
            _db.TestModels.RemoveRange(_db.TestModels);
            source.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            _db.AddRange(source);
            _db.SaveChanges();
            // Act
            using var command = _db.Database.GetDbConnection().CreateCommand();

            _db.Database.OpenConnection();
            command.CommandText = "SELECT * FROM TestModels";
            using var reader = command.ExecuteReader();
            _jfYuExcel.Write(reader, filePath, JfYuExcelExtension.GetTitles<TestModel>(), new JfYuExcelOptions() { SheetMaxRecord = 10 });

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            data!.AddRange(_jfYuExcel.Read<TestModel>(filePath, 1, 1)!);
            Assert.Equal(JsonConvert.SerializeObject(_db.TestModels.ToList()), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }      
    }
}
#endif