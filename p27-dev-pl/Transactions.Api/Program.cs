using Foundation.Business.DomainNotitications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Business.Contracts;
using Transactions.Business.UseCase;
using Transactions.Data.Repository;
using MassTransit;
using Foundation.Business.BehaviorHandlers;
using Extensions.OpenTelemetry;
using Foundation.Settings;
using Extensions.Log;
using Extensions.Authentication;
using Microsoft.AspNetCore.Authorization;
using Transactions.Data.Mapping;
using Extensions.Extensions.MongoDb;
using MongoDB.Driver;
using Extensions.HealthCheck;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
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
            .AddMassTransit(x =>
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(appSettings.MessageBroker.Host, "/", h =>
                    {
                        h.Username(appSettings.MessageBroker.User);
                        h.Password(appSettings.MessageBroker.Password);
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
            })
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(TransactionInputDto).Assembly, typeof(AddTransactionUseCase).Assembly);
            })
            .AddScoped<DomainNotificationContext, TransactionDomainNotificationContext>()
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddMongoDb(appSettings)
            .AddAutoMapper(typeof(MappingProfile))
            .AddScoped<ITransactionRepository, TransactionMongoRepository>()                        
            .AddScoped<IMongoDatabase>(provider =>
            {
                var client = provider.GetService<IMongoClient>();
                return client.GetDatabase("Transaction");
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

app.MapPost("/transaction/add",  async (
    [FromBody] TransactionInputDto input,
    IMediator mediator,
    DomainNotificationContext domainNotificaionContext,
    CancellationToken cancellationToken) => {
        await mediator.Send(input, cancellationToken);
        if (!domainNotificaionContext.IsValid)
        {
            return Results.BadRequest(domainNotificaionContext.Notifications);
        }
        return Results.Created("/", null);
    })
.WithOpenApi();

app.Run();

