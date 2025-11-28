using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.UnitTests.Models;
using JfYu.UnitTests.Models.Entity;
#if NET8_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Globalization;
using NPOI.OpenXmlFormats.Spreadsheet;

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
#if NET8_0_OR_GREATER
            services.AddDbContext<TestDbContext>(options =>
              options.UseSqlite($"Data Source={_dbPath}"));
            var serviceProvider = services.BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
            _db = serviceProvider.GetRequiredService<TestDbContext>();
            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();
#else
            var serviceProvider = services.BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
            Database.SetInitializer(new CreateDatabaseIfNotExists<TestDbContext>());
            _db = new TestDbContext($"Data Source={_dbPath};Version=3;");
            _db.Database.Connection.Open();
            _db.Database.ExecuteSqlCommand(@"
CREATE TABLE IF NOT EXISTS TestModels (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT,
        Age INTEGER NOT NULL,
        Address TEXT,
        DateTime TEXT,
       Sub_Id INTEGER
    );

 CREATE TABLE IF NOT EXISTS TestSubModels (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        CardNum TEXT NOT NULL,
        ExpiresIn TEXT,
        TestModelId INTEGER,
        FOREIGN KEY (TestModelId) REFERENCES TestModels(Id)
    );
");
            _db.Database.Connection.Close();
#endif
        }

        [Fact]
        public void DbDataReaderWriter_NullSource_ThrowException()
        {
            var filePath = "DbDataReaderWriter_NullSource_ThrowException.xlsx";

            IDataReader? dataReader = null;
            var ex = Record.Exception(() => _jfYuExcel.Write(dataReader!, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>()));
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_NullTitles_ThrowException()
        {
            var filePath = "DbDataReaderWriter_NullTitles_ThrowException.xlsx";
#if NET8_0_OR_GREATER
            using var command = _db.Database.GetDbConnection().CreateCommand();
            _db.Database.OpenConnection();
#else
            using var command = _db.Database.Connection.CreateCommand();
            _db.Database.Connection.Open();
#endif
            command.CommandText = "SELECT * FROM sqlite_master";
            using var reader = command.ExecuteReader();
            var ex = Record.Exception(() => _jfYuExcel.Write(reader, filePath));
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_EmptyTitles_ThrowException()
        {
            var filePath = "DbDataReaderWriter_EmptyTitles_ThrowException.xlsx";
#if NET8_0_OR_GREATER
            using var command = _db.Database.GetDbConnection().CreateCommand();
            _db.Database.OpenConnection();
#else
            using var command = _db.Database.Connection.CreateCommand();
            _db.Database.Connection.Open();
#endif
            command.CommandText = "SELECT * FROM sqlite_master";
            using var reader = command.ExecuteReader();
            var ex = Record.Exception(() => _jfYuExcel.Write(reader, filePath, []));
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void DbDataReaderWriter_ClosedSource_ThrowException()
        {
            var filePath = "DbDataReaderWriter_ClosedSource_ThrowException.xlsx";
#if NET8_0_OR_GREATER
            using var command = _db.Database.GetDbConnection().CreateCommand();
            _db.Database.OpenConnection();
#else
            using var command = _db.Database.Connection.CreateCommand();
            _db.Database.Connection.Open();
#endif
            command.CommandText = "SELECT * FROM sqlite_master";

            var reader = command.ExecuteReader();
#if NET8_0_OR_GREATER
            _db.Database.CloseConnection();
#else
            _db.Database.Connection.Close();
#endif
            var ex = Record.Exception(() => _jfYuExcel.Write(reader, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>()));

#if NET8_0_OR_GREATER
             Assert.IsType<DataException> (ex);
#else
            Assert.IsType<InvalidOperationException>(ex);
#endif
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
#if NET8_0_OR_GREATER
            _db.AddRange(source);
#else
            _db.TestModels.AddRange(source);
#endif
            _db.SaveChanges();
            // Act 
#if NET8_0_OR_GREATER
            using var command = _db.Database.GetDbConnection().CreateCommand();

            _db.Database.OpenConnection();
#else
            using var command = _db.Database.Connection.CreateCommand();

            _db.Database.Connection.Open();
#endif
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
#if NET8_0_OR_GREATER
            _db.AddRange(source);
#else
            _db.TestModels.AddRange(source);
#endif
            _db.SaveChanges();
            // Act
#if NET8_0_OR_GREATER
            using var command = _db.Database.GetDbConnection().CreateCommand();

            _db.Database.OpenConnection();
#else
            using var command = _db.Database.Connection.CreateCommand();

            _db.Database.Connection.Open();
#endif
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