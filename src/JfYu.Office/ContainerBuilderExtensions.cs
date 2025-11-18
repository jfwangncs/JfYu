using JfYu.Office.Excel;
using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Write.Implementation;
using JfYu.Office.Excel.Write.Interface;
using JfYu.Office.Word;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;

namespace JfYu.Office
{
    /// <summary>
    /// Provides extension methods for registering JfYu.Office services in the dependency injection container.
    /// Simplifies the setup of Excel and Word document processing capabilities.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Adds JfYu Excel services to the specified IServiceCollection.
        /// Registers all required services for Excel read/write operations including support for List, DataTable, DbDataReader, and dynamic objects.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="setupAction">An optional action to configure JfYuExcelOptions. Use this to set SheetMaxRecord, RowAccessSize, or AllowAppend defaults.</param>
        /// <returns>The IServiceCollection with the added services for method chaining.</returns>
        /// <example>
        /// <code>
        /// // Basic registration
        /// services.AddJfYuExcel();
        /// 
        /// // With custom options
        /// services.AddJfYuExcel(options =>
        /// {
        ///     options.SheetMaxRecord = 500000;
        ///     options.RowAccessSize = 100;
        ///     options.AllowAppend = WriteOperation.Append;
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddJfYuExcel(this IServiceCollection services, Action<JfYuExcelOptions>? setupAction = null)
        {
            services.Configure<JfYuExcelOptions>(opts => setupAction?.Invoke(opts));
            services.AddScoped<IJfYuExcel, JfYuExcel>();
            services.AddScoped<IJfYuExcelWriterFactory, JfYuExcelWriterFactory>();
            services.AddScoped<IJfYuExcelWrite<DataTable>, DataTableWriter>();
            services.AddScoped<IJfYuExcelWrite<DbDataReader>, DbDataReaderWriter>();
            services.AddScoped(typeof(IJfYuExcelWrite<>), typeof(ListWriter<>));
            return services;
        }

        /// <summary>
        /// Adds JfYu Word services to the specified IServiceCollection.
        /// Registers services for Word document template processing and placeholder replacement.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <returns>The IServiceCollection with the added services for method chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddJfYuWord();
        /// </code>
        /// </example>
        public static IServiceCollection AddJfYuWord(this IServiceCollection services)
        {
            services.AddScoped<IJfYuWord, JfYuWord>();
            return services;
        }
    }
}