using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluxoDeCaixa.Relatorios.API.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<AccountEntryContext>(db =>
            {
                return new AccountEntryContext(
                    configuration.GetSection("MongoDBConnection:ConnectionString").Value,
                    configuration.GetSection("MongoDBConnection:Database").Value
                );
            });

            services.AddScoped<IAccountEntryRepository, AccountEntryRepository>();

            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }
    }
}
