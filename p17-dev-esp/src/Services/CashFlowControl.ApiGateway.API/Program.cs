using CashFlowControl.ApiGateway.API.Configurations;
using CashFlowControl.ApiGateway.API.Configurations.ResolveDI;
using CashFlowControl.ApiGateway.API.Extensions;
using CashFlowControl.Core.Application.ResolveDI;
using CashFlowControl.Core.Infrastructure.Configurations.ResolveDI;
using CashFlowControl.Core.Infrastructure.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ConfigureKestrel.Configure(builder);

SerilogConfig.Configuration();

builder.Host.UseSerilog();

DatabaseDI.Registry(builder);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Configuration.SetBasePath("/app").AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

TokenJwtDI.RegistryConsumer(builder);
builder.Services.ConfigureSecurityModule(builder.Configuration);

//SwaggerDI.Registry(builder);

builder.Services.AddEndpointsApiExplorer();

ResolveServicesDI.RegistryServices(builder);

builder.Services.AddControllers();

builder.Services.ConfigureAuthCQRS(builder.Configuration);

ResolveRepositoriesDI.RegistryRepositories(builder);

//builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddSingleton<CustomDelegatingHandler>();
builder.Services.AddOcelot()
        .AddDelegatingHandler<CustomDelegatingHandler>();


var app = builder.Build();

app.UseCors("AllowAllOrigins");

//SwaggerConfig.Configure(app);

//app.UseSwaggerForOcelotUI(options =>
//{
//    options.PathToSwaggerGenerator = "/swagger/docs"; 
//});

app.UseSerilogRequestLogging();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseForwardedHeaders();


app.UseOcelot().Wait();

app.Run();
