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