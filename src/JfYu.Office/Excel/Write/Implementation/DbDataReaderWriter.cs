using JfYu.Office.Excel.Extensions;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace JfYu.Office.Excel.Write.Implementation
{
    /// <summary>
    /// The DbDataReader writer.
    /// </summary>
    /// <param name="configuration">The JfYuExcel configuration </param>
    public class DbDataReaderWriter(IOptionsMonitor<JfYuExcelOption> configuration) : JfYuWriterBase<DbDataReader>
    {
        private readonly JfYuExcelOption _configuration = configuration.CurrentValue;

        /// <inheritdoc/>
        protected override void WriteDataToWorkbook(IWorkbook workbook, DbDataReader source, Dictionary<string, string>? titles = null, Action<int>? callback = null)
        {
            if (source.IsClosed)
                throw new DataException("Data Connection is closed.");
            if (titles is null || titles.Count <= 0)
                throw new ArgumentNullException(nameof(titles));

            var sheetName = $"sheet{workbook.NumberOfSheets}";
            var sheet = workbook.CreateSheet(sheetName);
            sheet.AddTitle(titles);
            int sheetWriteRowIndex = 1;
            int writedCount = 0;

            while (source.Read())
            {
                var dataRow = sheet.CreateRow(sheetWriteRowIndex);
                var columnIndex = 0;
                foreach (var title in titles)
                {
                    var cell = dataRow.CreateCell(columnIndex);
                    var value = source[title.Key];
                    SetValue(title.Value.GetType(), value, cell);
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
            source.Close();
        }
    }
}