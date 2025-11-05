#if NET8_0_OR_GREATER
using JfYu.Office.Excel;
using JfYu.Office.Excel.Extensions;
using JfYu.UnitTests.Models;
using Microsoft.VisualBasic;
using Moq;
using NPOI.SS.UserModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelReadTests(IJfYuExcel jfYuExcel)
    {
        private readonly IJfYuExcel _jfYuExcel = jfYuExcel;

        #region Read

        [Fact]
        public void Read_FileNotExist_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<List<AllTypeTestModel>>(filePath));
            Assert.IsAssignableFrom<FileNotFoundException>(ex);
        }

        [Fact]
        public void Read_WrongT_ThrowException()
        {
            var filePath = $"{nameof(Read_WrongT_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel>(filePath));
            Assert.IsAssignableFrom<InvalidOperationException>(ex);
            File.Delete(filePath);
        }

        [Fact]
        public void Read_WrongGenericTypeDefinition_ThrowException()
        {
            var filePath = $"{nameof(Read_WrongGenericTypeDefinition_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ex = Record.Exception(() => _jfYuExcel.Read<Dictionary<string, AllTypeTestModel>>(filePath));
            Assert.IsAssignableFrom<InvalidOperationException>(ex);
            File.Delete(filePath);
        }

        [Fact]
        public void Read_EmptyFile_ReturnEmptyCollection()
        {
            var filePath = $"{nameof(Read_EmptyFile_ReturnEmptyCollection)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act
            var wb = _jfYuExcel.CreateExcel();
            wb.CreateSheet();
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();
            // Assert
            var data = _jfYuExcel.Read<List<AllTypeTestModel>>(filePath);

            Assert.True(data.Count == 0);
            File.Delete(filePath);
        }

        [Fact]
        public void Read_OnlyHaveHeader_ReturnEmptyCollection()
        {
            var filePath = $"{nameof(Read_OnlyHaveHeader_ReturnEmptyCollection)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);
            // Assert
            var data = _jfYuExcel.Read<List<AllTypeTestModel>>(filePath);

            Assert.True(data.Count == 0);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_ConvertFailed_ThrowException()
        {
            var filePath = $"{nameof(Read_ConvertFailed_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<string>() { "2", "1Xa1", "3" };
            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);

            // Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<List<int>>(filePath));
            Assert.IsAssignableFrom<FormatException>(ex);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithTypeTupleMoreThan8_ThrowException()
        {
            var filePath = $"{nameof(Read_WithTypeTupleMoreThan8_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            _jfYuExcel.UpdateOption(q => q.SheetMaxRecord = 100);
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, Tuple<List<TestModel>, List<TestModel>>>(d1, d2, d2, d2, d2, d2, d2, new Tuple<List<TestModel>, List<TestModel>>(d2, d2));
            // Act
            _jfYuExcel.Write(source, filePath);

            var ex = Record.Exception(() => _jfYuExcel.Read<Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, Tuple<List<TestModel>, List<TestModel>>>>(filePath));

            Assert.IsAssignableFrom<InvalidOperationException>(ex);
            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithInvalidCast_ThrowException()
        {
            var filePath = $"{nameof(Read_WithInvalidCast_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            var source = new List<string>() { null!, "xxx" };
            // Act
            var workbook = _jfYuExcel.Write(source, filePath, new Dictionary<string, string>() { { "Capacity", "Capacity" } });
            // Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<List<List<int>>>(filePath));
            Assert.IsAssignableFrom<InvalidCastException>(ex);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithUnsupportSimpleType_ThrowException()
        {
            var filePath = $"{nameof(Read_WithUnsupportSimpleType_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            var source = new List<string>() { null!, "xxx" };
            // Act
            var workbook = _jfYuExcel.Write(source, filePath, new Dictionary<string, string>() { { "Capacity", "Capacity" } });
            // Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<List<List<int>>>(filePath));
            Assert.IsAssignableFrom<InvalidCastException>(ex);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithUnknownCellType_ThrowException()
        {
            var filePath = $"{nameof(Read_WithUnknownCellType_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act

            var wb = JfYuExcelExtension.CreateExcel();
            var sheet = wb.CreateSheet();
            var row = sheet.CreateRow(0);
            var cell = row.CreateCell(0);
            cell.SetCellValue("A");//title
            row = sheet.CreateRow(1);
            cell = row.CreateCell(0);
            cell.SetCellErrorValue(0);
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();

            // Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<List<string>>(filePath));
            Assert.IsAssignableFrom<Exception>(ex);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithWrongFormulaCellType_ThrowException()
        {
            // Arrange
            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();
            // Act
            mockCell.SetupGet(c => c.CellType).Returns(CellType.Formula);
            mockCell.SetupGet(c => c.CachedFormulaResultType).Returns(CellType.Error);
            mockCell.SetupGet(c => c.ErrorCellValue).Returns(FormulaError.DIV0.Code);
            mockCell.SetupGet(c => c.CellFormula).Returns("1/0");
            // Assert
            var exception = Record.Exception(() => JfYuExcelExtension.ConvertCellValue(typeof(string), mockCell.Object));
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public void Read_WithNumericFormulaCellType_ThrowException()
        {
            var filePath = $"{nameof(Read_WithNumericFormulaCellType_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act

            var wb = JfYuExcelExtension.CreateExcel();
            var sheet = wb.CreateSheet();
            var row = sheet.CreateRow(0);
            var cell = row.CreateCell(0);
            cell.SetCellValue("A");//title

            IRow row1 = sheet.CreateRow(1);
            ICell cell1 = row1.CreateCell(0);
            cell1.SetCellFormula("12121+2131");
            wb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();

            // Assert
            var data = _jfYuExcel.Read<List<int>>(filePath);
            Assert.Equal(12121 + 2131, data.First());
            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithStringFormulaCellType_ThrowException()
        {
            // Arrange
            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();
            // Act
            mockCell.SetupGet(c => c.CellType).Returns(CellType.Formula);
            mockCell.SetupGet(c => c.CachedFormulaResultType).Returns(CellType.String); ;
            mockCell.SetupGet(c => c.StringCellValue).Returns("Hello World");

            // Assert
            var data = JfYuExcelExtension.ConvertCellValue(typeof(string), mockCell.Object);
            Assert.Equal("Hello World", data);
        }

        [Fact]
        public void Read_NullDateFormats_ReturnCorrectly()
        {
            // Arrange
            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();
            var mockCellStyle = new Mock<ICellStyle>();
            DateTime? dateTime = null;
            // Act
            mockCellStyle.SetupGet(c => c.DataFormat).Returns(164);
            mockCellStyle.Setup(c => c.GetDataFormatString()).Returns("dd/MM/yyyy");
            mockCell.SetupGet(c => c.CellType).Returns(CellType.Numeric);
            mockCell.SetupGet(c => c.DateCellValue).Returns(dateTime);
            mockCell.SetupGet(c => c.NumericCellValue).Returns(44927);
            mockCell.SetupGet(c => c.CellStyle).Returns(mockCellStyle.Object);

            // Assert
            var data = JfYuExcelExtension.ConvertCellValue(typeof(string), mockCell.Object);
            Assert.Null(data);
        }
        [Fact]
        public void Read_NullCell_ReturnCorrectly()
        {
            // Act 
            var data = JfYuExcelExtension.ConvertCellValue(typeof(string), null!);
            // Assert
            Assert.Null(data);
        }
        [Theory]
        [InlineData("2023-01-01", "yyyy-MM-dd")]
        [InlineData("01/01/2023", "dd/MM/yyyy")]
        public void Read_DifferentDateFormats_ReturnCorrectly(string dateString, string formatString)
        {
            // Arrange
            DateTime expectedDate = DateTime.ParseExact(dateString, formatString, null);
            var filePath = $"{nameof(Read_DifferentDateFormats_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var wb = JfYuExcelExtension.CreateExcel();
            var sheet = wb.CreateSheet();
            var row = sheet.CreateRow(0);
            var cell = row.CreateCell(0);
            cell.SetCellValue("A");//title
            row = sheet.CreateRow(1);
            cell = row.CreateCell(0);
            ICellStyle cellStyle = wb.CreateCellStyle();
            IDataFormat dataFormat = wb.CreateDataFormat();
            cellStyle.DataFormat = dataFormat.GetFormat(formatString);
            cell.CellStyle = cellStyle;
            cell.SetCellValue(expectedDate);
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();
            // Act
            var data = _jfYuExcel.Read<List<DateTime>>(filePath);

            // Assert
            Assert.Equal(expectedDate.Date, data[0].Date);
            File.Delete(filePath);
        }

        #endregion Read

        #region ReadSteam

        [Fact]
        public void ReadStream_FileNotExist_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<List<AllTypeTestModel>>(ms!));
            Assert.IsAssignableFrom<ArgumentNullException>(ex);
        }

        [Fact]
        public void ReadStream_WrongT_ThrowException()
        {
            var filePath = $"{nameof(ReadStream_WrongT_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);

            var ms = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel>(ms));
            Assert.IsAssignableFrom<InvalidOperationException>(ex);
            ms.Dispose();
            File.Delete(filePath);
        }

        [Fact]
        public void ReadStream_WrongGenericTypeDefinition_ThrowException()
        {
            var filePath = $"{nameof(ReadStream_WrongGenericTypeDefinition_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();

            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);

            var ms = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var ex = Record.Exception(() => _jfYuExcel.Read<Dictionary<string, AllTypeTestModel>>(ms));
            Assert.IsAssignableFrom<InvalidOperationException>(ex);
            ms.Dispose();
            File.Delete(filePath);
        }

        [Fact]
        public void ReadStream_EmptyFile_ThrowException()
        {
            var filePath = $"{nameof(ReadStream_EmptyFile_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act
            var wb = _jfYuExcel.CreateExcel();
            wb.CreateSheet();
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();
            var ms = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Assert
            var data = _jfYuExcel.Read<List<AllTypeTestModel>>(ms);

            Assert.True(data.Count == 0);
            File.Delete(filePath);
            ms.Dispose();
        }

        [Fact]
        public void ReadStream_OnlyHaveHeader_ThrowException()
        {
            var filePath = $"{nameof(ReadStream_OnlyHaveHeader_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            var workbook = _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ms = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Assert
            var data = _jfYuExcel.Read<List<AllTypeTestModel>>(ms);

            Assert.True(data.Count == 0);

            File.Delete(filePath);
            ms.Dispose();
        }

        #endregion ReadSteam
    }
}
#endif