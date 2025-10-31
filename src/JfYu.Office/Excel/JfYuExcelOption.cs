namespace JfYu.Office.Excel
{
    /// <summary>
    /// JfYuExcelOption
    /// </summary>
    public class JfYuExcelOption
    {
        /// <summary>
        /// SXSSF row count in memory
        /// </summary>
        public int RowAccessSize { get; set; } = 1000;

        /// <summary>
        /// One sheet max record default:1000000
        /// </summary>
        public int SheetMaxRecord { get; set; } = 1000000;
    }
}