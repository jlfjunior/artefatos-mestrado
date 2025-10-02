using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Context;
using FluxoCaixa.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluxoCaixa.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("FluxoCaixa")); // Ou UseSqlServer(connectionString)

            services.AddScoped<ILancamentoRepository, LancamentoRepository>();

            return services;
        }
    }
}
