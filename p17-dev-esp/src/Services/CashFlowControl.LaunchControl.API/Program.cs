using Serilog;
using CashFlowControl.LaunchControl.Infrastructure.ResolveDI;
using CashFlowControl.LaunchControl.API.Configurations.ResolveDI;
using CashFlowControl.LaunchControl.API.Configurations;
using CashFlowControl.Core.Application.ResolveDI;
using CashFlowControl.Core.Infrastructure.Configurations.ResolveDI;
using CashFlowControl.Core.Infrastructure.Configurations;
using CashFlowControl.Core.Infrastructure.Logging;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Application.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureKestrel.Configure(builder);

SerilogConfig.Configuration();

builder.Host.UseSerilog();

DatabaseDI.Registry(builder);

TokenJwtDI.RegistryConsumer(builder);
builder.Services.ConfigureSecurityModule(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

SwaggerDI.Registry(builder);

builder.Services.AddScoped<ITransactionService, TransactionService>();
MassTransitDI.Registry(builder);

builder.Services.AddControllers();

ResolveServicesDI.RegistryServices(builder);

ResolveRepositoriesDI.RegistryRepositories(builder);

var app = builder.Build();

SwaggerConfig.Configure(app);

app.UseCors("AllowAllOrigins");

app.UseSerilogRequestLogging();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.UseForwardedHeaders();

app.Run();

public partial class Program { }