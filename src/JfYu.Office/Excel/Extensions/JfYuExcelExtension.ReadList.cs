using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JfYu.Office.Excel.Extensions
{
    public static partial class JfYuExcelExtension
    {
        /// <summary>
        /// Converts the value of a cell to the specified target type.
        /// </summary>
        /// <param name="targetType">The target type to convert the cell value to.</param>
        /// <param name="cell">The cell to get the value from.</param>
        /// <returns>The converted value.</returns>

        public static object? ConvertCellValue(Type targetType, ICell cell)
        {
            if (cell == null)
                return default;

            object? rawValue = cell.CellType switch
            {
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                    ? cell.DateCellValue
                    : (object)cell.NumericCellValue,

                CellType.String => cell.StringCellValue,

                CellType.Boolean => cell.BooleanCellValue,

                CellType.Formula => cell.CachedFormulaResultType switch
                {
                    CellType.Numeric => cell.NumericCellValue,
                    CellType.String => cell.StringCellValue,
                    CellType.Boolean => cell.BooleanCellValue,
                    _ => throw new InvalidOperationException("Formula resulted in an error.")
                },
                CellType.Blank => null,
                _ => throw new ArgumentException($"Unknown cell type: {cell.CellType}")
            };

            if (rawValue == null)
                return default;

            var isNullable = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
                targetType = Nullable.GetUnderlyingType(targetType)!;

            if (targetType.IsInstanceOfType(rawValue))
                return rawValue;

            // enums
            if (targetType.IsEnum)
            {
                if (rawValue is string)
                    return Enum.Parse(targetType, rawValue.ToString()!, ignoreCase: true);
                // convert underlying numeric and then to enum
                var underlying = Enum.GetUnderlyingType(targetType);
                var val = Convert.ChangeType(rawValue, underlying, CultureInfo.InvariantCulture);
                return Enum.ToObject(targetType, val);
            }

            // some special types
            if (targetType == typeof(Guid))
                return Guid.Parse(rawValue.ToString()!);

            if (targetType == typeof(string))
                return rawValue.ToString();

            return Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Reads data from the workbook and converts it to a List of the specified class type.
        /// </summary>
        /// <typeparam name="T">The class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param>
        /// <param name="sheetIndex">The index of the sheet to read from (default is 0).</param>
        /// <returns>A List of T populated from the sheet.</returns>
        public static List<T> Read<T>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T : class
        {
            var type = typeof(T);

            if (type == typeof(string) || type == typeof(Enum))
                throw new NotSupportedException($"Type '{type.Name}' is not supported. Use a class type only.");

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
                throw new NotSupportedException($"Type '{type.Name}' is a collection. Only single class types are supported.");

            ISheet sheet = wb.GetSheetAt(sheetIndex);
            var list = new List<T>();

            //Title
            var headerRow = sheet.GetRow(0);
            if (headerRow == null) return list;

            bool isDynamic = type == typeof(object) || type == typeof(System.Dynamic.ExpandoObject);

            Dictionary<int, PropertyInfo?> cellNums = [];
            var titles = isDynamic ? [] : GetTitles(type);
            int x = 1;
            for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
            {
                var headerValue = headerRow.GetCell(i).StringCellValue.Trim();
                if (isDynamic)
                {
                    cellNums[i] = typeof(string).GetProperties()[0];
                    var key = NormalizeKey(headerValue);
                    if (titles.ContainsValue(key))
                    {
                        key += x.ToString();
                        x++;
                    }
                    titles[i.ToString()] = key;
                }
                else if (!string.IsNullOrEmpty(headerValue) && titles.ContainsValue(headerValue))
                    cellNums[i] = type.GetProperty(headerValue) ?? type.GetProperty(titles.FirstOrDefault(q => q.Value == headerValue).Key);
            }

            for (int i = firstRow; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                object item;
                if (isDynamic)
                    item = new System.Dynamic.ExpandoObject();
                else
                    item = Activator.CreateInstance<T>()!;

                var dict = isDynamic ? (IDictionary<string, object?>)item : null;

                foreach (var col in cellNums)
                {
                    var cell = row.GetCell(col.Key);
                    if (cell == null)
                        continue;
                    var p = col.Value;
                    if (p != null)
                    {
                        var result = ConvertCellValue(isDynamic ? typeof(object) : p.PropertyType, cell);
                        if (isDynamic)
                            dict![titles[col.Key.ToString()]] = result;
                        else
                        {
                            if (result != null)
                                p.SetValue(item, result, null);
                            else if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                p.SetValue(item, null, null);
                            else if (Type.GetTypeCode(p.PropertyType) == TypeCode.String)
                                p.SetValue(item, null, null);
                            else
                                throw new InvalidCastException($"Convert {p.Name} get error,value:{result}，model type:{p.PropertyType.Name},excel type {cell?.CellType}.");
                        }
                    }
                }
                list.Add((T)item);
            }
            return list;

            static string NormalizeKey(string? key)
            {
                if (string.IsNullOrWhiteSpace(key)) return "_";

                var cleaned = Regex.Replace(key, @"[^\w\u4e00-\u9fa5]", "_");

                cleaned = Regex.Replace(cleaned, "_{2,}", "_");

                cleaned = cleaned.Trim('_');

                if (string.IsNullOrEmpty(cleaned) || char.IsDigit(cleaned[0]))
                    cleaned = "_" + cleaned;

                return cleaned;
            }
        }

        /// <summary>
        /// Reads data from the workbook and converts it to two Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing two Lists populated from the sheet.</returns>
        public static (List<T1>, List<T2>) Read<T1, T2>(this IWorkbook wb, int firstRow = 1) where T1 : class where T2 : class
            => (wb.Read<T1>(firstRow, 0), wb.Read<T2>(firstRow, 1));

        /// <summary>
        /// Reads data from the workbook and converts it to three Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing three Lists populated from the sheet.</returns>
        public static (List<T1>, List<T2>, List<T3>) Read<T1, T2, T3>(this IWorkbook wb, int firstRow = 1) where T1 : class where T2 : class where T3 : class
            => (wb.Read<T1>(firstRow, 0), wb.Read<T2>(firstRow, 1), wb.Read<T3>(firstRow, 2));

        /// <summary>
        /// Reads data from the workbook and converts it to four Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing four Lists populated from the sheet.</returns>
        public static (List<T1>, List<T2>, List<T3>, List<T4>) Read<T1, T2, T3, T4>(this IWorkbook wb, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class
            => (wb.Read<T1>(firstRow, 0), wb.Read<T2>(firstRow, 1), wb.Read<T3>(firstRow, 2), wb.Read<T4>(firstRow, 3));

        /// <summary>
        /// Reads data from the workbook and converts it to five Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing five Lists populated from the sheet.</returns>
        public static (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>) Read<T1, T2, T3, T4, T5>(this IWorkbook wb, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class
            => (wb.Read<T1>(firstRow, 0), wb.Read<T2>(firstRow, 1), wb.Read<T3>(firstRow, 2), wb.Read<T4>(firstRow, 3), wb.Read<T5>(firstRow, 4));

        /// <summary>
        /// Reads data from the workbook and converts it to six Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <typeparam name="T6">The sixth class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing six Lists populated from the sheet.</returns>
        public static (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>) Read<T1, T2, T3, T4, T5, T6>(this IWorkbook wb, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
            => (wb.Read<T1>(firstRow, 0), wb.Read<T2>(firstRow, 1), wb.Read<T3>(firstRow, 2), wb.Read<T4>(firstRow, 3), wb.Read<T5>(firstRow, 4), wb.Read<T6>(firstRow, 5));

        /// <summary>
        /// Reads data from the workbook and converts it to seven Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <typeparam name="T6">The sixth class type to convert the data to.</typeparam>
        /// <typeparam name="T7">The seventh class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing seven Lists populated from the sheet.</returns>
        public static (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>) Read<T1, T2, T3, T4, T5, T6, T7>(this IWorkbook wb, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
            => (wb.Read<T1>(firstRow, 0), wb.Read<T2>(firstRow, 1), wb.Read<T3>(firstRow, 2), wb.Read<T4>(firstRow, 3), wb.Read<T5>(firstRow, 4), wb.Read<T6>(firstRow, 5), wb.Read<T7>(firstRow, 6));

    }
}
