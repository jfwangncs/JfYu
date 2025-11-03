using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JfYu.Office.Excel.Extensions
{
    public static partial class JfYuExcelExtension
    {
        /// <summary>
        /// Reads data from the workbook and converts it to a List of the specified class type.
        /// </summary>
        /// <typeparam name="T">The class type to convert the data to.</typeparam>
        /// <param name="wb">The workbook to read from.</param>
        /// <param name="firstRow">The first row to start reading from.</param>
        /// <param name="sheetIndex">The index of the sheet to read from.</param>
        /// <returns>A List of T populated from the sheet.</returns>
        public static List<T> Read<T>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T : class
        {
            var mainType = typeof(T);
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            var list = new List<T>();

            //Title
            var headerRow = sheet.GetRow(0);
            if (headerRow == null) return list;

            int cellCount = headerRow.LastCellNum;
            var titles = GetTitles(mainType);
            Dictionary<int, string> cellNums = new();
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                var c = headerRow.GetCell(i);
                var headerValue = c?.StringCellValue?.Trim();
                if (!string.IsNullOrEmpty(headerValue) && titles.ContainsValue(headerValue))
                    cellNums[i] = headerValue;
            }

            for (int i = firstRow; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                var item = Activator.CreateInstance<T>();
                foreach (var c in cellNums)
                {
                    var cell = row.GetCell(c.Key);
                    var p = mainType.GetProperty(c.Value) ?? mainType.GetProperty(titles.FirstOrDefault(q => q.Value == c.Value).Key);
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
                            throw new InvalidCastException($"Convert {p.Name} get error,value:{result}£¬model type:{p.PropertyType.Name},excel type {cell?.CellType}.");
                    }
                }
                list.Add(item);
            }
            return list;
        }


        public static (List<T1>, List<T2>) Read<T1, T2>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T1 : class where T2 : class
            => (wb.Read<T1>(firstRow, sheetIndex), wb.Read<T2>(firstRow, sheetIndex));

        public static (List<T1>, List<T2>, List<T3>) Read<T1, T2, T3>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T1 : class where T2 : class where T3 : class
            => (wb.Read<T1>(firstRow, sheetIndex), wb.Read<T2>(firstRow, sheetIndex), wb.Read<T3>(firstRow, sheetIndex));

        public static (List<T1>, List<T2>, List<T3>, List<T4>) Read<T1, T2, T3, T4>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T1 : class where T2 : class where T3 : class where T4 : class
            => (wb.Read<T1>(firstRow, sheetIndex), wb.Read<T2>(firstRow, sheetIndex), wb.Read<T3>(firstRow, sheetIndex), wb.Read<T4>(firstRow, sheetIndex));

        public static (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>) Read<T1, T2, T3, T4, T5>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class
            => (wb.Read<T1>(firstRow, sheetIndex), wb.Read<T2>(firstRow, sheetIndex), wb.Read<T3>(firstRow, sheetIndex), wb.Read<T4>(firstRow, sheetIndex), wb.Read<T5>(firstRow, sheetIndex));

        public static (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>) Read<T1, T2, T3, T4, T5, T6>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
            => (wb.Read<T1>(firstRow, sheetIndex), wb.Read<T2>(firstRow, sheetIndex), wb.Read<T3>(firstRow, sheetIndex), wb.Read<T4>(firstRow, sheetIndex), wb.Read<T5>(firstRow, sheetIndex), wb.Read<T6>(firstRow, sheetIndex));

        public static (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>) Read<T1, T2, T3, T4, T5, T6, T7>(this IWorkbook wb, int firstRow = 1, int sheetIndex = 0) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
            => (wb.Read<T1>(firstRow, sheetIndex), wb.Read<T2>(firstRow, sheetIndex), wb.Read<T3>(firstRow, sheetIndex), wb.Read<T4>(firstRow, sheetIndex), wb.Read<T5>(firstRow, sheetIndex), wb.Read<T6>(firstRow, sheetIndex), wb.Read<T7>(firstRow, sheetIndex));

    }
}
