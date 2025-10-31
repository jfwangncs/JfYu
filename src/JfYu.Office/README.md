
### <a href="#Office">Office</a>
```
1、支持导出、导入、自由操作Excel。
2、数据源支持List,Datatable、IQueryable、DbDataReader。
3、自动识别表头，如带Model的导出，并且Model字段标记有DisplayName则自动识别为表头，也可自行设置表头。
4、使用SXSSF导出方式，降低内存占用量。支持海量数据导出，使用超过100w数据自动分Sheet

```
Nuget安装

```
Install-Package jfYu.Core.Excel
```

使用

Excel:

```

//IOC注入
builder.Services.AddJfYuExcel();  

//list导出
var source = new List<ExcelTest>();
source.Add(new ExcelTest() { name = "A", age = 18, Address = "地址1" });
source.Add(new ExcelTest() { name = "B", age = 19, Address = "地址2" });
source.Add(new ExcelTest() { name = "C", age = 20, Address = "地址3" });
excel.ToExcel(source, "exceltest/source.xlsx");

//datatable
var dt = new DataTable();
dt.Columns.Add("id");
dt.Columns.Add("name");
dt.Columns.Add("sex");
dt.Columns.Add("age");
dt.Rows.Add("1", "王", "男", "12");
dt.Rows.Add("2", "wang", "男", "1200000");
dt.Rows.Add("3", "大的洼地", "女", "12");
dt.Rows.Add("4", "dwadwadwad", "1", "12");
dt.Rows.Add("5", "13213213", "0", "1465452");
excel.ToExcel(dt, "exceltest/dtwoh.xlsx");

//DbDataReader
using var conn = new SqliteConnection(datasource);         
conn.Open();
sql = "SELECT * FROM test";
cmd.CommandText = sql;
var reader = cmd.ExecuteReader();
excel.ToExcel(reader, "exceltest/dtreader.xlsx", dir);
excel.ToExcel<ExcelTest>(reader, "exceltest/reader.xlsx");//带model

//自由操作Excel
var workbook = excel.CreateWorkbook();
workbook.CreateSheetWithTitles<ExcelTest>();
  var dir = new Dictionary<string, string>
                {
                    { "Age", "年龄" }
                };
workbook.CreateSheetWithTitles(dir);
workbook.Save(workbook, "exceltest/new.xlsx");

 

//excel导入
var dtsourceH = excel.ToDataTable("exceltest/tmp2.xlsx", 0, 1);
var list = excel.GetList<ExcelTest>("exceltest/ImportList.xlsx");

```

Word:

```
builder.Services.AddJfYuWord();

var x = new System.Collections.Generic.Dictionary<string, object>
{
    { "x", "测试哦" }
};

var doc = _jfYuWord.GenerateWord();
var paragraph = doc.CreateParagraph();
paragraph.Alignment = ParagraphAlignment.CENTER; //字体居中
var run = paragraph.CreateRun();
run.IsBold = true;
run.SetText("${x}测试测试${y}");
run.FontSize = 28;
run.SetFontFamily("黑体", FontCharRange.None); //设置黑体
paragraph.SpacingBeforeLines = 20;//上方距离
paragraph.SpacingAfterLines = 20;//下方距离
Directory.CreateDirectory("doctest");
FileStream fs = new("doctest/1.docx", FileMode.Create);
doc.Write(fs);
fs.Close();
x.Add("y", "xxx天秤");
_jfYuWord.GenerateWordByTemplate("doctest/1.docx", x, "doctest/2.docx");
Assert.True(File.Exists("doctest/2.docx"));
var fst = File.Open("doctest/2.docx", FileMode.Open);
Assert.True(fst.Length > 0);
fst.Close();
Directory.Delete("doctest", true);
```