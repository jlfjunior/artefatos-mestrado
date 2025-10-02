using Asp.Versioning.ApiExplorer;
using EasyCash.Abstractions;
using EasyCash.Background.Jobs;
using EasyCash.HealthCheck.Provider;
using EasyCash.Keycloak.Identity.Provider;
using EasyCash.Redis.Provider;
using EasyCash.Report.Api.ConsumerInitializer;
using EasyCash.Report.Api.Extensions;
using EasyCash.Report.Api.OpenApi;
using EasyCash.Report.Api.Policyes;
using EasyCash.Report.Command;
using EasyCash.Report.Command.Store;
using EasyCash.Report.Query;
using EasyCash.Report.Query.Store;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

EasyCash.OpenTelemetry.DependencyInjection.AddOpenTelemetryProvider(builder, "EasyCash.Report");

// Add services to the container.
builder.Services.AddApplicationReportCommand();
builder.Services.AddApplicationReportQuery();

builder.Services.AddInfrastructureReportCommandStore(builder.Configuration);
builder.Services.AddInfrastructureReportQueryStore(builder.Configuration);
builder.Services.AddInfrastructureHealthCheckProvider(builder.Configuration);
builder.Services.AddInfrastructureCachingProvider(builder.Configuration);
builder.Services.AddInfrastructureReportBackgroundJobs(builder.Configuration);
builder.Services.AddInfrastructureAuthentication(builder.Configuration);

builder.Services.AddSingleton<IIntegrationConsumerInitializer, IntegrationConsumerInitializer>();

builder.Services.ConfigureCors();
var assembliesToScan = new[]
{
    typeof(EasyCash.Report.Command.DependencyInjection).Assembly,
    typeof(EasyCash.Report.Query.DependencyInjection).Assembly,
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