using Cashflow.Management.Application.Requests.ConsolidateTransaction;

namespace Cashflow.Management.Api.Bootstrap
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

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ConsolidateTransactionHandler).Assembly));

            return services;
        }
    }
}
