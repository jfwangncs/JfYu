namespace JfYu.Office.Excel.Constant
{
    /// <summary>
    /// Represents the different Excel file format versions.
    /// Used when creating new Excel workbooks to specify the output format.
    /// </summary>
    public enum JfYuExcelVersion
    {
        /// <summary>
        /// Office Open XML format (.xlsx) - Excel 2007 and later.
        /// This is the modern format supporting larger files and better compression.
        /// Supports up to 1,048,576 rows and 16,384 columns.
        /// </summary>
        Xlsx,

        /// <summary>
        /// Binary Interchange File Format (.xls) - Excel 97-2003.
        /// Legacy format with limitations: maximum 65,536 rows and 256 columns.
        /// Use only for compatibility with older Excel versions.
        /// </summary>
        Xls,

        /// <summary>
        /// Comma-Separated Values format (.csv).
        /// Plain text format that can be opened by Excel and other applications.
        /// Note: CSV format is handled separately via WriteCSV/ReadCSV methods, not CreateExcel.
        /// </summary>
        Csv
    }
}