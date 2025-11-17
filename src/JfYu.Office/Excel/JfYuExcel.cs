using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.Office.Excel.Write.Implementation;
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
    /// Provides functionality for reading and writing Excel and CSV files, including support for multiple sheet types
    /// and custom configuration options.
    /// </summary>
    /// <remarks>This class supports reading and writing Excel files in various formats, as well as CSV files.
    /// It allows customization through configuration and writer factories, making it suitable for a wide range of data
    /// import and export scenarios. Thread safety depends on the underlying writer factory and configuration;
    /// concurrent operations may require separate instances.</remarks>
    /// <param name="excelWriterFactory">The factory used to create Excel writer instances for handling different data types and write operations.</param>
    /// <param name="configuration">The configuration options for Excel operations. If not specified, default options are used.</param>
    public class JfYuExcel(IJfYuExcelWriterFactory? excelWriterFactory = null, IOptions<JfYuExcelOptions>? configuration = null) : IJfYuExcel
    {
        private readonly JfYuExcelOptions _configuration = configuration?.Value ?? new JfYuExcelOptions();
        private readonly IJfYuExcelWriterFactory _excelWriterFactory = excelWriterFactory ?? new DefaultExcelWriterFactory();


        /// <inheritdoc/>
        public IWorkbook CreateExcel(JfYuExcelVersion excelVersion = JfYuExcelVersion.Xlsx)
        {
            return JfYuExcelExtension.CreateExcel(excelVersion, _configuration.RowAccessSize);
        }

        /// <inheritdoc/>
        public void Write<T>(T source, string filePath, Dictionary<string, string>? titles = null, JfYuExcelOptions? excelOption = null, Action<int>? callback = null) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentException.ThrowIfNullOrEmpty(filePath);
            var writer = _excelWriterFactory.GetWriter<T>();
            writer.Write(source, filePath, excelOption ?? _configuration, titles, callback);
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
            StringBuilder rowStr = new();
            foreach (var item in source)
            {
                rowStr.Clear();
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


        #region ReadCSV
        /// <inheritdoc/>
        public List<dynamic> ReadCSV(string filePath, int firstRow = 1)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, Encoding.UTF8);

            var records = new List<dynamic>();
            List<string> headerNames = [];
            string? line;
            int rowIndex = 1;

            while ((line = sr.ReadLine()) != null)
            {
                if (rowIndex < firstRow)
                {
                    rowIndex++;
                    continue;
                }

                var fields = ParseCsvLine(line);

                if (rowIndex == firstRow)
                {
                    headerNames = ReplaceEmptyHeaders(fields);
                }
                else
                {
                    records.Add(BuildRecord(headerNames, fields));
                }

                rowIndex++;
            }

            return records;
        }

        private static List<string> ReplaceEmptyHeaders(string[] fields)
        {
            var list = new List<string>(fields);
            int counter = 1;

            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(list[i]))
                {
                    list[i] = $"Column{counter++}";
                }
            }
            return list;
        }

        private static dynamic BuildRecord(List<string> headers, string[] fields)
        {
            dynamic record = new ExpandoObject();
            var dict = (IDictionary<string, object>)record;

            for (int i = 0; i < fields.Length; i++)
            {
                string key = i < headers.Count ? headers[i] : $"Column{i + 1}";
                dict[key] = fields[i];
            }

            return record;
        }

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString().Trim('"'));
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString().Trim('"'));
            return fields.ToArray();
        }

        #endregion

        #region ReadExcel

        /// <inheritdoc/>
        public List<T> Read<T>(Stream stream, int firstRow = 1, int sheetIndex = 0) where T : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T>(wb, firstRow, sheetIndex);
        }

        /// <inheritdoc/>
        public List<T> Read<T>(string filePath, int firstRow = 1, int sheetIndex = 0) where T : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T>(wb, firstRow, sheetIndex);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>) Read<T1, T2>(Stream stream, int firstRow = 1)
            where T1 : class
            where T2 : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T1, T2>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>) Read<T1, T2>(string filePath, int firstRow = 1)
            where T1 : class
            where T2 : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T1, T2>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>) Read<T1, T2, T3>(Stream stream, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T1, T2, T3>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>) Read<T1, T2, T3>(string filePath, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T1, T2, T3>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>) Read<T1, T2, T3, T4>(Stream stream, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T1, T2, T3, T4>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>) Read<T1, T2, T3, T4>(string filePath, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T1, T2, T3, T4>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>) Read<T1, T2, T3, T4, T5>(Stream stream, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T1, T2, T3, T4, T5>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>) Read<T1, T2, T3, T4, T5>(string filePath, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T1, T2, T3, T4, T5>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>) Read<T1, T2, T3, T4, T5, T6>(Stream stream, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T1, T2, T3, T4, T5, T6>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>) Read<T1, T2, T3, T4, T5, T6>(string filePath, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T1, T2, T3, T4, T5, T6>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>) Read<T1, T2, T3, T4, T5, T6, T7>(Stream stream, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var wb = WorkbookFactory.Create(stream);
            return JfYuExcelExtension.Read<T1, T2, T3, T4, T5, T6, T7>(wb, firstRow);
        }

        /// <inheritdoc/>
        public (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>) Read<T1, T2, T3, T4, T5, T6, T7>(string filePath, int firstRow = 1)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IWorkbook wb = WorkbookFactory.Create(file);
            return JfYuExcelExtension.Read<T1, T2, T3, T4, T5, T6, T7>(wb, firstRow);
        }
        #endregion
    }
}