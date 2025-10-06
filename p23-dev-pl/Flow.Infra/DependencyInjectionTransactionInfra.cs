using Flow.Core.Repositories;
using Flow.Core.Security.Tokens;
using Flow.Infra.Data;
using Flow.Infra.Repositories;
using Flow.Infra.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Infra
{
    public static class DependencyInjectionTransactionInfra
    {
        public static void AddDependencyInjectionTransactionInfra(this IServiceCollection services, IConfiguration configuration)
        {
            AddDbContext(services, configuration);
            AddUnityOfWork(services);
            AddRepositories(services);
            AddTokens(services, configuration);
        }

        public static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RelationalDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("RelationalDb"),
                    sqlOptions => sqlOptions.MigrationsAssembly("Flow.Infra")));
        }
        public static void AddTokens(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("JWT");
            var secretKey = jwtSection["SecretKey"];
            var timeLifeToken = jwtSection["TimeLifeTokenInMinutes"];

            services.AddScoped<ITokenGenerate>(option => new TokenGenerate(secretKey, int.Parse(timeLifeToken)));
            services.AddScoped<ITokenValidate>(option => new TokenValidate(secretKey));
        }

        public static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IUserReadOnlyRepository, UserReadOnlyRepository>();
        }

        public static void AddUnityOfWork(IServiceCollection services)
        {
            services.AddScoped<IUnityOfWork, UnityOfWork>();
        }
    }
}
