using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JfYu.RabbitMQ;
#if NET8_0_OR_GREATER
using JfYu.Data.Constant;
using Microsoft.EntityFrameworkCore;
using JfYu.Data.Extension;
using JfYu.UnitTests.Models.Entity;
#endif

namespace JfYu.UnitTests
{     
    public static class Common
    {
#if NET8_0_OR_GREATER
        public static IServiceCollection AddDataContextServices(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .Build();

            var dbName = "TestDb_" + Guid.NewGuid().ToString("N");

            services.AddJfYuDbContext<DataContext>(options =>
            {
                configuration.GetSection("JfYuConnectionStrings").Bind(options);
                options.ConnectionString = dbName;
            });
            return services;
        }
        public static void Clear<T>(this DbContext context) where T : class
        {
            try
            {
                context.Set<T>().ExecuteDelete();
            }
            catch (Exception)
            {
                context.Set<T>().RemoveRange(context.Set<T>());
                context.SaveChanges();
            }
        }
#endif
        public static IServiceCollection AddRabbitMQServices(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddRabbitMQ((q, e) =>
            {
                configuration.GetSection("RabbitMQ").Bind(q);
                configuration.GetSection("RabbitMQ:MessageOption").Bind(e);
            });

            return services;
        }
    }
}