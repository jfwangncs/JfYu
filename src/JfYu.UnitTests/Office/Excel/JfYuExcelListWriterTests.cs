#if NET8_0_OR_GREATER
using JfYu.Office;
using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.UnitTests.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.IO;

namespace JfYu.UnitTests.Office.Excel
{
    [Collection("Excel")]
    public class JfYuExcelListWriterTests
    {
        public readonly IJfYuExcel _jfYuExcel;
        public JfYuExcelListWriterTests()
        {
            var services = new ServiceCollection();
            services.AddJfYuExcel();
            var serviceProvider = services.BuildServiceProvider();
            _jfYuExcel = serviceProvider.GetRequiredService<IJfYuExcel>();
        }
        [Fact]
        public void ListWriter_WithMultipleSheetButMoreThanMax_ThrowException()
        {
            var filePath = $"{nameof(ListWriter_WithMultipleSheetButMoreThanMax_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
 
            var d1 = new TestModelFaker().Generate(26);
            d1.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            // Act 
            var source = new Tuple<List<TestModel>, List<TestModel>>(d1, d1);            ;

            var ex = Record.Exception(() => _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 10 }));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Fact]
        public void ListWriter_WithoutTitles_ReturnCorrectly()
        {
            var filePath = "ListWriterWithoutTitles.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            // Act 
            _jfYuExcel.Write(source.AsQueryable(), filePath, null, new JfYuExcelOptions() { AllowAppend = WriteOperation.None }, callback);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(6, data.Count);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeIlist_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeIlist.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.Write((IList<AllTypeTestModel>)source, filePath);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeList_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeList.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.Write(source.ToList(), filePath);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeQueryable_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeQueryable.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeEnumerable_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeEnumerable.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);

            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.Write(source.AsEnumerable(), filePath);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithAllType_ReturnCorrectly()
        {
            var filePath = "ListWriterWithAllType.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = AllTypeTestModel.GenerateTestList();
            // Act
            _jfYuExcel.Write(source.AsQueryable(), filePath);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithMultipleSheet_ReturnCorrectly()
        {
            var filePath = "ListWriterWithMultipleSheet.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new TestModelFaker().Generate(26);
            source.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            // Act 
            _jfYuExcel.Write(source.AsQueryable(), filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 10 });

            // Assert
            var data = _jfYuExcel.Read<TestModel>(filePath);
            Assert.NotNull(data);
            Assert.Equal(10, data.Count);

            data.AddRange(_jfYuExcel.Read<TestModel>(filePath, 1, 1)!);
            Assert.NotNull(data);
            Assert.Equal(20, data.Count);

            data.AddRange(_jfYuExcel.Read<TestModel>(filePath, 1, 2)!);
            Assert.NotNull(data);
            Assert.Equal(26, data.Count);

            Assert.Equal(JsonConvert.SerializeObject(source.OrderBy(q => q.Name)), JsonConvert.SerializeObject(data.OrderBy(q => q.Name)));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithUnsupportedType_ReturnCorrectly()
        {
            var filePath = "ListWriterUnsupportedType.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Write(new HashSet<int>(), filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>()));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        [Fact]
        public void ListWriter_WithUnsupportedType1_ReturnCorrectly()
        {
            var filePath = "ListWriterUnsupportedType1.xlsx";

            var ex = Record.Exception(() => _jfYuExcel.Write(new AllTypeTestModel(), filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>()));
            Assert.IsAssignableFrom<Exception>(ex);
        }

        #region Tuple

        [Fact]
        public void ListWriter_WithTypeTuple1_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple1.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var source = new Tuple<List<AllTypeTestModel>>(d1);
            // Act
            _jfYuExcel.Write(source, filePath, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 });

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data));

            var data1 = _jfYuExcel.Read<AllTypeTestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data1));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTuple2_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple2.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTuple3_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple3.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTuple4_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple4.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTuple5_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple5.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item5));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTuple6_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple6.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel,TestModel, TestModel, TestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item5));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item6));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTuple7_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTuple7.xlsx";
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
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel, TestModel, TestModel, TestModel,TestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item3));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item4));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item5));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item6));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item7));
            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithTypeTupleButMoreThanMaxRecord_ThrowException()
        {
            var filePath = "ListWriterWithTypeTupleButMoreThanMaxRecord.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>>(d1, d2);

            var ex = Record.Exception(() => _jfYuExcel.Write(source, filePath, JfYuExcelExtension.GetTitles<AllTypeTestModel>(), excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 }));
            Assert.IsAssignableFrom<InvalidOperationException>(ex);
        }

        [Fact]
        public void ListWriter_WithTypeTupleAndCallBack_ReturnCorrectly()
        {
            var filePath = "ListWriterWithTypeTupleAndCallBack.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var callbackCalledCount = 0;
            void callback(int count) => callbackCalledCount = count;

            var list = AllTypeTestModel.GenerateTestList();
            var d1 = list;
            var d2 = new TestModelFaker().Generate(60).ToList();
            d2.ForEach(q => { q.DateTime = DateTime.Parse(q.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture); q.Sub = null; q.Items = []; });
            var source = new Tuple<List<AllTypeTestModel>, List<TestModel>>(d1, d2);
            // Act
            _jfYuExcel.Write(source, filePath, null, excelOption: new JfYuExcelOptions() { SheetMaxRecord = 100 }, callback);

            // Assert
            var data = _jfYuExcel.Read<AllTypeTestModel, TestModel>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(d1), JsonConvert.SerializeObject(data.Item1));
            Assert.Equal(JsonConvert.SerializeObject(d2), JsonConvert.SerializeObject(data.Item2));
            File.Delete(filePath);
        }
        

        [Fact]
        public void ListWriter_SimpleTypeString_ReturnCorrectly()
        {
            var filePath = $"{nameof(ListWriter_SimpleTypeString_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<string>() { "2", "1Xa1", "3" };
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<SimpleModel<string>>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data.Select(q => q.Value)));

            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_SimpleTypeInt_ReturnCorrectly()
        {
            var filePath = $"{nameof(ListWriter_SimpleTypeInt_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<int>() { 111, 132, 342432 };
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<SimpleModel<int>>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data.Select(q => q.Value)));

            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_SimpleTypeDecimal_ReturnCorrectly()
        {
            var filePath = $"{nameof(ListWriter_SimpleTypeDecimal_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<decimal>() { 111.3213M, 132.8939M, 342432.4294M };
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<SimpleModel<decimal>>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data.Select(q => q.Value)));

            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_SimpleTypeDatetime_ReturnCorrectly()
        {
            var filePath = $"{nameof(ListWriter_SimpleTypeDatetime_ReturnCorrectly)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<DateTime>() { DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture), DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture) };
            // Act
            _jfYuExcel.Write(source, filePath);
            // Assert
            var data = _jfYuExcel.Read<SimpleModel<DateTime>>(filePath);

            Assert.Equal(JsonConvert.SerializeObject(source), JsonConvert.SerializeObject(data.Select(q=>q.Value)));

            File.Delete(filePath);
        }

        [Fact]
        public void ListWriter_WithErrorTitle_ThrowException()
        {
            var filePath = $"{nameof(ListWriter_WithErrorTitle_ThrowException)}.xlsx";
            // Arrange
            if (File.Exists(filePath))
                File.Delete(filePath);
            var source = new List<IJfYuExcel>() { _jfYuExcel };

            // Act
            var ex = Record.Exception(() => _jfYuExcel.Write(source, filePath, new Dictionary<string, string>() { { "x", "y" } }));
            Assert.IsAssignableFrom<InvalidOperationException>(ex);
        }

        #endregion Tuple
    }
}
#endif