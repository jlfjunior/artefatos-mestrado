using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.IdentifyServer.Api.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("IdentityServerDb");
        services.AddDbContext<IdentityServerDbContext>(options => options.UseNpgsql(cs));

        return services;
    }
}