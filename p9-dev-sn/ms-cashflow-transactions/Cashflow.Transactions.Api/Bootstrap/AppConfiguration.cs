using Cashflow.Transactions.Application.Mappings;
using Cashflow.Transactions.Application.Requests.GetTransactionsSummary;

namespace Cashflow.Transactions.Api.Bootstrap
{
    public static class AppConfiguration
    {
        public static IServiceCollection ConfigureApp(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DevelopmentCors", policy =>
                {
                    policy.WithOrigins("*")
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });

                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

                options.AddPolicy("ProductionCors", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTransactionsSummaryRequest).Assembly));

            services.AddAutoMapper(typeof(TransactionMappingProfile));

            return services;
        }
    }
}
