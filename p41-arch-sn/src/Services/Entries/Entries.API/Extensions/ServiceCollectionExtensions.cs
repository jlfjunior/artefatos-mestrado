using Entries.Application.UseCases.CreateEntry;
using Entries.Application.UseCases.GetEntriesByDate;
using Entries.Domain.Interfaces;
using Entries.Domain.Repositories;
using Entries.Infrastructure.Messaging;
using Entries.Infrastructure.Persistence;
using Entries.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Entries.API.Extensions
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
            services.AddScoped<IEntryRepository, EntryRepository>();
            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.AddScoped<CreateEntryHandler>();
            services.AddScoped<GetEntriesByDateHandler>();
            return services;
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IMessagePublisher>(_ =>
                RabbitMqPublisher.CreateAsync(
                    configuration["RabbitMq:HostName"]!).GetAwaiter().GetResult());

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
