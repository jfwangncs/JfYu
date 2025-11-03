using JfYu.Office.Excel.Constant;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace JfYu.Office.Excel
{
    /// <summary>
    /// IJfYuExcel
    /// </summary>
    public interface IJfYuExcel
    {
        /// <summary>
        /// Creates a new Excel workbook.
        /// </summary>
        /// <param name="excelVersion">The version of the Excel file to create.</param>
        /// <returns>A new instance of <see cref="IWorkbook"/>.</returns>
        IWorkbook CreateExcel(JfYuExcelVersion excelVersion = JfYuExcelVersion.Xlsx);

        /// <summary>
        /// Reads data from an Excel file stream.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param>
        /// <param name="sheetIndex">The index of the sheet to read from (default is 0).</param>
        /// <returns>The data read from the Excel file.</returns>
        List<T> Read<T>(Stream stream, int firstRow = 1, int sheetIndex = 0) where T : class;

        /// <summary>
        /// Reads data from an Excel file.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param>
        /// <param name="sheetIndex">The index of the sheet to read from (default is 0).</param>
        /// <returns>The data read from the Excel file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        List<T> Read<T>(string filePath, int firstRow = 1, int sheetIndex = 0) where T : class;

        /// <summary>
        /// Updates the options for the Excel operations.
        /// </summary>
        /// <param name="updateAction">Action to update the options.</param>
        void UpdateOption(Action<JfYuExcelOption> updateAction);

        /// <summary>
        /// Writes data to an Excel file.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="source">The data source.</param>
        /// <param name="filePath">The file path where the Excel file will be saved.</param>
        /// <param name="titles">Optional dictionary of column titles.</param>
        /// <param name="writeOperation">Specifies the write operation to perform (e.g., None, Append).</param>
        /// <param name="callback">Optional callback action to report progress.</param>
        /// <returns>Returns the created or modified <see cref="IWorkbook"/> instance.</returns>
        IWorkbook Write<T>(T source, string filePath, Dictionary<string, string>? titles = null, JfYuExcelWriteOperation writeOperation = JfYuExcelWriteOperation.None, Action<int>? callback = null);

        /// <summary>
        /// Writes data to a CSV file.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="source">The data source.</param>
        /// <param name="filePath">The file path where the CSV file will be saved.</param>
        /// <param name="titles">Optional dictionary of column titles.</param>
        /// <param name="callback">Optional callback action to report progress.</param>
        /// <exception cref="Exception">Thrown when the file already exists.</exception>
        void WriteCSV<T>(List<T> source, string filePath, Dictionary<string, string>? titles = null, Action<int>? callback = null);

        /// <summary>
        /// Reads data from a CSV file.
        /// </summary>
        /// <param name="filePath">The file path of the CSV file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param>
        /// <returns>A list of dynamic objects representing the data read from the CSV file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        List<dynamic> ReadCSV(string filePath, int firstRow = 1);
    }
}