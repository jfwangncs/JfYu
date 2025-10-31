using JfYu.Office.Excel.Write.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JfYu.Office.Excel.Write.Implementation
{
    /// <summary>
    /// Factory class for creating instances of Excel writers.
    /// </summary>
    public class JfYuExcelWriterFactory(IServiceProvider serviceProvider) : IJfYuExcelWriterFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        /// <summary>
        /// Gets an instance of the Excel writer for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data to be written to Excel.</typeparam>
        /// <returns>An instance of <see cref="IJfYuExcelWrite{T}"/>.</returns>
        public IJfYuExcelWrite<T> GetWriter<T>()
        {
            return _serviceProvider.GetRequiredService<IJfYuExcelWrite<T>>();
        }
    }
}