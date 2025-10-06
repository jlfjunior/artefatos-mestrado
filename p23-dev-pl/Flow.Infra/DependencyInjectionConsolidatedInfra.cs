using Flow.Core.Repositories;
using Flow.Infra.Data;
using Flow.Infra.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Infra
{
    public static class DependencyInjectionConsolidatedInfra
    {
        public static void AddDependencesConsolidatedInfra(this IServiceCollection services, IConfiguration configuration)
        {
            AddMongoDb(services, configuration);
            AddRepositories(services);
        }

        public static void AddMongoDb(IServiceCollection services, IConfiguration configuration)
        {
            var mongoSection = configuration.GetSection("MongoDb");
            services.AddSingleton(new MongoDbContext(
                    mongoSection["ConnectionString"],
                    mongoSection["Database"]));
        }

        public static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        }
    }
}
