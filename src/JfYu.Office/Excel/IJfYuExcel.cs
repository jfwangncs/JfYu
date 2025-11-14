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
        /// Writes data to an Excel file.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="source">The data source.</param>
        /// <param name="filePath">The file path where the Excel file will be saved.</param>
        /// <param name="titles">Optional dictionary of column titles.</param>
        /// <param name="excelOption">Specifies the write operation to perform (e.g., None, Append).</param>
        /// <param name="callback">Optional callback action to report progress.</param> 
        void Write<T>(T source, string filePath, Dictionary<string, string>? titles = null, JfYuExcelOptions? excelOption = null, Action<int>? callback = null) where T : notnull;

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

        #region ReadExcel
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
        /// Reads data from an Excel file stream and converts it to two Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing two Lists populated from the sheet.</returns>
        (List<T1>, List<T2>) Read<T1, T2>(Stream stream, int firstRow = 1) where T1 : class where T2 : class;

        /// <summary>
        /// Reads data from an Excel file and converts it to two Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing two Lists populated from the sheet.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        (List<T1>, List<T2>) Read<T1, T2>(string filePath, int firstRow = 1) where T1 : class where T2 : class;

        /// <summary>
        /// Reads data from an Excel file stream and converts it to three Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing three Lists populated from the sheet.</returns>
        (List<T1>, List<T2>, List<T3>) Read<T1, T2, T3>(Stream stream, int firstRow = 1) where T1 : class where T2 : class where T3 : class;

        /// <summary>
        /// Reads data from an Excel file and converts it to three Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing three Lists populated from the sheet.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        (List<T1>, List<T2>, List<T3>) Read<T1, T2, T3>(string filePath, int firstRow = 1) where T1 : class where T2 : class where T3 : class;

        /// <summary>
        /// Reads data from an Excel file stream and converts it to four Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing four Lists populated from the sheet.</returns>
        (List<T1>, List<T2>, List<T3>, List<T4>) Read<T1, T2, T3, T4>(Stream stream, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class;

        /// <summary>
        /// Reads data from an Excel file and converts it to four Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing four Lists populated from the sheet.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        (List<T1>, List<T2>, List<T3>, List<T4>) Read<T1, T2, T3, T4>(string filePath, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class;

        /// <summary>
        /// Reads data from an Excel file stream and converts it to five Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing five Lists populated from the sheet.</returns>
        (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>) Read<T1, T2, T3, T4, T5>(Stream stream, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class;

        /// <summary>
        /// Reads data from an Excel file and converts it to five Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing five Lists populated from the sheet.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>) Read<T1, T2, T3, T4, T5>(string filePath, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class;

        /// <summary>
        /// Reads data from an Excel file stream and converts it to six Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <typeparam name="T6">The sixth class type to convert the data to.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing six Lists populated from the sheet.</returns>
        (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>) Read<T1, T2, T3, T4, T5, T6>(Stream stream, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;

        /// <summary>
        /// Reads data from an Excel file and converts it to six Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <typeparam name="T6">The sixth class type to convert the data to.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing six Lists populated from the sheet.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>) Read<T1, T2, T3, T4, T5, T6>(string filePath, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;

        /// <summary>
        /// Reads data from an Excel file stream and converts it to seven Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <typeparam name="T6">The sixth class type to convert the data to.</typeparam>
        /// <typeparam name="T7">The seventh class type to convert the data to.</typeparam>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing seven Lists populated from the sheet.</returns>
        (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>) Read<T1, T2, T3, T4, T5, T6, T7>(Stream stream, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;

        /// <summary>
        /// Reads data from an Excel file and converts it to seven Lists of the specified class types.
        /// </summary>
        /// <typeparam name="T1">The first class type to convert the data to.</typeparam>
        /// <typeparam name="T2">The second class type to convert the data to.</typeparam>
        /// <typeparam name="T3">The third class type to convert the data to.</typeparam>
        /// <typeparam name="T4">The fourth class type to convert the data to.</typeparam>
        /// <typeparam name="T5">The fifth class type to convert the data to.</typeparam>
        /// <typeparam name="T6">The sixth class type to convert the data to.</typeparam>
        /// <typeparam name="T7">The seventh class type to convert the data to.</typeparam>
        /// <param name="filePath">The file path of the Excel file.</param>
        /// <param name="firstRow">The first row to start reading from (default is 1).</param> 
        /// <returns>A tuple containing seven Lists populated from the sheet.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        (List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>) Read<T1, T2, T3, T4, T5, T6, T7>(string filePath, int firstRow = 1) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;

        #endregion
    }
}