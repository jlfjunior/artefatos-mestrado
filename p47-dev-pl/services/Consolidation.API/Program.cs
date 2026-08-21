using Consolidation.API.Application.Services;
using Consolidation.API.Infrastructure.Data;
using Consolidation.API.Infrastructure.MessageConsumers;
using Consolidation.API.src.Application.Interface;
using Consolidation.API.src.Domain.Interfaces;
using Consolidation.API.src.Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);


// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/consolidation-api-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Service", "Consolidation.API")
    .CreateLogger();

builder.Host.UseSerilog();


// Configurar Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<ConsolidationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));


// Configurar MassTransit + RabbitMQ
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitPort = builder.Configuration.GetValue<ushort?>("RabbitMQ:Port") ?? 5672;
var rabbitUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<TransactionCreatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, rabbitPort, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        // Retry 
        cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(context);
    });
});

// Registrar Serviços
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IConsolidationService, ConsolidationService>();

// Configurar Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

// Configurar Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Consolidation API", Version = "v1" });
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();

// Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var response = new HealthResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.ToDictionary(x => x.Key, x => x.Value.Status.ToString())
        };
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
});

Log.Information("Consolidation API iniciando...");
await app.RunAsync();
