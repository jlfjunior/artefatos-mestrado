using EasyCash.Report.Api.Middleware;
using EasyCash.Report.Api.Policyes;
using EasyCash.Report.Command.Store.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EasyCash.Report.Api.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ReportDbContext dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

        dbContext.Database.Migrate();
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        var corsConfig = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AllowedOriginsOptions>>()
            .Value;

        services.AddCors(options =>
        {
            options.AddPolicy(PoliciesConfig.CorsPolicy,
                builder =>
                {
                    builder
                        .SetIsOriginAllowed(origin =>
                        {
                            return corsConfig.Production.Any(allowed =>
                            {
                                if (allowed.Contains("*"))
                                {
                                    var pattern = allowed.Replace("*.", "([a-z0-9-]+\\.)");
                                    return System.Text.RegularExpressions.Regex.IsMatch(
                                        origin,
                                        "^" + pattern + "$",
                                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                    );
                                }
                                return origin.Equals(allowed);
                            });
                        })
                        .WithMethods(
                            "GET",
                            "POST",
                            "PUT",
                            "DELETE",
                            "OPTIONS",
                            "PATCH"
                        )
                        .WithHeaders(
                            "Authorization",
                            "Content-Type",
                            "Accept",
                            "X-Requested-With",
                            "X-Custom-Header",
                            "x-requested-with",
                            "accept",
                            "origin",
                            "access-control-request-method",
                            "access-control-request-headers",
                            "connection",
                            "user-agent"
                        )
                        .WithExposedHeaders(
                            "Content-Disposition",
                            "X-SignalR-User-Agent"
                        )
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
        });

        return services;
    }
}

public class AllowedOriginsOptions
{
    public List<string> Production { get; set; } = new List<string>();
}