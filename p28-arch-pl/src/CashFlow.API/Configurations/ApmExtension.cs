using System.Diagnostics.CodeAnalysis;
using Elastic.Apm.NetCoreAll;

namespace CashFlow.API.Configurations;

[ExcludeFromCodeCoverage]
public static class ApmExtension
{
    public static IApplicationBuilder UseApm(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseAllElasticApm(configuration);

        return app;
    }
}