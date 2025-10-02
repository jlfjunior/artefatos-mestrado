using Asp.Versioning.ApiExplorer;
using EasyCash.Abstractions;
using EasyCash.Api.ConsumerInitializer;
using EasyCash.Api.Extensions;
using EasyCash.Api.OpenApi;
using EasyCash.Api.Policyes;
using EasyCash.Background.Jobs;
using EasyCash.Command;
using EasyCash.Command.Store;
using EasyCash.HealthCheck.Provider;
using EasyCash.Keycloak.Identity.Provider;
using EasyCash.Query;
using EasyCash.Query.Store;
using EasyCash.Redis.Provider;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

EasyCash.OpenTelemetry.DependencyInjection.AddOpenTelemetryProvider(builder, "EasyCash.Api");

// Add services to the container.
builder.Services.AddApplicationCommand();
builder.Services.AddApplicationQuery();

builder.Services.AddInfrastructureCommandStore(builder.Configuration);
builder.Services.AddInfrastructureQueryStore(builder.Configuration);
builder.Services.AddInfrastructureHealthCheckProvider(builder.Configuration);
builder.Services.AddInfrastructureCachingProvider(builder.Configuration);
builder.Services.AddInfrastructureBackgroundJobs(builder.Configuration);
builder.Services.AddInfrastructureAuthentication(builder.Configuration);

builder.Services.AddSingleton<IIntegrationConsumerInitializer, IntegrationConsumerInitializer>();

builder.Services.ConfigureCors();
var assembliesToScan = new[]
{
    typeof(EasyCash.Command.DependencyInjection).Assembly,
    typeof(EasyCash.Query.DependencyInjection).Assembly,
};
builder.Services.AddCoreApplicationMessaging(assembliesToScan);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (ApiVersionDescription description in app.DescribeApiVersions())
        {
            string url = $"/swagger/{description.GroupName}/swagger.json";
            string name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
}

app.ApplyMigrations();

//app.SeedData(app.Environment);

app.UseCustomExceptionHandler();

app.UseRequestContextLogging();

app.UseCors(PoliciesConfig.CorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

namespace EasyCash.Api
{
    public partial class Program;
}
