using Consolidate.Business.Contracts;
using Consolidate.Business.UseCase.AddConsolidateTransaction;
using Consolidate.Business.UseCase.GetConsolidateByDateUseCase;
using Consolidate.Data.Mapping;
using Consolidate.Data.Repository;
using Extensions.OpenTelemetry;
using Foundation.Business.DomainNotitications;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Extensions.Extensions.MongoDb;
using Extensions.Log;
using Extensions.Authentication;
using Extensions.HealthCheck;
using Foundation.Settings;
using Foundation.Business.BehaviorHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( o =>
{
    o.AddSecurityDefinition("Bearer", AutenticationExtension.OpenApiSecurityScheme);
    o.AddSecurityRequirement(AutenticationExtension.OpenApiSecurityRequirement);
});

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);

builder.AddSerilog();

builder.Services
    .AddAuthentication(appSettings)
    .AddOpenTelemetry(appSettings)
    .AddHealthChecks(appSettings)
    .AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(GetConsolidateByDateInputDto).Assembly, 
            typeof(GetConsolidateByDateUseCase).Assembly);

        cfg.RegisterServicesFromAssemblies(typeof(AddConsolidateTransactionInputDto).Assembly, typeof(AddConsolidateTransaction).Assembly);


    })
    .AddAutoMapper(typeof(MappingProfile))
    .AddScoped<DomainNotificationContext, ConsolidateTransactionDomainNotificationContext>()
    .AddScoped<IConsolidateTransactionRepository, ConsolidateTransactionMongoRepository>()
    .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
    .AddMongoDb(appSettings)
    .AddScoped<IMongoDatabase>(provider =>
    {
        var client = provider.GetService<IMongoClient>();
        return client.GetDatabase("Consolidate");
    })
    .AddMassTransit(x =>
    {
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host("rabbitmq2", "/", h => {
                h.Username("guest");
                h.Password("guest");
            });
            cfg.ConfigureEndpoints(ctx);
        });

        x.AddConsumer<TransactionCreatedConsumer>();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePrometheusHealthChecks();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapLogin(appSettings);

app.MapGet("/consolidate",  async (
    [FromQuery] DateTime Date,
    IMediator mediator,
    DomainNotificationContext domainNotificaionContext,
    CancellationToken cancellationToken) => {

        var result = await mediator.Send(new GetConsolidateByDateInputDto(DateOnly.FromDateTime(Date)), cancellationToken);
        if (!domainNotificaionContext.IsValid)
        {
            return Results.BadRequest(domainNotificaionContext.Notifications);
        }
        return Results.Created("/", result);
    })
.WithOpenApi();

app.Run();