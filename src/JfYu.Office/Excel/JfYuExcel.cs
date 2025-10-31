using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.Office.Excel.Write.Interface;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JfYu.Office.Excel
{
    /// <summary>
    /// Class for handling Excel operations such as creating, reading, and writing Excel files.
    /// </summary>
    public class JfYuExcel(IOptionsMonitor<JfYuExcelOption> configuration, IJfYuExcelWriterFactory excelWriterFactory) : IJfYuExcel
    {
        private readonly IOptionsMonitor<JfYuExcelOption> _configuration = configuration;
        private readonly IJfYuExcelWriterFactory _excelWriterFactory = excelWriterFactory;

        /// <inheritdoc/>
        public IWorkbook CreateExcel(JfYuExcelVersion excelVersion = JfYuExcelVersion.Xlsx)
        {
            return JfYuExcelExtension.CreateExcel(excelVersion, _configuration.CurrentValue.RowAccessSize);
        }

        /// <inheritdoc/>
        public IWorkbook Write<T>(T source, string filePath, Dictionary<string, string>? titles = null, JfYuExcelWriteOperation writeOperation = JfYuExcelWriteOperation.None, Action<int>? callback = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentException.ThrowIfNullOrEmpty(filePath);
            var writer = _excelWriterFactory.GetWriter<T>();
            return writer.Write(source, filePath, titles, writeOperation, callback);
        }

        /// <inheritdoc/>
        public void UpdateOption(Action<JfYuExcelOption> updateAction)
        {
            var currentOptions = _configuration.CurrentValue;
            updateAction(currentOptions);
        }

        /// <inheritdoc/>
        public T Read<T>(Stream stream, int firstRow = 1, int sheetIndex = 0)
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T>(wb, firstRow, sheetIndex);
        }

        /// <inheritdoc/>
        public T Read<T>(string filePath, int firstRow = 1, int sheetIndex = 0)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T>(wb, firstRow, sheetIndex);
        }

        /// <inheritdoc/>
        public void WriteCSV<T>(List<T> source, string filePath, Dictionary<string, string>? titles = null, Action<int>? callback = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentException.ThrowIfNullOrEmpty(filePath);
            var ext = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(ext))
                filePath += ".csv";
            else
                filePath = filePath.Replace(ext, ".csv");
            if (File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));

            using FileStream fs = File.Create(filePath);
            using StreamWriter sw = new(fs, Encoding.UTF8);
            StringBuilder str = new();
            titles ??= JfYuExcelExtension.GetTitles<T>();
            //title
            foreach (var item in titles)
            {
                str.Append(item.Value + ",");
            }
            sw.WriteLine(str.ToString().Trim(','));
            //writed row count
            int writedCount = 0;
            //start write
            foreach (var item in source)
            {
                StringBuilder rowStr = new StringBuilder();
                var columnIndex = 0;
                foreach (var key in titles.Select(q => q.Key))
                {
                    var value = typeof(T).GetProperty(key)!.GetValue(item)?.ToString() ?? "";
                    if (typeof(T).GetProperty(key)!.PropertyType == typeof(DateTime))
                        value = DateTime.Parse(value, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    if (value.Contains(','))
                        rowStr.Append($"\"{value}\",");
                    else
                        rowStr.Append(value + ",");
                    columnIndex++;
                }
                sw.WriteLine(rowStr.ToString().Trim(','));
                writedCount++;
                callback?.Invoke(writedCount);
            }
        }

        /// <inheritdoc/>
        public List<dynamic> ReadCSV(string filePath, int firstRow = 1)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader sr = new(fs, Encoding.UTF8);
            string? line;
            int rowIndex = 1;
            List<dynamic> records = [];
            List<string> headerNames = [];
            while ((line = sr.ReadLine()) != null)
            {
                if (rowIndex < firstRow)
                {
                    rowIndex++;
                    continue;
                }
                string[] fields = ParseCsvLine(line);

                if (rowIndex == firstRow)
                    headerNames = ReplaceEmptyStrings(new List<string>(fields));
                else
                {
                    dynamic record = new ExpandoObject();
                    var dict = (IDictionary<string, object>)record;
                    if (fields.Length <= headerNames.Count)
                    {
                        for (int i = 0; i < headerNames.Count && i < fields.Length; i++)
                        {
                            dict[headerNames[i]] = fields[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (i < headerNames.Count)
                                dict[headerNames[i]] = fields[i];
                            else
                                dict[$"Column{i}"] = fields[i];
                        }
                    }
                    records.Add(record);
                }
                rowIndex++;
            }
            return records;

            static List<string> ReplaceEmptyStrings(List<string> list)
            {
                string prefix = "Column";
                int counter = 1;

                for (int i = 0; i < list.Count; i++)
                {
                    if (string.IsNullOrEmpty(list[i]))
                    {
                        list[i] = $"{prefix}{counter}";
                        counter++;
                    }
                }
                return list;
            }
            static string[] ParseCsvLine(string line)
            {
                List<string> fields = [];
                bool inQuotes = false;
                StringBuilder currentField = new();

                foreach (char c in line)
                {
                    if (c == '"')
                    {
                        inQuotes = !inQuotes;
                    }
                    else if (c == ',' && !inQuotes)
                    {
                        fields.Add(currentField.ToString().Trim('"'));
                        currentField.Clear();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                if (currentField.Length > 0 || fields.Count == 0)
                {
                    fields.Add(currentField.ToString().Trim('"'));
                }

                return [.. fields];
            }
        }
    }
}