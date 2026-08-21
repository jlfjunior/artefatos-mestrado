using Consolidation.Application.UseCases.GetDailyReport;
using Consolidation.Application.UseCases.ProcessEntryCreated;
using Consolidation.Domain.Interfaces;
using Consolidation.Domain.Repositories;
using Consolidation.Infrastructure.Messaging;
using Consolidation.Infrastructure.Persistence;
using Consolidation.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Consolidation.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.AddScoped<GetDailyReportHandler>();
            services.AddScoped<IProcessEntryCreatedHandler, ProcessEntryCreatedHandler>();
            return services;
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHostedService(sp =>
                new RabbitMqConsumerService(
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    configuration["RabbitMq:HostName"]!,
                    sp.GetRequiredService<ILogger<RabbitMqConsumerService>>()));

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}
