using CashFlowControl.Core.Application.ResolveDI;
using CashFlowControl.Core.Infrastructure.Configurations;
using CashFlowControl.Core.Infrastructure.Configurations.ResolveDI;
using CashFlowControl.Core.Infrastructure.Logging;
using CashFlowControl.Permissions.API.Configurations;
using CashFlowControl.Permissions.API.Configurations.ResolveDI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ConfigureKestrel.Configure(builder);

SerilogConfig.Configuration();

builder.Host.UseSerilog();

DatabaseDI.Registry(builder);

TokenJwtDI.RegistryGenerate(builder);
builder.Services.ConfigureSecurityModule(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

SwaggerDI.Registry(builder);

builder.Services.AddControllers();

ResolveServicesDI.RegistryServices(builder);

builder.Services.ConfigureAuthCQRS(builder.Configuration);

ResolveRepositoriesDI.RegistryRepositories(builder);

var app = builder.Build();

await DatabaseMigrator.ApplyMigrationsAsync(app);

SwaggerConfig.Configure(app);

app.UseCors("AllowAllOrigins");

app.UseSerilogRequestLogging();

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.UseForwardedHeaders();

app.Run();
