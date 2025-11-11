namespace JfYu.Office.Excel.Constant
{
    /// <summary>
    /// Defines the write operation behavior when writing to Excel files.
    /// Controls how the library handles existing files.
    /// </summary>
    public enum WriteOperation
    {
        /// <summary>
        /// No append operation. Throws an exception if the target file already exists.
        /// This is the default and safest mode to prevent accidental data overwrites.
        /// </summary>
        None,

        /// <summary>
        /// Appends data to an existing Excel file as a new sheet.
        /// If the file doesn't exist, creates a new file with the data in the first sheet.
        /// Each append operation creates a new sheet named Sheet1, Sheet2, etc.
        /// </summary>
        Append
    }
}