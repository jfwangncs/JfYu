namespace JfYu.Office.Excel.Write.Interface
{
    /// <summary>
    /// Factory Interface for creating instances of Excel writers.
    /// </summary>
    public interface IJfYuExcelWriterFactory
    {
        /// <summary>
        /// Gets an instance of the Excel writer for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data to be written to Excel.</typeparam>
        /// <returns>An instance of <see cref="IJfYuExcelWrite{T}"/>.</returns>
        IJfYuExcelWrite<T> GetWriter<T>();
    }
}