using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Extensions;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using NPOI.SS.Formula.Functions;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelCSVTests
    {
        private readonly IJfYuExcel _jfYuExcel;

        public JfYuExcelCSVTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddJfYuExcel().BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
        }

        [Fact]
        public void WriteCSV_SourceIsNull_ThrowException()
        {
            var filePath = $"{nameof(WriteCSV_SourceIsNull_ThrowException)}.csv";
            List<AllTypeTestModel>? source = null;
            var ex = Record.Exception(() => _jfYuExcel.WriteCSV(source!, filePath)); 
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("!@#$%^&*()_+")]
        public void WriteCSV_PathIsInvalid_ThrowException(string? path)
        {
            var source = new List<AllTypeTestModel>();
            var ex = Record.Exception(() => _jfYuExcel.Write(source, path!)); 
            Assert.IsType<Exception>(ex,false);
        }

        [Fact]
        public void WriteCSV_FileExist_ThrowException()
        {
            var filePath = $"{nameof(WriteCSV_FileExist_ThrowException)}.csv";
            if (!File.Exists(filePath))
            {
                var s = File.Create(filePath);
                s.Dispose();
            }
            var source = new List<AllTypeTestModel>();
            var ex = Record.Exception(() => _jfYuExcel.WriteCSV(source, filePath));
            Assert.IsType<FileNotFoundException>(ex);
            File.Delete(filePath);
        }

        [Fact]
        public void WriteCSV_FilePathNoExt_ReturnCorrectly()
        {
            var filePath = $"{nameof(WriteCSV_FilePathNoExt_ReturnCorrectly)}";
            // Arrange
            if (File.Exists(filePath + ".csv"))
                File.Delete(filePath + ".csv");
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.WriteCSV(source, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>());

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath + ".csv");

            Assert.Equal(source.Count, data.Count);
            File.Delete(filePath + ".csv");
        }

        [Fact]
        public void WriteCSV_WithCallback_ReturnCorrectly()
        {
            var filePath = $"{nameof(WriteCSV_WithCallback_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var callbackCalledCount = 0;
            Action<int> callback = (count) => callbackCalledCount++;

            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.WriteCSV(source, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>(), callback);

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath);

            Assert.Equal(source.Count, data.Count);

            Assert.Equal(callbackCalledCount, data.Count);
            File.Delete(filePath);
        }

        [Fact]
        public void WriteCSV_WithTitles_ReturnCorrectly()
        {
            var filePath = $"{nameof(WriteCSV_WithTitles_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.WriteCSV(source, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>());

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath);

            Assert.Equal(source.Count, data.Count);
            File.Delete(filePath);
        }

        [Fact]
        public void WriteCSV_WithoutTitles_ReturnCorrectly()
        {
            var filePath = $"{nameof(WriteCSV_WithoutTitles_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.WriteCSV(source, filePath);

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath);

            Assert.Equal(source.Count, data.Count);
            File.Delete(filePath);
        }

        [Fact]
        public void WriteCSV_WithEmptyTitles_ReturnCorrectly()
        {
            var filePath = $"{nameof(WriteCSV_WithEmptyTitles_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.WriteCSV(source, filePath, []);

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath);

            Assert.Equal(source.Count, data.Count);
            File.Delete(filePath);
        }

        [Fact]
        public void ReadCSV_FileNotFound_ThrowException()
        {
            var filePath = $"{nameof(ReadCSV_FileNotFound_ThrowException)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var ex = Record.Exception(() => _jfYuExcel.ReadCSV(filePath));
            Assert.IsType<FileNotFoundException>(ex);
        }

        [Fact]
        public void ReadCSV_SetupFirstRow_ReturnCorrectly()
        {
            var filePath = $"{nameof(ReadCSV_SetupFirstRow_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new TestModelFaker().Generate(26);
            // Act
            _jfYuExcel.WriteCSV(source, filePath);

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath, 10);

            Assert.Equal(source.Count - 10 + 2 - 1, data.Count);
            File.Delete(filePath);
        }

        [Fact]
        public void ReadCSV_SomeRowFiledsMoreThanHeader_ReturnCorrectly()
        {
            var filePath = $"{nameof(ReadCSV_SomeRowFiledsMoreThanHeader_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 写入 header（单列）
                writer.WriteLine("HeaderColumn");

                // 写入第二行（5列数据）
                writer.WriteLine(string.Join(",", new List<string> { "Data1", "Data2", "Data3", "Data4", "Data5" }));

                // 写入第三行（8列数据）
                writer.WriteLine(string.Join(",", new List<string> { "Data6", "Data7", "Data8", "Data9", "Data10", "Data11", "Data12", "Data13" }));
            }

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath);

            var d1 = data[0] as IDictionary<string, object>;
            var d2 = data[1] as IDictionary<string, object>;
            Assert.Equal(2, data.Count);
            Assert.Equal(5, d1!.Count);
            Assert.Equal(8, d2!.Count);
            File.Delete(filePath);
        }

        [Fact]
        public void ReadCSV_EmptySource_ReturnCorrectly()
        {
            var filePath = $"{nameof(ReadCSV_EmptySource_ReturnCorrectly)}.csv";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<TestModelFaker>();
            // Act
            _jfYuExcel.WriteCSV(source, filePath);

            // Assert
            var data = _jfYuExcel.ReadCSV(filePath, 10);

            Assert.Equal(source.Count, data.Count);
            File.Delete(filePath);
        }
    }
}