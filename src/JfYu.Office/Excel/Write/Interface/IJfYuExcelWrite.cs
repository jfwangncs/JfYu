using JfYu.Office.Excel.Constant;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;

namespace JfYu.Office.Excel.Write.Interface
{
    /// <summary>
    /// Interface for writing excel
    /// </summary>
    public interface IJfYuExcelWrite<T> where T : notnull
    {
        /// <summary>
        /// Writes the provided data to an Excel file.
        /// </summary>
        /// <param name="source">The data source to write to the Excel file.</param>
        /// <param name="filePath">The file path where the Excel file will be saved.</param>
        /// <param name="titles">Optional dictionary of titles for the columns.</param>
        /// <param name="writeOperation">Specifies the write operation to perform (e.g., None, Append).</param>
        /// <param name="callback">Optional callback action to report progress.</param>
        /// <returns>Returns the created or modified IWorkbook instance.</returns>
        void Write(T source, string filePath, Dictionary<string, string>? titles = null, JfYuExcelWriteOperation writeOperation = JfYuExcelWriteOperation.None, Action<int>? callback = null);
    }
}