using JfYu.Office.Excel.Constant;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JfYu.Office.Excel.Extensions
{
    /// <summary>
    /// The extension methods for Excel operations.
    /// </summary>
    public static partial class JfYuExcelExtension
    {
        /// <summary>
        /// Creates an Excel workbook.
        /// </summary>
        /// <param name="excelVersion">The version of the Excel file to create.</param>
        /// <param name="rowAccessSize">The row access size for the workbook.</param>
        /// <returns>The created workbook.</returns>
        public static IWorkbook CreateExcel(JfYuExcelVersion excelVersion = JfYuExcelVersion.Xlsx, int rowAccessSize = 1000)
        {
            if (excelVersion == JfYuExcelVersion.Xls)
                return new HSSFWorkbook();
            else if (excelVersion == JfYuExcelVersion.Xlsx)
                return new SXSSFWorkbook(new XSSFWorkbook(), rowAccessSize);
            else
                throw new ArgumentException("not support create CSV file");
        }

        /// <summary>
        /// Gets the titles of the properties of a given type T.
        /// </summary>
        /// <typeparam name="T">The type to get the property titles from.</typeparam>
        /// <returns>A dictionary with property names as keys and display names as values.</returns>
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
        /// Gets the titles of the properties of a given type.
        /// </summary>
        /// <param name="type">The type to get the property titles from.</param>
        /// <returns>A dictionary with property names as keys and display names as values.</returns>
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
        /// Adds a title row to the given sheet.
        /// </summary>
        /// <param name="sheet">The sheet to add the title row to.</param>
        /// <param name="titles">A dictionary with column names and their corresponding titles.</param>
        /// <returns>The sheet with the added title row.</returns>
        public static ISheet AddTitle(this ISheet sheet, Dictionary<string, string> titles)
        {
            ArgumentNullException.ThrowIfNull(sheet);
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
        /// Gets a list of data from the sheet and converts it to the specified type.
        /// </summary>
        /// <param name="sheet">The sheet to read from.</param>
        /// <param name="type">The type to convert the data to.</param>
        /// <param name="firstRow">The first row to start reading from.</param>
        /// <returns>A list of data converted to the specified type.</returns>

        public static IList GetList(this ISheet sheet, Type type, int firstRow)
        {
            Type listType = typeof(List<>).MakeGenericType(type);
            IList list = (IList)Activator.CreateInstance(listType)!;

            //Title
            var headerRow = sheet.GetRow(0);
            if (headerRow == null)
                return list;
            int cellCount = headerRow.LastCellNum;
            var titles = GetTitles(type);
            if (type.IsSimpleType())
                titles = new Dictionary<string, string>() { { "A", "A" } };
            Dictionary<int, string> cellNums = [];
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                var headerValue = headerRow.GetCell(i).StringCellValue.Trim();
                if (titles.ContainsValue(headerValue))
                    cellNums.Add(i, headerValue);
            }

            //Content
            for (int i = firstRow; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                object? item;
                if (type == typeof(string))
                    item = string.Empty;
                else
                    item = Activator.CreateInstance(type);
                foreach (var c in cellNums)
                {
                    var cell = row.GetCell(c.Key);
                    if (type.IsSimpleType())
                    {
                        var result = ConvertCellValue(type, cell);
                        item = result;
                    }
                    else
                    {
                        var p = type.GetProperty(c.Value);

                        if (p == null)
                        {
                            var title = titles.FirstOrDefault(q => q.Value == c.Value);
                            p = type.GetProperty(title.Key);
                        }
                        if (p != null)
                        {
                            var result = ConvertCellValue(p.PropertyType, cell);
                            if (result != null)
                                p.SetValue(item, result, null);
                            else if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                p.SetValue(item, null, null);
                            else if (Type.GetTypeCode(p.PropertyType) == TypeCode.String)
                                p.SetValue(item, null, null);
                            else
                                throw new InvalidCastException($"Convert {p.Name} get error,value:{result}，model type:{p.PropertyType.Name},excel type {cell.CellType}.");
                        }
                    }
                }
                list.Add(item);
            }
            return list;
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