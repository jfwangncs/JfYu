using JfYu.Office;
using JfYu.Office.Excel; 
using Microsoft.Extensions.DependencyInjection; 

namespace JfYu.UnitTests.Office.Excel
{
    /// <summary>
    /// Tests for `JfYuExcel.Read<dynamic>` covering English headers, special characters and Chinese headers.
    /// </summary>
    [Collection("Excel")]
    public class JfYuExcelReadDynamicTests
    {
        private readonly IJfYuExcel _jfYuExcel;

        public JfYuExcelReadDynamicTests()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.AddJfYuExcel().BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
        }

        #region Basic Dynamic read tests (English headers)

        [Fact]
        public void ReadDynamic_SimpleData_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_SimpleData_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath))
                File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            
            // Create header (English)
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Name");
            headerRow.CreateCell(1).SetCellValue("Age");
            headerRow.CreateCell(2).SetCellValue("City");
            
            // Create data rows (English values)
            var dataRow1 = sheet.CreateRow(1);
            dataRow1.CreateCell(0).SetCellValue("John");
            dataRow1.CreateCell(1).SetCellValue(25);
            dataRow1.CreateCell(2).SetCellValue("Beijing");
            
            var dataRow2 = sheet.CreateRow(2);
            dataRow2.CreateCell(0).SetCellValue("Mary");
            dataRow2.CreateCell(1).SetCellValue(30);
            dataRow2.CreateCell(2).SetCellValue("Shanghai");
            
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            var record1 = result[0] as IDictionary<string, object>;
            Assert.NotNull(record1);
            Assert.True(record1.ContainsKey("Name"));
            Assert.Equal("John", record1["Name"].ToString());

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_MixedTypes_HandleCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_MixedTypes_HandleCorrectly)}.xlsx";
            if (File.Exists(filePath))
                File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            
            // Headers in English
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ProductName");
            headerRow.CreateCell(1).SetCellValue("Price");
            headerRow.CreateCell(2).SetCellValue("InStock");
            headerRow.CreateCell(3).SetCellValue("ReleaseDate");
            
            // Data row
            var dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue("Laptop");
            dataRow.CreateCell(1).SetCellValue(5999.99);
            dataRow.CreateCell(2).SetCellValue(true);
            dataRow.CreateCell(3).SetCellValue(DateTime.Now);
            
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.True(record.ContainsKey("ProductName"));
            Assert.True(record.ContainsKey("Price"));
            Assert.True(record.ContainsKey("InStock"));
            Assert.True(record.ContainsKey("ReleaseDate"));

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_FromStream_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_FromStream_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath))
                File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            
            // Header (English)
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ID");
            headerRow.CreateCell(1).SetCellValue("Title");
            headerRow.CreateCell(2).SetCellValue("Content");
            
            // Data rows
            for (int i = 1; i <= 5; i++)
            {
                var dataRow = sheet.CreateRow(i);
                dataRow.CreateCell(0).SetCellValue(i);
                dataRow.CreateCell(1).SetCellValue($"Title{i}");
                dataRow.CreateCell(2).SetCellValue($"Content{i}");
            }
            
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Close();

            // Act
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var result = _jfYuExcel.Read<dynamic>(stream);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
            
            for (int i = 0; i < 5; i++)
            {
                var record = result[i] as IDictionary<string, object>;
                Assert.NotNull(record);
                Assert.True(record.ContainsKey("Title"));
                Assert.Contains($"Title{i + 1}", record["Title"].ToString());
            }

            File.Delete(filePath);
        }

        #endregion

        #region Edge cases

        [Fact]
        public void ReadDynamic_EmptyFile_ReturnEmptyList()
        {
            var filePath = $"{nameof(ReadDynamic_EmptyFile_ReturnEmptyList)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            wb.CreateSheet("Sheet1");
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_OnlyHeader_ReturnEmptyList()
        {
            var filePath = $"{nameof(ReadDynamic_OnlyHeader_ReturnEmptyList)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Col1");
            headerRow.CreateCell(1).SetCellValue("Col2");
            headerRow.CreateCell(2).SetCellValue("Col3");
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_WithNullAndEmptyValues_HandleCorrectly()
        {
            var filePath = $"{nameof(ReadDynamic_WithNullAndEmptyValues_HandleCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Name");
            headerRow.CreateCell(1).SetCellValue("Age");
            headerRow.CreateCell(2).SetCellValue("Note");

            var dataRow1 = sheet.CreateRow(1);
            dataRow1.CreateCell(0).SetCellValue("张三");
            dataRow1.CreateCell(1).SetCellValue(""); // empty string
            // Cell 2 not created -> null

            var dataRow2 = sheet.CreateRow(2);
            dataRow2.CreateCell(0).SetCellValue("李四");
            dataRow2.CreateCell(1).SetCellValue(25);
            dataRow2.CreateCell(2).SetCellValue("Has note");

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            var record1 = result[0] as IDictionary<string, object>;
            Assert.NotNull(record1);
            Assert.True(record1.ContainsKey("Name"));

            File.Delete(filePath);
        }

        #endregion

        #region Multi-sheet tests

        [Fact]
        public void ReadDynamic_MultipleSheets_ReadFirstSheetByDefault()
        {
            var filePath = $"{nameof(ReadDynamic_MultipleSheets_ReadFirstSheetByDefault)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet1 = wb.CreateSheet("Sheet1");
            var headerRow1 = sheet1.CreateRow(0);
            headerRow1.CreateCell(0).SetCellValue("FirstSheet");
            var dataRow1 = sheet1.CreateRow(1);
            dataRow1.CreateCell(0).SetCellValue("Data1");

            var sheet2 = wb.CreateSheet("Sheet2");
            var headerRow2 = sheet2.CreateRow(0);
            headerRow2.CreateCell(0).SetCellValue("SecondSheet");
            var dataRow2 = sheet2.CreateRow(1);
            dataRow2.CreateCell(0).SetCellValue("Data2");

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act - 默认读取第一个Sheet
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.True(record.ContainsKey("FirstSheet"));

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_SpecificSheet_ReadCorrectSheet()
        {
            var filePath = $"{nameof(ReadDynamic_SpecificSheet_ReadCorrectSheet)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet1 = wb.CreateSheet("SalesData");
            var headerRow1 = sheet1.CreateRow(0);
            headerRow1.CreateCell(0).SetCellValue("Product");
            var dataRow1 = sheet1.CreateRow(1);
            dataRow1.CreateCell(0).SetCellValue("ProductA");

            var sheet2 = wb.CreateSheet("StockData");
            var headerRow2 = sheet2.CreateRow(0);
            headerRow2.CreateCell(0).SetCellValue("Warehouse");
            var dataRow2 = sheet2.CreateRow(1);
            dataRow2.CreateCell(0).SetCellValue("WarehouseA");

            var sheet3 = wb.CreateSheet("FinanceData");
            var headerRow3 = sheet3.CreateRow(0);
            headerRow3.CreateCell(0).SetCellValue("Account");
            var dataRow3 = sheet3.CreateRow(1);
            dataRow3.CreateCell(0).SetCellValue("Revenue");

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act - 读取第二个Sheet (索引为1)
            var result = _jfYuExcel.Read<dynamic>(filePath, sheetIndex: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.True(record.ContainsKey("Warehouse"));
            Assert.Equal("WarehouseA", record["Warehouse"].ToString());

            File.Delete(filePath);
        }

        #endregion
        #region Special headers tests
        [Fact]
        public void ReadDynamic_SpecialHeaders_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_SpecialHeaders_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");

            // Chinese headers
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("1234");
            headerRow.CreateCell(1).SetCellValue(" ");
            headerRow.CreateCell(2).SetCellValue("2dada");
            headerRow.CreateCell(3).SetCellValue("!@#$%^&*(){}\\;'.,/~:\"<>?|");
            headerRow.CreateCell(4).SetCellValue(" ");

            var dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue("王小明");
            dataRow.CreateCell(1).SetCellValue("研发部1");
            dataRow.CreateCell(2).SetCellValue("研发部2");
            dataRow.CreateCell(3).SetCellValue(15000.50);
            dataRow.CreateCell(4).SetCellValue("研发部3");

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.Equal("王小明", record["_1234"]);
            Assert.Equal("研发部1", record["_"]);
            Assert.Equal("研发部2", record["_2dada"]);
            Assert.Equal(15000.50, record["_1"]);
            Assert.Equal("研发部3", record["_2"]);

            File.Delete(filePath);
        }
        #endregion
        #region Special characters and Chinese headers tests

        [Fact]
        public void ReadDynamic_ChineseHeaders_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_ChineseHeaders_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            
            // Chinese headers
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("员工姓名");
            headerRow.CreateCell(1).SetCellValue("部门名称");
            headerRow.CreateCell(2).SetCellValue("月度工资");
            
            var dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue("王小明");
            dataRow.CreateCell(1).SetCellValue("研发部");
            dataRow.CreateCell(2).SetCellValue(15000.50);
            
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.True(record.ContainsKey("员工姓名"));
            Assert.True(record.ContainsKey("部门名称"));
            Assert.True(record.ContainsKey("月度工资"));

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_SpecialCharactersInHeaders_HandleCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_SpecialCharactersInHeaders_HandleCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            
            // Headers with special characters
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Name (姓名)");
            headerRow.CreateCell(1).SetCellValue("Age-年龄");
            headerRow.CreateCell(2).SetCellValue("Email@Address");
            
            var dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue("TestUser");
            dataRow.CreateCell(1).SetCellValue(30);
            dataRow.CreateCell(2).SetCellValue("test@example.com");
            
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);

            File.Delete(filePath);
        }

        #endregion

        #region Large dataset tests

        [Fact]
        public void ReadDynamic_LargeDataSet_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_LargeDataSet_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Index");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Value");

            for (int i = 1; i <= 1000; i++)
            {
                var dataRow = sheet.CreateRow(i);
                dataRow.CreateCell(0).SetCellValue(i);
                dataRow.CreateCell(1).SetCellValue($"Item{i}");
                dataRow.CreateCell(2).SetCellValue(i * 100.5);
            }

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Count);
            
            var firstRecord = result[0] as IDictionary<string, object>;
            Assert.NotNull(firstRecord);
            
            var lastRecord = result[999] as IDictionary<string, object>;
            Assert.NotNull(lastRecord);

            File.Delete(filePath);
        }

        #endregion

        #region Exception cases

        [Fact]
        public void ReadDynamic_FileNotExist_ThrowException()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_FileNotExist_ThrowException)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            // Act & Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<dynamic>(filePath));
            Assert.NotNull(ex);
            Assert.IsType<FileNotFoundException>(ex);
        }

        [Fact]
        public void ReadDynamic_NullStream_ThrowException()
        {
            // Arrange
            Stream stream = null!;

            // Act & Assert
            var ex = Record.Exception(() => _jfYuExcel.Read<dynamic>(stream));
            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void ReadDynamic_InvalidSheetIndex_ThrowException()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_InvalidSheetIndex_ThrowException)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            wb.CreateSheet("Sheet1");
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act & Assert - 尝试读取不存在的Sheet (索引为5)
            var ex = Record.Exception(() => _jfYuExcel.Read<dynamic>(filePath, sheetIndex: 5));
            Assert.NotNull(ex);

            File.Delete(filePath);
        }

        #endregion

        #region Data type conversion tests (English headers)

        [Fact]
        public void ReadDynamic_NumericTypes_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_NumericTypes_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Integer");
            headerRow.CreateCell(1).SetCellValue("Float");
            headerRow.CreateCell(2).SetCellValue("Percentage");

            var dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue(100);
            dataRow.CreateCell(1).SetCellValue(99.99);
            dataRow.CreateCell(2).SetCellValue(0.85);

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.True(record.ContainsKey("Integer"));
            Assert.True(record.ContainsKey("Float"));
            Assert.True(record.ContainsKey("Percentage"));

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_DateTimeTypes_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_DateTimeTypes_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("CreatedDate");
            headerRow.CreateCell(1).SetCellValue("UpdatedAt");

            var now = DateTime.Now;
            var dataRow = sheet.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue(now.Date);
            dataRow.CreateCell(1).SetCellValue(now);

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.True(record.ContainsKey("CreatedDate"));
            Assert.True(record.ContainsKey("UpdatedAt"));

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_BooleanTypes_ReturnCorrectly()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_BooleanTypes_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("IsActive");
            headerRow.CreateCell(1).SetCellValue("IsDeleted");

            var dataRow1 = sheet.CreateRow(1);
            dataRow1.CreateCell(0).SetCellValue(true);
            dataRow1.CreateCell(1).SetCellValue(false);

            var dataRow2 = sheet.CreateRow(2);
            dataRow2.CreateCell(0).SetCellValue(false);
            dataRow2.CreateCell(1).SetCellValue(true);

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var record1 = result[0] as IDictionary<string, object>;
            Assert.NotNull(record1);
            Assert.True(record1.ContainsKey("IsActive"));
            Assert.True(record1.ContainsKey("IsDeleted"));

            File.Delete(filePath);
        }

        #endregion

        #region Performance and boundary tests (English headers)

        [Fact]
        public void ReadDynamic_WideTable_ReturnCorrectly()
        {
            // Arrange - 测试宽表（很多列）
            var filePath = $"{nameof(ReadDynamic_WideTable_ReturnCorrectly)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            for (int col = 0; col < 50; col++) headerRow.CreateCell(col).SetCellValue($"Col{col + 1}");

            var dataRow = sheet.CreateRow(1);
            for (int col = 0; col < 50; col++) dataRow.CreateCell(col).SetCellValue($"Data{col + 1}");

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act
            var result = _jfYuExcel.Read<dynamic>(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var record = result[0] as IDictionary<string, object>;
            Assert.NotNull(record);
            Assert.Equal(50, record.Count);

            File.Delete(filePath);
        }

        [Fact]
        public void ReadDynamic_FirstRowParameter_ReadFromSpecificRow()
        {
            // Arrange
            var filePath = $"{nameof(ReadDynamic_FirstRowParameter_ReadFromSpecificRow)}.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            var wb = _jfYuExcel.CreateExcel();
            var sheet = wb.CreateSheet("Sheet1");
            var titleRow = sheet.CreateRow(0); titleRow.CreateCell(0).SetCellValue("TitleRow");
            var descRow = sheet.CreateRow(1); descRow.CreateCell(0).SetCellValue("DescriptionRow");
            var headerRow = sheet.CreateRow(2); headerRow.CreateCell(0).SetCellValue("Name"); headerRow.CreateCell(1).SetCellValue("Age");

            var dataRow1 = sheet.CreateRow(3); dataRow1.CreateCell(0).SetCellValue("John"); dataRow1.CreateCell(1).SetCellValue(25);
            var dataRow2 = sheet.CreateRow(4); dataRow2.CreateCell(0).SetCellValue("Mary"); dataRow2.CreateCell(1).SetCellValue(30);

            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) wb.Write(savefs);
            wb.Close();

            // Act - 从第3行开始读取（表头在第2行）
            var result = _jfYuExcel.Read<dynamic>(filePath, firstRow: 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            File.Delete(filePath);
        }

        #endregion
    }
}
