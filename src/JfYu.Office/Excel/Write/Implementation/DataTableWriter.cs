using JfYu.Office.Excel.Extensions;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JfYu.Office.Excel.Write.Implementation
{
    /// <summary>
    /// The DataTable writer.
    /// </summary>
    /// <param name="configuration">The JfYuExcel configuration </param>
    public class DataTableWriter(IOptionsMonitor<JfYuExcelOption> configuration) : JfYuWriterBase<DataTable>
    {
        private readonly JfYuExcelOption _configuration = configuration.CurrentValue;

        /// <inheritdoc/>
        protected override void WriteDataToWorkbook(IWorkbook workbook, DataTable source, Dictionary<string, string>? titles = null, Action<int>? callback = null)
        {
            if (titles == null)
            {
                titles = [];
                foreach (DataColumn item in source.Columns)
                    titles.Add(item.ColumnName, item.ColumnName);
            }

            var sheetName = source.TableName;
            if (string.IsNullOrEmpty(sheetName))
                sheetName = $"sheet{workbook.NumberOfSheets}";
            var sheet = workbook.CreateSheet(sheetName);
            sheet.AddTitle(titles);

            int sheetWriteRowIndex = 1;
            //writed row count
            int writedCount = 0;
            //start write
            foreach (DataRow item in source.Rows)
            {
                var dataRow = sheet.CreateRow(sheetWriteRowIndex);
                var columnIndex = 0;
                foreach (var key in titles.Select(q=>q.Key))
                {
                    var cell = dataRow.CreateCell(columnIndex);
                    SetValue(item[key].GetType(), item[key], cell);
                    columnIndex++;
                }
                sheetWriteRowIndex++;
                if (sheetWriteRowIndex > _configuration.SheetMaxRecord)
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