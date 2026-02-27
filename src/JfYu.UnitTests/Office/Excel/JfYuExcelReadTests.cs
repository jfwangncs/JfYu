using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Moq;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System.Globalization;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelReadTests
    {
        private readonly IJfYuExcel _jfYuExcel;

        public JfYuExcelReadTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddJfYuExcel().BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
        }

        #region ReadWrongTypeThrowException

        [Fact]
        public void Read_FileNotExist_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
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
            _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ex = Record.Exception(() => _jfYuExcel.Read<Dictionary<string, AllTypeTestModel>>(filePath));
            Assert.IsType<NotSupportedException>(ex, exactMatch: false);
            File.Delete(filePath);
        }

        [Fact]
        public void Read_WrongTypeString_ThrowException()
        {
            var filePath = $"{nameof(Read_WrongTypeString_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ex = Record.Exception(() => _jfYuExcel.Read<string>(filePath));
            Assert.IsType<NotSupportedException>(ex, exactMatch: false);
            File.Delete(filePath);
        }
        [Fact]
        public void Read_WrongTypeEnum_ThrowException()
        {
            var filePath = $"{nameof(Read_WrongTypeEnum_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ex = Record.Exception(() => _jfYuExcel.Read<Enum>(filePath));
            Assert.IsType<NotSupportedException>(ex, exactMatch: false);
            File.Delete(filePath);
        }
        [Fact]
        public void Read_WrongTypeList_ThrowException()
        {
            var filePath = $"{nameof(Read_WrongTypeList_ThrowException)}.xlsx";

            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ex = Record.Exception(() => _jfYuExcel.Read<List<string>>(filePath));
            Assert.IsType<NotSupportedException>(ex, exactMatch: false);
            File.Delete(filePath);
        }

        #endregion

        #region ConvertWrongTypeThrowException

        [Fact]
        public void Read_ConvertFailed_ThrowException()
        {
            var filePath = $"{nameof(Read_ConvertFailed_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<string>() { "2", "1Xa1", "3" };
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);

            // Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<SimpleModel<int>>(filePath));
            Assert.IsType<FormatException>(ex, exactMatch: false);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_ConvertBlankToInt_ThrowException()
        {
            var filePath = nameof(Read_ConvertBlankToInt_ThrowException) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("A");
            // data row at 1
            var data1 = sheet.CreateRow(1);
            data1.CreateCell(0).SetCellValue("1");
            // data row at 2 - blank row    
            var data2 = sheet.CreateRow(2);
            data2.CreateCell(0).SetBlank();

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(fs);
            wb.Close();

            var ex = Record.Exception(() => _jfYuExcel.Read<SimpleModel<int>>(filePath));
            Assert.IsType<Exception>(ex, exactMatch: false);

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

            using var wb = JfYuExcelExtension.CreateExcel();
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
            Assert.IsType<Exception>(ex, exactMatch: false);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithWrongFormulaCellType_ThrowException()
        {
            // Arrange 
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
        [Theory]
        [InlineData(CellType.Error)]
        [InlineData(CellType.Unknown)]
        public void Read_WithWrongCellType_ThrowException(CellType type)
        {
            // Arrange 
            var mockCell = new Mock<ICell>();
            // Act
            mockCell.SetupGet(c => c.CellType).Returns(type);
            // Assert
            var exception = Record.Exception(() => JfYuExcelExtension.ConvertCellValue(typeof(string), mockCell.Object));
            Assert.IsType<ArgumentException>(exception);
        }
        [Fact]
        public void Read_GuidConvertWrong_ThrowException()
        {
            var filePath = nameof(Read_GuidConvertWrong_ThrowException) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("A");
            // data row at 1
            var data1 = sheet.CreateRow(1);
            data1.CreateCell(0).SetCellValue("x");

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(fs);
            wb.Close();

            var exception = Record.Exception(() => _jfYuExcel.Read<SimpleModel<Guid>>(filePath));
            Assert.IsType<FormatException>(exception);

            File.Delete(filePath);
        }
        #endregion

        #region ReadCorrectly
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
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Empty(data);
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
            _jfYuExcel.Write(source.AsQueryable(), filePath);
            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Empty(data);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithNumericFormulaCellType_Correctly()
        {
            var filePath = $"{nameof(Read_WithNumericFormulaCellType_Correctly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act

            using var wb = JfYuExcelExtension.CreateExcel();
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
            var data = _jfYuExcel.Read<SimpleModel<int>>(filePath);
            Assert.Equal(12121 + 2131, data.FirstOrDefault()?.Value);
            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithStringFormulaCellType_Correctly()
        {
            // Arrange 
            var mockCell = new Mock<ICell>();
            // Act
            mockCell.SetupGet(c => c.CellType).Returns(CellType.Formula);
            mockCell.SetupGet(c => c.CachedFormulaResultType).Returns(CellType.String);
            mockCell.SetupGet(c => c.StringCellValue).Returns("Hello World");

            // Assert
            var data = JfYuExcelExtension.ConvertCellValue(typeof(string), mockCell.Object);
            Assert.Equal("Hello World", data);
        }
        [Fact]
        public void Read_WithBoolFormulaCellType_Correctly()
        {
            // Arrange 
            var mockCell = new Mock<ICell>();
            // Act
            mockCell.SetupGet(c => c.CellType).Returns(CellType.Formula);
            mockCell.SetupGet(c => c.CachedFormulaResultType).Returns(CellType.Boolean);
            mockCell.SetupGet(c => c.StringCellValue).Returns("false");

            // Assert
            var data = (bool?)JfYuExcelExtension.ConvertCellValue(typeof(bool), mockCell.Object);
            Assert.False(data);
        }

        [Fact]
        public void Read_NullDateFormats_ReturnCorrectly()
        {
            // Arrange 
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
        [Fact]
        public void Read_ConvertBlankToNullInt_ReturnCorrectly()
        {
            var filePath = nameof(Read_ConvertBlankToNullInt_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("A");
            // data row at 1
            var data1 = sheet.CreateRow(1);
            data1.CreateCell(0).SetCellValue("1");
            // data row at 2 - blank row    
            var data2 = sheet.CreateRow(2);
            data2.CreateCell(0).SetBlank();

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(fs);
            wb.Close();
            var result = _jfYuExcel.Read<SimpleModel<int?>>(filePath);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Value);
            Assert.Null(result[1].Value);

            File.Delete(filePath);
        }

        [Fact]
        public void Read_Guid_ReturnCorrectly()
        {
            var filePath = nameof(Read_Guid_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("A");
            // data row at 1
            var data1 = sheet.CreateRow(1);
            var guid = Guid.NewGuid().ToString();
            data1.CreateCell(0).SetCellValue(guid);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(fs);
            wb.Close();
            var result = _jfYuExcel.Read<SimpleModel<Guid>>(filePath);
            Assert.NotNull(result);
            Assert.Equal(guid.ToString(), result[0].Value.ToString());

            File.Delete(filePath);
        }

        [Fact]
        public void Read_String_ReturnCorrectly()
        {
            var filePath = nameof(Read_String_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("A");
            // data row at 1
            var data1 = sheet.CreateRow(1);
            data1.CreateCell(0).SetCellValue("false");
            data1.CreateCell(0).SetCellType(CellType.Boolean);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(fs);
            wb.Close();
            var result = _jfYuExcel.Read<SimpleModel<string>>(filePath);
            Assert.NotNull(result);
            Assert.False(bool.Parse(result[0].Value!));

            File.Delete(filePath);
        }
        [Fact]
        public void Read_Enum_ReturnCorrectly()
        {
            var filePath = nameof(Read_Enum_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("A");

            sheet.CreateRow(1).CreateCell(0).SetCellValue(9);
            sheet.CreateRow(2).CreateCell(0).SetCellValue("9");
            sheet.CreateRow(3).CreateCell(0).SetCellValue("Success");

            sheet.CreateRow(4).CreateCell(0).SetCellValue(0);
            sheet.CreateRow(5).CreateCell(0).SetCellValue("0");
            sheet.CreateRow(6).CreateCell(0).SetCellValue("Failed");

            sheet.CreateRow(7).CreateCell(0).SetCellValue(11);
            sheet.CreateRow(8).CreateCell(0).SetCellValue("11");
            sheet.CreateRow(9).CreateCell(0).SetCellValue("Pending");

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(fs);
            wb.Close();
            var result = _jfYuExcel.Read<SimpleModel<EnumTest>>(filePath);
            Assert.NotNull(result);
            Assert.Equal(EnumTest.Success, result[0].Value);
            Assert.Equal(EnumTest.Success, result[1].Value);
            Assert.Equal(EnumTest.Success, result[2].Value);
            Assert.Equal(EnumTest.Failed, result[3].Value);
            Assert.Equal(EnumTest.Failed, result[4].Value);
            Assert.Equal(EnumTest.Failed, result[5].Value);
            Assert.Equal(EnumTest.Pending, result[6].Value);
            Assert.Equal(EnumTest.Pending, result[7].Value);
            Assert.Equal(EnumTest.Pending, result[8].Value);

            File.Delete(filePath);
        }
        [Fact]
        public void Read_ModelTitleIsMoreThanExcel_ReturnCorrectly()
        {
            var filePath = nameof(Read_ModelTitleIsMoreThanExcel_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("Id");
            header.CreateCell(1).SetCellValue("ExpiresIn");
            header.CreateCell(2).SetCellValue("xxxx");
            var d1 = DateTime.Now.AddDays(10);
            // data row at 1
            var data1 = sheet.CreateRow(1);
            data1.CreateCell(0).SetCellValue("1");
            data1.CreateCell(1).SetCellValue(d1);


            // data row at 3
            var d2 = DateTime.Now.AddDays(20);
            var data3 = sheet.CreateRow(3);
            data3.CreateCell(0).SetCellValue("2");
            data3.CreateCell(1).SetCellValue(d2);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(fs);
            wb.Close();

            var result = _jfYuExcel.Read<TestSubModel>(filePath);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal($"{d1:G}", $"{result[0].ExpiresIn:G}");
            Assert.Equal(2, result[1].Id);
            Assert.Equal($"{d2:G}", $"{result[1].ExpiresIn:G}");

            File.Delete(filePath);
        }

        [Fact]
        public void Read_SkipsNullRows_WhenEncounteredBetweenFirstAndLastRow()
        {
            var filePath = nameof(Read_SkipsNullRows_WhenEncounteredBetweenFirstAndLastRow) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // header at row 0
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("Value");

            // data row at 1
            var data1 = sheet.CreateRow(1);
            data1.CreateCell(0).SetCellValue("A1");

            // do NOT create row 2 -> should be null

            // data row at 3
            var data3 = sheet.CreateRow(3);
            data3.CreateCell(0).SetCellValue("A3");

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(fs);
            wb.Close();

            var result = _jfYuExcel.Read<dynamic>(filePath);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("A1", result[0].Value);
            Assert.Equal("A3", result[1].Value);

            File.Delete(filePath);
        }
        [Theory]
        [InlineData("2023-01-01", "yyyy-MM-dd")]
        [InlineData("01/01/2023", "dd/MM/yyyy")]
        public void Read_DifferentDateFormats_ReturnCorrectly(string dateString, string formatString)
        {
            // Arrange
            DateTime expectedDate = DateTime.ParseExact(dateString, formatString, CultureInfo.InvariantCulture);
            var filePath = $"{nameof(Read_DifferentDateFormats_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            using var wb = JfYuExcelExtension.CreateExcel();
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
            var data = _jfYuExcel.Read<SimpleModel<DateTime>>(filePath);

            // Assert
            Assert.Equal(expectedDate.Date, data[0].Value);
            File.Delete(filePath);
        }

        #endregion

        #region ReadSteamThrowException

        [Fact]
        public void ReadStream_FileNotExist_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<List<AllTypeTestModel>>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
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
            _jfYuExcel.Write(source.AsQueryable(), filePath);

            var ms = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var ex = Record.Exception(() => _jfYuExcel.Read<Dictionary<string, AllTypeTestModel>>(ms));
            Assert.IsType<NotSupportedException>(ex, exactMatch: false);
            ms.Dispose();
            File.Delete(filePath);
        }

        [Fact]
        public void ReadStream_EmptyFile_ReturnCorrectly()
        {
            var filePath = $"{nameof(ReadStream_EmptyFile_ReturnCorrectly)}.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel>(ms);

            Assert.Empty(data);
            File.Delete(filePath);
            ms.Dispose();
        }

        [Fact]
        public void ReadStream_OnlyHaveHeader_ReturnCorrectly()
        {
            var filePath = $"{nameof(ReadStream_OnlyHaveHeader_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<AllTypeTestModel>();
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);
            var ms = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(ms);

            Assert.Empty(data);

            File.Delete(filePath);
            ms.Dispose();
        }

        #endregion ReadSteam

        #region Multiple sheet

        #region file not exist
        [Fact]
        public void Read_FileNotExist2_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist2_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
        }
        [Fact]
        public void Read_FileNotExist3_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist3_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
        }
        [Fact]
        public void Read_FileNotExist4_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist4_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
        }
        [Fact]
        public void Read_FileNotExist5_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist5_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
        }
        [Fact]
        public void Read_FileNotExist6_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist6_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
        }
        [Fact]
        public void Read_FileNotExist7_ThrowException()
        {
            var filePath = $"{nameof(Read_FileNotExist7_ThrowException)}.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(filePath));
            Assert.IsType<FileNotFoundException>(ex, exactMatch: false);
        }
        #endregion

        #region stream not exist
        [Fact]
        public void ReadStream_FileNotExist2_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
        }
        [Fact]
        public void ReadStream_FileNotExist3_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
        }
        [Fact]
        public void ReadStream_FileNotExist4_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
        }
        [Fact]
        public void ReadStream_FileNotExist5_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
        }
        [Fact]
        public void ReadStream_FileNotExist6_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
        }
        [Fact]
        public void ReadStream_FileNotExist7_ThrowException()
        {
            MemoryStream ms = null!;

            var ex = Record.Exception(() => _jfYuExcel.Read<AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel, AllTypeTestModel>(ms!));
            Assert.IsType<ArgumentNullException>(ex, exactMatch: false);
        }

        #endregion

        #region stream
        [Fact]
        public void Read_StreamSheet1_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet1_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var source = new Tuple<List<AllTypeTestModel>>(d1);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data));

            File.Delete(filePath);
        }

        [Fact]
        public void Read_StreamSheet2_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet2_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60);
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>>(d1, d2);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            File.Delete(filePath);
        }

        [Fact]
        public void Read_StreamSheet3_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet3_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>>(d1, d2, d2);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            File.Delete(filePath);
        }

        [Fact]
        public void Read_StreamSheet4_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet4_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>, List<TestModel>>(d1, d2, d2, d2);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            File.Delete(filePath);
        }

        [Fact]
        public void Read_StreamSheet5_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet5_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>>(d1, d2, d2, d2, d2);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel, TestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item5));
            File.Delete(filePath);
        }

        [Fact]
        public void Read_StreamSheet6_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet6_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>>(d1, d2, d2, d2, d2, d2);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel, TestModel, TestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item5));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item6));
            File.Delete(filePath);
        }

        [Fact]
        public void Read_StreamSheet7_ReturnCorrectly()
        {
            var filePath = $"{nameof(Read_StreamSheet7_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>, List<TestModel>>(d1, d2, d2, d2, d2, d2, d2);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel, TestModel, TestModel, TestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item5));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item6));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item7));
            File.Delete(filePath);
        }

        #endregion

        #endregion

        #region NoIOC
        [Fact]
        public void Read_WithoutIOC_ReturnCorrectly()
        {
            var noIocExcel = new JfYuExcel();

            var filePath = nameof(Read_WithoutIOC_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var source = new Tuple<List<AllTypeTestModel>>(d1);
            // Act
            noIocExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = noIocExcel.Read<AllTypeTestModel>(stream);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data));

            var data1 = noIocExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data1));

            File.Delete(filePath);
        }

        [Fact]
        public void Read_WithoutIOCAndOption_ReturnCorrectly()
        {
            var noIocExcel = new JfYuExcel();

            // Act 
            var filePath = nameof(Read_WithoutIOCAndOption_ReturnCorrectly) + ".xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);
           
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var source = new Tuple<List<AllTypeTestModel>>(d1);
            noIocExcel.Write(source, filePath);
            // Assert

            var data1 = noIocExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data1));

            File.Delete(filePath);
        }
        #endregion
    }
}