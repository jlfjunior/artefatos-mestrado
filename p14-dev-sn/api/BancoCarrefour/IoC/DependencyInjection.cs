using Data.Context;
using Data.Repositories;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Services;

namespace IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICashEntryService, CashEntryService>();
            services.AddScoped<ICashEntryRepository, CashEntryRepository>();
            return services;
        }

        public static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {            
            services.AddDbContext<CarrefourContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CarrefourConnection")
                )
            );

            return services;
        }
    }
}
