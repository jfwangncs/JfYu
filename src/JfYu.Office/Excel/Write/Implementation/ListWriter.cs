using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JfYu.Office.Excel.Write.Implementation
{
    /// <summary>
    /// Class for writing a list of data to an Excel workbook.
    /// </summary>
    /// <typeparam name="T">The type of data to be written to Excel.</typeparam>
    public class ListWriter<T> : JfYuWriterBase<T> where T : notnull
    {
        /// <inheritdoc/>
        protected override void WriteDataToWorkbook(IWorkbook workbook, T source, JfYuExcelOptions writeOperation, Dictionary<string, string>? titles = null, Action<int>? callback = null)
        {
            if (!source.GetType().IsConstructedGenericType)
                throw new NotSupportedException($"Unsupported data type {typeof(T)}.");
            var tType = source.GetType().GetGenericArguments()[0];

            if (source.GetType().GetGenericTypeDefinition().Name.StartsWith("Tuple"))
            {
                var newData = (ITuple)source;
                for (int i = 0; i < newData.Length; i++)
                {
                    if (newData[i] is IList newItemData)
                    {
                        if (newItemData.Count > writeOperation.SheetMaxRecord)
                            throw new NotSupportedException($"For write multiple sheets each sheet count need less than SheetMaxRecord:{writeOperation.SheetMaxRecord}");
                        tType = newItemData.GetType().GetGenericArguments()[0];
                        Write(newItemData.AsQueryable(), workbook, tType, writeOperation, titles, callback);
                    }
                }
                return;
            }
            IQueryable data = ConvertToQueryable(source) ?? throw new NotSupportedException($"Unsupported data type {typeof(T)}.");

            Write(data, workbook, tType, writeOperation, titles, callback);

            static IQueryable? ConvertToQueryable(T source)
            {
                var def = typeof(T).GetGenericTypeDefinition();

                if (def == typeof(IQueryable<>))
                    return (IQueryable)source;

                if (def == typeof(IEnumerable<>))
                    return ((IEnumerable)source).AsQueryable();

                if (def == typeof(IList<>) || def == typeof(List<>))
                    return ((IList)source).AsQueryable();

                return null;
            }
        }

        /// <summary>
        /// Writes data to an Excel sheet.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="workbook">The workbook to write data to.</param>
        /// <param name="tType">The type of data.</param>
        /// <param name="writeOperation">Specifies the write operation to perform (e.g., None, Append).</param>
        /// <param name="titles">Optional dictionary of column titles.</param>        /// 
        /// <param name="callback">Optional callback action to report progress.</param>
        /// <param name="needAutoCreateSheet">Indicates whether to automatically create new sheets when the row limit is reached.</param>
        /// <exception cref="InvalidOperationException">Thrown when a title's value cannot be found.</exception>
        protected void Write(IQueryable data, IWorkbook workbook, Type tType, JfYuExcelOptions writeOperation, Dictionary<string, string>? titles, Action<int>? callback = null, bool needAutoCreateSheet = true)
        {
            if (tType.IsSimpleType())
                titles ??= new Dictionary<string, string>() { { "A", "A" } };
            titles ??= JfYuExcelExtension.GetTitles(tType);

            var sheetName = $"sheet{workbook.NumberOfSheets}";
            var sheet = workbook.CreateSheet(sheetName);
            sheet.AddTitle(titles);
            int sheetWriteRowIndex = 1;
            int writedCount = 0;
            var enumerator = data.GetEnumerator();
            var properties = tType.GetProperties().ToDictionary(k => k.Name, k => k);
            while (enumerator.MoveNext())
            {
                var dataRow = sheet.CreateRow(sheetWriteRowIndex);
                var columnIndex = 0;
                foreach (var key in titles.Select(q => q.Key))
                {
                    var cell = dataRow.CreateCell(columnIndex);
                    properties.TryGetValue(key, out PropertyInfo? valueType);
                    if (tType.IsSimpleType())
                        SetValue(tType, enumerator.Current, cell);
                    else if (valueType != null)
                        SetValue(valueType.PropertyType, valueType.GetValue(enumerator.Current), cell);
                    else
                        throw new InvalidOperationException($"Can't find title {key}'s value");
                    columnIndex++;
                }
                sheetWriteRowIndex++;
                if (sheetWriteRowIndex > (writeOperation.SheetMaxRecord))
                {
                    sheetName = $"sheet{workbook.NumberOfSheets}";
                    sheet = workbook.CreateSheet(sheetName);
                    sheet.AddTitle(titles);
                    sheetWriteRowIndex = 1;
                }
                writedCount++;
                callback?.Invoke(writedCount);
            }
        }
    }
}