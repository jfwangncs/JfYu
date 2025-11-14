# JfYu.Office

A powerful .NET library for Excel and Word document manipulation, supporting comprehensive read/write operations, multiple data sources, and template-based generation.

## Features

### Excel Features

- **Multiple Format Support**: Excel (.xlsx, .xls) and CSV files
- **Flexible Data Sources**:
  - `List<T>`, `IList<T>`, `IQueryable<T>`, `IEnumerable<T>`
  - `DataTable`
  - `DbDataReader`
  - Dynamic objects
- **Smart Column Headers**: Auto-detects from `DisplayName` attributes or custom titles
- **High Performance**:
  - SXSSF streaming mode for large datasets
  - Automatic sheet splitting for massive data (configurable via `SheetMaxRecord`)
  - Memory-efficient processing with configurable row access size
- **Read Operations**:
  - Strongly-typed model binding
  - Dynamic object support
  - Multi-sheet reading (supports reading up to 7 different types from different sheets)
  - Stream and file path support
- **Write Operations**:
  - Multiple write modes (None, Append)
  - Progress callback support
  - Automatic column width adjustment

### Word Features

- **Template-Based Generation**: Replace placeholders in Word templates
- **Text Replacement**: Replace `{placeholder}` patterns with actual values
- **Image Insertion**: Insert images at placeholder positions
- **Table Support**: Replacements work in tables and paragraphs

## Installation

```bash
dotnet add package JfYu.Office
```

Or via NuGet Package Manager:

```powershell
Install-Package JfYu.Office
```

## Quick Start

### Dependency Injection Setup

```csharp
// Register Excel services
builder.Services.AddJfYuExcel();

// Register Excel services with custom options
builder.Services.AddJfYuExcel(options =>
{
    options.SheetMaxRecord = 1000000;  // Max records per sheet before splitting
    options.RowAccessSize = 100;       // SXSSF row access window size
    options.AllowAppend = WriteOperation.None;  // Default write operation
});

// Register Word services
builder.Services.AddJfYuWord();
```

## Excel Usage Examples

### Writing Data

#### Write from List

```csharp
// Simple list export
var data = new List<Product>
{
    new() { Id = 1, Name = "Laptop", Price = 5999.99m },
    new() { Id = 2, Name = "Mouse", Price = 99.99m },
    new() { Id = 3, Name = "Keyboard", Price = 299.99m }
};

_jfYuExcel.Write(data, "products.xlsx");

// With custom column titles
var titles = new Dictionary<string, string>
{
    { "Id", "产品ID" },
    { "Name", "产品名称" },
    { "Price", "价格" }
};
_jfYuExcel.Write(data, "products_cn.xlsx", titles);

// With progress callback
_jfYuExcel.Write(data, "products.xlsx", titles, null, (count) =>
{
    Console.WriteLine($"Processed {count} rows");
});
```

#### Write from DataTable

```csharp
var dt = new DataTable();
dt.Columns.Add("Id", typeof(int));
dt.Columns.Add("Name", typeof(string));
dt.Columns.Add("Age", typeof(int));

dt.Rows.Add(1, "Alice", 25);
dt.Rows.Add(2, "Bob", 30);
dt.Rows.Add(3, "Charlie", 35);

_jfYuExcel.Write(dt, "users.xlsx");
```

#### Write from DbDataReader

```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
using var command = connection.CreateCommand();
command.CommandText = "SELECT Id, Name, Email FROM Users";
using var reader = await command.ExecuteReaderAsync();

var titles = new Dictionary<string, string>
{
    { "Id", "用户ID" },
    { "Name", "姓名" },
    { "Email", "邮箱" }
};

_jfYuExcel.Write(reader, "users.xlsx", titles);
```

#### Advanced Options

```csharp
// Append to existing file
var options = new JfYuExcelOptions
{
    AllowAppend = WriteOperation.Append,
    SheetMaxRecord = 50000  // Split into new sheet after 50k records
};

_jfYuExcel.Write(data, "output.xlsx", titles, options, (count) =>
{
    Console.WriteLine($"Progress: {count}");
});
```

### Reading Data

#### Read to Strongly-Typed List

```csharp
// Read from first sheet, starting from row 1 (after header)
var products = _jfYuExcel.Read<Product>("products.xlsx");

// Read from specific sheet and row
var products = _jfYuExcel.Read<Product>("products.xlsx", firstRow: 2, sheetIndex: 1);

// Read from stream
using var stream = File.OpenRead("products.xlsx");
var products = _jfYuExcel.Read<Product>(stream);
```

#### Read Multiple Sheet Types

```csharp
// Read 2 different types from different sheets
var (products, orders) = _jfYuExcel.Read<Product, Order>("data.xlsx");

// Read 3 types
var (products, orders, customers) = _jfYuExcel.Read<Product, Order, Customer>("data.xlsx");

// Supports up to 7 different types
var (t1, t2, t3, t4, t5, t6, t7) = _jfYuExcel.Read<T1, T2, T3, T4, T5, T6, T7>("data.xlsx");
```

#### Read as Dynamic Objects

```csharp
// Read with dynamic column names
var dynamicData = _jfYuExcel.Read<dynamic>("products.xlsx");
foreach (var row in dynamicData)
{
    var dict = row as IDictionary<string, object>;
    Console.WriteLine($"{dict["Name"]}: {dict["Price"]}");
}
```

### CSV Operations

```csharp
// Write CSV
var data = new List<Person> { /* ... */ };
_jfYuExcel.WriteCSV(data, "export.csv");

// Write CSV with custom titles
var titles = new Dictionary<string, string> { { "Name", "姓名" }, { "Age", "年龄" } };
_jfYuExcel.WriteCSV(data, "export.csv", titles);

// Read CSV
var csvData = _jfYuExcel.ReadCSV("import.csv");

// Read CSV starting from specific row
var csvData = _jfYuExcel.ReadCSV("import.csv", firstRow: 5);
```

### Advanced Excel Manipulation

```csharp
// Create custom workbook
var workbook = _jfYuExcel.CreateExcel(JfYuExcelVersion.Xlsx);
var sheet = workbook.CreateSheet("MySheet");

// Add title row using extension method
var titles = new Dictionary<string, string>
{
    { "Column1", "Header 1" },
    { "Column2", "Header 2" }
};
sheet.AddTitle(titles);

// Add data rows manually
var row = sheet.CreateRow(1);
row.CreateCell(0).SetCellValue("Value 1");
row.CreateCell(1).SetCellValue("Value 2");

// Save workbook
using var fileStream = new FileStream("custom.xlsx", FileMode.Create);
workbook.Write(fileStream);
```

## Word Usage Examples

### Template-Based Generation

```csharp
// Create replacements list
var replacements = new List<JfYuWordReplacement>
{
    // Text replacements
    new() { Key = "name", Value = new JfYuWordString { Text = "John Doe" } },
    new() { Key = "date", Value = new JfYuWordString { Text = DateTime.Now.ToString("yyyy-MM-dd") } },
    new() { Key = "title", Value = new JfYuWordString { Text = "Annual Report" } },

    // Image replacements
    new()
    {
        Key = "logo",
        Value = new JfYuWordPicture
        {
            Width = 200,
            Height = 100,
            Bytes = File.ReadAllBytes("logo.png")
        }
    }
};

// Generate document from template
_jfYuWord.GenerateWordByTemplate(
    "template.docx",      // Template file path
    "output.docx",        // Output file path
    replacements          // Replacement list
);
```

### Template Format

Template placeholders use curly braces:

```
Document Title: {title}
Name: {name}
Date: {date}

{logo}

This is a template document with placeholders that will be replaced.
```

### Programmatic Document Creation

```csharp
using NPOI.XWPF.UserModel;

// For advanced document creation, use NPOI directly
var doc = new XWPFDocument();
var paragraph = doc.CreateParagraph();
paragraph.Alignment = ParagraphAlignment.CENTER;

var run = paragraph.CreateRun();
run.IsBold = true;
run.SetText("Document Title");
run.FontSize = 28;
run.SetFontFamily("Arial", FontCharRange.None);

using var fs = new FileStream("document.docx", FileMode.Create);
doc.Write(fs);
```

## Configuration Options

### JfYuExcelOptions

```csharp
public class JfYuExcelOptions
{
    /// <summary>
    /// Maximum records per sheet before auto-splitting (default: 1048576)
    /// </summary>
    public int SheetMaxRecord { get; set; } = 1048576;

    /// <summary>
    /// SXSSF row access window size (default: 100)
    /// </summary>
    public int RowAccessSize { get; set; } = 100;

    /// <summary>
    /// Write operation mode (default: WriteOperation.None)
    /// </summary>
    public WriteOperation AllowAppend { get; set; } = WriteOperation.None;
}
```

### WriteOperation Enum

```csharp
public enum WriteOperation
{
    None = 0,    // Throws exception if file exists
    Append = 1   // Appends data as new sheet
}
```

## Model Attributes

Use `DisplayName` attribute for automatic column header recognition:

```csharp
using System.ComponentModel;

public class Product
{
    public int Id { get; set; }

    [DisplayName("产品名称")]
    public string Name { get; set; }

    [DisplayName("单价")]
    public decimal Price { get; set; }

    [DisplayName("库存数量")]
    public int Stock { get; set; }
}
```

## Supported Data Types

The library supports all common .NET types:

- Primitive types: `int`, `long`, `short`, `byte`, `bool`, `char`
- Floating-point: `float`, `double`, `decimal`
- Date/Time: `DateTime`, `DateTimeOffset`, `TimeSpan`
- Text: `string`, `Guid`
- Nullable versions of above types
- Enums

## Error Handling

```csharp
try
{
    var data = _jfYuExcel.Read<Product>("products.xlsx");
}
catch (FileNotFoundException ex)
{
    // File not found
}
catch (FormatException ex)
{
    // Data type conversion error
}
catch (Exception ex)
{
    // Other errors (invalid file format, etc.)
}
```

## Best Practices

1. **Use streaming for large datasets**: The library automatically uses SXSSF for efficient memory usage
2. **Configure sheet splitting**: Set `SheetMaxRecord` based on your data volume
3. **Provide progress callbacks**: For long-running operations, use callbacks to track progress
4. **Use strongly-typed models**: Better type safety and automatic header detection
5. **Dispose resources**: Always dispose of streams and connections properly
6. **Validate data**: Check data compatibility before writing to avoid conversion errors

## Multi-Targeting

- **Targets**: `net8.0`
- **Dependencies**: NPOI for Office file manipulation

## License

MIT License - see LICENSE file for details

## Source Code

GitHub: [https://github.com/jfwangncs/JfYu](https://github.com/jfwangncs/JfYu)
