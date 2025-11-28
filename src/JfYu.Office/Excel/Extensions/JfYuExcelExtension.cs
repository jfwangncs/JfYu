using JfYu.Office.Excel.Constant;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JfYu.Office.Excel.Extensions
{
    /// <summary>
    /// Provides extension methods for Excel operations including workbook creation, title extraction, and sheet manipulation.
    /// These methods support both NPOI ISheet and IWorkbook objects.
    /// </summary>
    public static partial class JfYuExcelExtension
    {
        /// <summary>
        /// Creates an Excel workbook of the specified version.
        /// For .xlsx files, creates a streaming workbook (SXSSFWorkbook) for memory-efficient large file operations.
        /// For .xls files, creates a standard HSSFWorkbook.
        /// </summary>
        /// <param name="excelVersion">The Excel file format version to create (Xlsx or Xls).</param>
        /// <param name="rowAccessSize">The number of rows to keep in memory for SXSSF streaming mode. Default is 1000. Smaller values reduce memory usage.</param>
        /// <returns>An IWorkbook instance of the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown when attempting to create a CSV file (use WriteCSV method instead).</exception>
        public static IWorkbook CreateExcel(JfYuExcelVersion excelVersion = JfYuExcelVersion.Xlsx, int rowAccessSize = 1000)
        {
            if (excelVersion == JfYuExcelVersion.Xls)
                return new HSSFWorkbook();
            else if (excelVersion == JfYuExcelVersion.Xlsx)
            {
#pragma warning disable CA2000 // SXSSFWorkbook takes ownership of XSSFWorkbook and will dispose it when SXSSFWorkbook is disposed
                return new SXSSFWorkbook(new XSSFWorkbook(), rowAccessSize);
#pragma warning restore CA2000
            }
            else
                throw new ArgumentException("not support create CSV file");
        }

        /// <summary>
        /// Extracts property titles from a type T, using DisplayName attributes when available.
        /// Only includes simple types (primitives, strings, dates, enums, GUIDs, decimals).
        /// </summary>
        /// <typeparam name="T">The type to extract property titles from.</typeparam>
        /// <returns>A dictionary mapping property names to their display names (from DisplayName attribute or property name if not specified).</returns>
        public static Dictionary<string, string> GetTitles<T>()
        {
            var titles = new Dictionary<string, string>();
            var pops = typeof(T).GetProperties();
            for (int i = 0; i < pops.Length; i++)
            {
                var t = pops[i].PropertyType;
                if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    t = t.GetGenericArguments()[0];
                if (t.IsSimpleType())
                {
                    // Get display name if available, otherwise get property's name
                    var colName = pops[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? pops[i].Name;
                    titles.Add(pops[i].Name, colName);
                }
            }
            return titles;
        }

        /// <summary>
        /// Extracts property titles from a runtime Type, using DisplayName attributes when available.
        /// Only includes simple types (primitives, strings, dates, enums, GUIDs, decimals).
        /// </summary>
        /// <param name="type">The Type to extract property titles from.</param>
        /// <returns>A dictionary mapping property names to their display names (from DisplayName attribute or property name if not specified).</returns>
        public static Dictionary<string, string> GetTitles(Type type)
        {
            var titles = new Dictionary<string, string>();
            var pops = type.GetProperties();
            for (int i = 0; i < pops.Length; i++)
            {
                var t = pops[i].PropertyType;
                if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    t = t.GetGenericArguments()[0];
                if (t.IsSimpleType())
                {
                    var colName = pops[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? pops[i].Name;
                    titles.Add(pops[i].Name, colName);
                }
            }
            return titles;
        }

        /// <summary>
        /// Adds a formatted title row to the first row (index 0) of the sheet.
        /// Applies bold, centered styling and automatically adjusts column widths based on title text length.
        /// </summary>
        /// <param name="sheet">The sheet to add the title row to.</param>
        /// <param name="titles">A dictionary mapping column keys to their display titles. The order determines column positions.</param>
        /// <returns>The same sheet instance with the title row added, for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when sheet is null.</exception>
        public static ISheet AddTitle(this ISheet sheet, Dictionary<string, string> titles)
        {

#if NETSTANDARD2_0
            ArgumentNullExceptionExtension.ThrowIfNull(sheet);
#else
            ArgumentNullException.ThrowIfNull(sheet);
#endif
            IRow headerRow = sheet.CreateRow(0);
            ICellStyle headStyle = sheet.Workbook.CreateCellStyle();
            headStyle.Alignment = HorizontalAlignment.Center;
            IFont font = sheet.Workbook.CreateFont();
            font.FontHeightInPoints = 10;
            font.IsBold = true;
            headStyle.SetFont(font);
            var i = 0;
            foreach (var header in titles.Select(q => q.Value))
            {
                headerRow.CreateCell(i).SetCellValue(header);
                var colLength = Encoding.UTF8.GetBytes(header).Length;
                if (colLength > 100)
                    colLength = 100 * 256;
                else
                    colLength = colLength < 20 ? 10 * 256 : colLength * 256;
                sheet.SetColumnWidth(i, colLength);
                headerRow.GetCell(i).CellStyle = headStyle;
                i++;
            }
            return sheet;
        }
        /// <summary>
        /// Determines if the given type is a simple type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a simple type; otherwise, false.</returns>
        internal static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(Guid) || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
        }
    }
}