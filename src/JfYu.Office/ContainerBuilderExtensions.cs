using JfYu.Office.Excel;
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
    /// Provides extension methods for registering JfYu services in the dependency injection container.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Adds JfYu Excel services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="setupAction">An optional action to configure JfYuExcelOption.</param>
        /// <returns>The IServiceCollection with the added services.</returns>
        public static IServiceCollection AddJfYuExcel(this IServiceCollection services, Action<JfYuExcelOption>? setupAction = null)
        {
            var options = new JfYuExcelOption();
            services.Configure<JfYuExcelOption>(opts =>
            {
                opts.RowAccessSize = options.RowAccessSize;
                opts.SheetMaxRecord = options.SheetMaxRecord;
                setupAction?.Invoke(opts);
            });
            services.AddScoped<IJfYuExcel, JfYuExcel>();
            services.AddScoped<IJfYuExcelWriterFactory, JfYuExcelWriterFactory>();
            services.AddScoped<IJfYuExcelWrite<DataTable>, DataTableWriter>();
            services.AddScoped<IJfYuExcelWrite<DbDataReader>, DbDataReaderWriter>();
            services.AddScoped(typeof(IJfYuExcelWrite<>), typeof(ListWriter<>));
            return services;
        }

        /// <summary>
        /// Adds JfYu Word services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <returns>The IServiceCollection with the added services.</returns>
        public static IServiceCollection AddJfYuWord(this IServiceCollection services)
        {
            services.AddScoped<IJfYuWord, JfYuWord>();
            return services;
        }
    }
}