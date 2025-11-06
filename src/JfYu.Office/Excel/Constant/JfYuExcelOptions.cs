namespace JfYu.Office.Excel.Constant
{
    /// <summary>
    /// JfYuExcelOption
    /// </summary>
    public class JfYuExcelOptions
    {
        /// <summary>
        /// SXSSF row count in memory
        /// </summary>
        public int RowAccessSize { get; set; } = 1000;

        /// <summary>
        /// One sheet max record default:1000000
        /// </summary>
        public int SheetMaxRecord { get; set; } = 1000000;

        /// <summary>
        /// Default write operation
        /// </summary>
        public WriteOperation AllowAppend { get; set; } = WriteOperation.None;
    }
}