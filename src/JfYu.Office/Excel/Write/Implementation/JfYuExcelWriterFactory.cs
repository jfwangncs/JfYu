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
        public IJfYuExcelWrite<T> GetWriter<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<IJfYuExcelWrite<T>>();
        }
    }

    /// <summary>
    /// Provides a default implementation of the <see cref="IJfYuExcelWriterFactory"/> interface for creating Excel
    /// writer instances.
    /// </summary>
    /// <remarks>This factory creates writers that operate on lists.</remarks>
    public class DefaultExcelWriterFactory : IJfYuExcelWriterFactory
    {

        /// <summary>
        /// Creates a new writer instance for the specified data type.
        /// </summary>
        /// <typeparam name="T">The type of elements to be written. Must not be null.</typeparam>
        /// <returns>An <see cref="IJfYuExcelWrite{T}"/> instance that can be used to write data of type <typeparamref
        /// name="T"/>.</returns>
        public IJfYuExcelWrite<T> GetWriter<T>() where T : notnull
        {
            return new ListWriter<T>();
        }
    }
}