using Cashflow.Management.Data.AppContext;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Api.Bootstrap
{
    public static class DataConfiguration
    {
        public static IServiceCollection ConfigureData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CashflowManagementDb");
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            return services;
        }

        public static IApplicationBuilder DataConfigure(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            using var appDbContext = serviceScope.ServiceProvider.GetService<AppDbContext>();

            appDbContext?.Database.Migrate();

            return app;
        }
    }
}
