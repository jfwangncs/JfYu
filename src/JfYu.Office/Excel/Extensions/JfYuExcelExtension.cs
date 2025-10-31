using JfYu.Office.Excel.Constant;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JfYu.Office.Excel.Extensions
{
    /// <summary>
    /// The extension methods for Excel operations.
    /// </summary>
    public static class JfYuExcelExtension
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
            {
                using (var xssfWorkbook = new XSSFWorkbook())
                    return new SXSSFWorkbook(xssfWorkbook, rowAccessSize);
            }
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
        /// Reads data from the workbook and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from.</param>
        /// <param name="sheetIndex">The index of the sheet to read from.</param>
        /// <returns>The data converted to the specified type.</returns>
        public static T Read<T>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0)
        {
            var mainType = typeof(T);
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            if (mainType.IsTypeDefinition)
                throw new InvalidOperationException($"Unsupported data type {typeof(T)}.");
            if (mainType.GetGenericTypeDefinition() == typeof(List<>) || typeof(T).GetGenericTypeDefinition() == typeof(IList<>))
                return (T)sheet.GetList(mainType.GetGenericArguments()[0], firstRow);
            else if (mainType.GetGenericTypeDefinition().Name.StartsWith("Tuple"))
            {
                var tupleType = mainType.GetGenericArguments();
                Type tuple = CreateTupleType(tupleType);
                List<object> elements = [];

                for (var i = 0; i < tupleType.Length; i++)
                {
                    var item = tupleType[i];

                    if (item.GetGenericTypeDefinition() == typeof(List<>) || item.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        var sheetData = wb.GetSheetAt(i);
                        elements.Add(sheetData.GetList(item.GetGenericArguments()[0], firstRow));
                    }
                }
                return (T)CreateTupleInstance(tuple, [.. elements]);
            }
            else
                throw new InvalidOperationException($"Unsupported data type {typeof(T)}.");
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
        /// Converts the value of a cell to the specified target type.
        /// </summary>
        /// <param name="target">The target type to convert the cell value to.</param>
        /// <param name="cell">The cell to get the value from.</param>
        /// <returns>The converted value.</returns>

        public static object? ConvertCellValue(Type target, ICell cell)
        {
            string? result;

            if (cell == null)
                return default;

            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        result = cell.DateCellValue?.ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        result = cell.NumericCellValue.ToString();
                    break;

                case CellType.String:
                    result = cell.StringCellValue;
                    break;

                case CellType.Blank:
                    result = null;
                    break;

                case CellType.Boolean:
                    result = cell.BooleanCellValue.ToString();
                    break;

                case CellType.Formula:
                    if (cell.CachedFormulaResultType == CellType.Numeric)
                        result = cell.NumericCellValue.ToString();
                    else if (cell.CachedFormulaResultType == CellType.String)
                        result = cell.StringCellValue;
                    else
                        throw new InvalidOperationException("Formula resulted in an error.");
                    break;

                default:
                    throw new ArgumentException("unknown cell type,{CellType}", cell.CellType.ToString());
            }

            if (result == null)
                return default;

            if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                target = target.GetGenericArguments()[0];
            }
            if (target.IsInstanceOfType(result))
                return result;
            // enums
            if (target.IsEnum)
            {
                if (result is string s)
                    return Enum.Parse(target, s, ignoreCase: true);
                // convert underlying numeric and then to enum
                var underlying = Enum.GetUnderlyingType(target);
                var val = Convert.ChangeType(result, underlying, CultureInfo.InvariantCulture);
                return Enum.ToObject(target, val);
            }

            // some special types
            if (target == typeof(Guid))
                return Guid.Parse(result.ToString());

            return Type.GetTypeCode(target) switch
            {
                TypeCode.Int16 => Convert.ToInt16(result, CultureInfo.InvariantCulture),
                TypeCode.Int32 => Convert.ToInt32(result, CultureInfo.InvariantCulture),
                TypeCode.Int64 => Convert.ToInt64(result, CultureInfo.InvariantCulture),
                TypeCode.UInt16 => Convert.ToUInt16(result, CultureInfo.InvariantCulture),
                TypeCode.UInt32 => Convert.ToUInt32(result, CultureInfo.InvariantCulture),
                TypeCode.UInt64 => Convert.ToUInt64(result, CultureInfo.InvariantCulture),
                TypeCode.Double => Convert.ToDouble(result, CultureInfo.InvariantCulture),
                TypeCode.Single => Convert.ToSingle(result, CultureInfo.InvariantCulture),
                TypeCode.Decimal => Convert.ToDecimal(result, CultureInfo.InvariantCulture),
                TypeCode.Boolean => Convert.ToBoolean(result, CultureInfo.InvariantCulture),
                TypeCode.DateTime => Convert.ToDateTime(result, CultureInfo.InvariantCulture),
                TypeCode.SByte => Convert.ToSByte(result, CultureInfo.InvariantCulture),
                TypeCode.Byte => Convert.ToByte(result, CultureInfo.InvariantCulture),
                _ => result,
            };

        }

        /// <summary>
        /// Creates an instance of a tuple type with the specified arguments.
        /// </summary>
        /// <param name="tupleType">The type of the tuple to create.</param>
        /// <param name="args">The arguments to pass to the tuple constructor.</param>
        /// <returns>The created tuple instance.</returns>
        internal static object CreateTupleInstance(Type tupleType, object[] args)
        {
            ConstructorInfo? constructor = tupleType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, Array.ConvertAll(args, arg => arg.GetType()), null);
            return constructor!.Invoke(args);
        }

        /// <summary>
        /// Creates a tuple type with the specified generic type arguments.
        /// </summary>
        /// <param name="types">An array of types to be used as generic type arguments for the tuple.</param>
        /// <returns>A Type object representing the constructed tuple type.</returns>
        internal static Type CreateTupleType(Type[] types)
        {
            return types.Length switch
            {
                1 => typeof(Tuple<>).MakeGenericType(types[0]),
                2 => typeof(Tuple<,>).MakeGenericType(types[0], types[1]),
                3 => typeof(Tuple<,,>).MakeGenericType(types[0], types[1], types[2]),
                4 => typeof(Tuple<,,,>).MakeGenericType(types[0], types[1], types[2], types[3]),
                5 => typeof(Tuple<,,,,>).MakeGenericType(types[0], types[1], types[2], types[3], types[4]),
                6 => typeof(Tuple<,,,,,>).MakeGenericType(types[0], types[1], types[2], types[3], types[4], types[5]),
                7 => typeof(Tuple<,,,,,,>).MakeGenericType(types[0], types[1], types[2], types[3], types[4], types[5], types[6]),
                _ => throw new InvalidOperationException("Only Support up to 7 elements."),
            };
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