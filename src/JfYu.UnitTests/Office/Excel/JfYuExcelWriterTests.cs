#if NET8_0_OR_GREATER
using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.UserModel;
using System.Data;
using System.Text;
using JfYu.Office.Excel.Extensions;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelWriterTests
    {
        private readonly IJfYuExcel _jfYuExcel;
        public JfYuExcelWriterTests()
        {
            var services = new ServiceCollection();
            services.AddJfYuExcel();
            var serviceProvider = services.BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
        }
        [Theory]
        [InlineData(JfYuExcelVersion.Xlsx)]
        [InlineData(JfYuExcelVersion.Xls)]
        public void CreateExcel_ReturnIWorkBook(JfYuExcelVersion version)
        {
            var wb = _jfYuExcel.CreateExcel(version);
            Assert.NotNull(wb);
        }

        [Fact]
        public void CreateExcel_UnSupportVersion_ThrowException()
        {
            var ex = Record.Exception(() => _jfYuExcel.CreateExcel(JfYuExcelVersion.Csv));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("!@#$%^&*()_+")]
        public void ExcelWriter_PathIsNullOrEmpty_ThrowException(string? path)
        {
            using var dt = new DataTable();
            var ex = Record.Exception(() => _jfYuExcel.Write(dt, path!));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Fact]
        public void ExcelWriter_NoImplementedProvider_ThrowException()
        {
            var data = new Dictionary<string, string>();
            var ex = Record.Exception(() => _jfYuExcel.Write(data, "1.xlsx"));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Fact]
        public void AddTitle_AddsHeadersToFirstRow()
        {
            // Arrange
            var titles = new Dictionary<string, string> { { "Column1", "Header 1" }, { "Column2", "Header 2" }, { "Column3", "abcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcde" }, { "Column4", "abcdeabcdeabcdeabcdeabcde" } };

            // Act
            var workbook = _jfYuExcel.CreateExcel();
            var sheet = workbook.CreateSheet();
            sheet.AddTitle(titles);

            // Assert
            IRow headerRow = sheet.GetRow(0);
            Assert.NotNull(headerRow);
            int i = 0;
            foreach (var title in titles)
            {
                ICell cell = headerRow.GetCell(i++);
                Assert.NotNull(cell);
                Assert.Equal(title.Value, cell.StringCellValue);

                ICellStyle style = cell.CellStyle;
                Assert.Equal(HorizontalAlignment.Center, style.Alignment);
                IFont font = style.GetFont(workbook);
                Assert.Equal(10, font.FontHeightInPoints);
                Assert.True(font.IsBold);
            }
            // Verify column widths
            i = 0;
            foreach (var title in titles)
            {
                int colLength = Encoding.UTF8.GetBytes(title.Value).Length;
                int multiplier;
                if (colLength < 20)
                    multiplier = 10;
                else if (colLength > 100)
                    multiplier = 100;
                else
                    multiplier = colLength;

                int expectedWidth = multiplier * 256;
                Assert.Equal(expectedWidth, sheet.GetColumnWidth(i++));
            }

            workbook.Close();
        }
    }
}
#endif