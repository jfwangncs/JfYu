namespace JfYu.Office.Excel.Constant
{
    /// <summary>
    /// Configuration options for Excel operations.
    /// Controls memory usage, sheet splitting behavior, and file write operations.
    /// </summary>
    public class JfYuExcelOptions
    {
        /// <summary>
        /// Gets or sets the SXSSF row access window size (number of rows kept in memory).
        /// Default is 1000. Smaller values use less memory but may be slower for random access.
        /// Only applies when using SXSSF (streaming) mode for writing large datasets.
        /// </summary>
        public int RowAccessSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum number of records per sheet.
        /// Default is 1,000,000. When this limit is exceeded, data will automatically split into additional sheets.
        /// Maximum Excel limit is 1,048,576 rows per sheet.
        /// </summary>
        public int SheetMaxRecord { get; set; } = 1000000;

        /// <summary>
        /// Gets or sets the default write operation mode.
        /// Default is WriteOperation.None (throws error if file exists).
        /// Set to WriteOperation.Append to add data as a new sheet to existing files.
        /// </summary>
        public WriteOperation AllowAppend { get; set; } = WriteOperation.None;
    }
}