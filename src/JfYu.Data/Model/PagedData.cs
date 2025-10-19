using System.Collections.Generic;

namespace JfYu.Data.Model
{
    /// <summary>
    /// Paged Data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    public class PagedData<T>
    {
        /// <summary>
        /// Total Pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Total Count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Data.
        /// </summary>
        public List<T> Data { get; set; } = [];
    }
}