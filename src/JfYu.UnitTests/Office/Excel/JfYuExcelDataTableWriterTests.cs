#if NET8_0_OR_GREATER 
using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelDataTableWriterTests
    {
        private readonly IJfYuExcel _jfYuExcel;

        public JfYuExcelDataTableWriterTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddJfYuExcel().BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
        }
        [Fact]
        public void DataTableExcelWriter_ValidDataTableWithWriteOperationNone_CreatesWorkbookWithCorrectData()
        {
            var filePath = "ValidDataTableWithWriteOperationNone.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            var titles = new Dictionary<string, string> { { "Id", "ID" }, { "Name", "姓名" }, { "Age", "年龄" }, { "DateTime", "DateTime" } };
            var writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.None };
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            // Act
            _jfYuExcel.Write(source, filePath, titles, writeOperation, callback);

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            Assert.Equal(callbackCalledCount, source.Rows.Count);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTableButFileExistWithWriteOperationNone_ThrowException()
        {
            var filePath = "ValidDataTableButFileExistWithWriteOperationNone.xlsx";
            var fs = File.Create(filePath);
            fs.Dispose();
            // Arrange
            using var source = CreateSampleDataTable();
            var titles = new Dictionary<string, string> { { "Id", "ID" }, { "Name", "姓名" } };
            var writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.None };
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            // Act & Assert
            var ex = Record.Exception(() => _jfYuExcel.Write(source, filePath, titles, writeOperation, callback));
            Assert.IsAssignableFrom<Exception>(ex);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTableWithWriteOperationAppend_CreatesWorkbookWithCorrectData()
        {
            var filePath = "ValidDataTableWithWriteOperationAppend.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            var titles = new Dictionary<string, string> { { "Id", "ID" }, { "Name", "姓名" }, { "Age", "年龄" }, { "DateTime", "DateTime" } };
            var writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.None };
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            // Act
            _jfYuExcel.Write(source, filePath, titles, writeOperation, callback);

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            Assert.Equal(callbackCalledCount, source.Rows.Count);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTableButFileExistWithWriteOperationAppend_CreatesWorkbookWithCorrectData()
        {
            var filePath = "ValidDataTableButFileExistWithWriteOperationAppend.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            var titles = new Dictionary<string, string> { { "Id", "ID" }, { "Name", "姓名" }, { "Age", "年龄" }, { "DateTime", "DateTime" } };
            var writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.None };
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            // Act
            _jfYuExcel.Write(source, filePath, titles, writeOperation, callback);
            writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.Append };
            using var source1 = CreateSampleDataTable("table2");
            _jfYuExcel.Write(source1, filePath, titles, writeOperation, callback);

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);

            // Assert
            data = _jfYuExcel.Read<TestModel>(filePath, 1, 1);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTableWithoutTitles_CreatesWorkbookWithCorrectData()
        {
            var filePath = "ValidDataTableWithoutTitles.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            var writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.None };
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            // Act
            _jfYuExcel.Write(source, filePath, null, writeOperation, callback);

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTableWithoutCallBack_CreatesWorkbookWithCorrectData()
        {
            var filePath = "ValidDataTableWithoutCallBack.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            var writeOperation = new JfYuExcelOptions() { AllowAppend = WriteOperation.None };
            // Act
            _jfYuExcel.Write(source, filePath, null, writeOperation, null);
            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTableWithoutWriteOperation_CreatesWorkbookWithCorrectData()
        {
            var filePath = "ValidDataTableWithoutWriteOperation.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTable_CreatesWorkbookWithXls()
        {
            var filePath = "ValidDataTable.xls";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTable_CreatesWorkbookWithDict()
        {
            var filePath = "excel1/ValidDataTable.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            if (File.Exists(filePath))
                File.Delete(filePath);
            Directory.Delete("excel1");
        }

        [Fact]
        public void DataTableExcelWriter_ValidDataTable_CreatesWorkbookWithExistDict()
        {
            var filePath = "excel/ValidDataTableExistDict.xlsx";
            // Arrange
            if (!Directory.Exists("excel"))
                Directory.CreateDirectory("excel");
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable();
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            if (File.Exists(filePath))
                File.Delete(filePath);
            Directory.Delete("excel");
        }

        [Fact]
        public void DataTableExcelWriter_EmptyTableNameDataTable_CreatesWorkbookWithCorrectData()
        {
            var filePath = "EmptyTableNameDataTable.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var source = CreateSampleDataTable("");
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_DataTableWithType_CreatesWorkbookWithCorrectData()
        {
            var filePath = "DataTableWithNullData.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var table = new DataTable();
            table.Columns.Add("Short", typeof(short));
            table.Columns.Add("Ushort", typeof(ushort));
            table.Columns.Add("Int", typeof(int));
            table.Columns.Add("Uint", typeof(uint));
            table.Columns.Add("Long", typeof(long));
            table.Columns.Add("Ulong", typeof(ulong));
            table.Columns.Add("Float", typeof(float));
            table.Columns.Add("Double", typeof(double));
            table.Columns.Add("Decimal", typeof(decimal));
            table.Columns.Add("Bool", typeof(bool));
            table.Columns.Add("String", typeof(string));
            table.Columns.Add("DateTime", typeof(DateTime));
            table.Columns.Add("Byte", typeof(byte));
            table.Columns.Add("Sbyte", typeof(sbyte));
            table.Columns.Add("EmptyStr", typeof(string));
            table.Columns.Add("NullStr", typeof(string));
            for (int i = 0; i < 5; i++)
            {
                table.Rows.Add(i + 1, i + 1, i + 1, i + 1, i + 1, i + 1, i + 0.123456F, i + 0.123456D, i + 123456789.123456789M, i % 2 != 0, $"str{i}", DateTime.Parse("2025-01-01 10:01:02", CultureInfo.InvariantCulture).AddHours(i), i + 100, i + 50, "", null);
            }

            // Act
            _jfYuExcel.Write(table, filePath);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(5, data.Count);
            foreach (DataRow item in table.Rows)
            {
                var index = (int)item["Int"] - 1;
                Assert.Equal(item["Short"], data[index].Short);
                Assert.Equal(item["Ushort"], data[index].Ushort);
                Assert.Equal(item["Int"], data[index].Int);
                Assert.Equal(item["Uint"], data[index].Uint);
                Assert.Equal(item["Long"], data[index].Long);
                Assert.Equal(item["Ulong"], data[index].Ulong);
                Assert.Equal(item["Float"], data[index].Float);
                Assert.Equal(item["Double"], data[index].Double);
                Assert.Equal(item["Decimal"], data[index].Decimal);
                Assert.Equal(item["Bool"], data[index].Bool);
                Assert.Equal(item["String"], data[index].String);
                Assert.Equal(item["DateTime"], data[index].DateTime);
                Assert.Equal(item["byte"], data[index].Byte);
                Assert.Equal(item["sbyte"], data[index].Sbyte);
                Assert.Equal(item["EmptyStr"], data[index].EmptyStr);
                Assert.Null(data[index].NullStr);
            }
            File.Delete(filePath);
        }

        [Fact]
        public void DataTableExcelWriter_DataTableWithMultipleSheet_CreatesWorkbookWithCorrectData()
        {
            var filePath = "DataTableWithMultipleSheet.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            table.Columns.Add("DateTime", typeof(DateTime));
            table.Columns.Add("NullString", typeof(string));
            table.Columns.Add("EmptyString", typeof(string));

            for (int i = 1; i <= 26; i++)
            {
                table.Rows.Add(i, $"Name{i}", i + 10, DateTime.Now, null, string.Empty);
            }

            // Act
            _jfYuExcel.Write(table, filePath, excelOption: new JfYuExcelOptions() {  SheetMaxRecord = 10 });

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(10, data.Count);

            data = _jfYuExcel.Read<TestModel>(filePath, 1, 1);
            Assert.NotNull(data);
            Assert.Equal(10, data.Count);

            data = _jfYuExcel.Read<TestModel>(filePath, 1, 2);
            Assert.NotNull(data);
            Assert.Equal(6, data.Count);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private static DataTable CreateSampleDataTable(string tableName = "Sheet1")
        {
            var table = new DataTable
            {
                TableName = tableName
            };
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            table.Columns.Add("DateTime", typeof(DateTime));

            for (int i = 1; i <= 5; i++)
            {
                table.Rows.Add(i, $"Name{i}", i + 10, DateTime.Now);
            }

            return table;
        }
    }
}
#endif