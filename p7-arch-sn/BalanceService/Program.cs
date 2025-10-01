using BalanceService.Application.Commands;
using BalanceService.Application.Queries;
using BalanceService.Domain.Events;
using BalanceService.Infrastructure.DI;
using BalanceService.Infrastructure.EventHandlers;
using BalanceService.Infrastructure.EventStore;
using BalanceService.Infrastructure.Messaging.Consumers;
using BalanceService.Infrastructure.Messaging.Publishers;
using BalanceService.Infrastructure.Options;
using BalanceService.Infrastructure.Repositories;
using BalanceService.Presentation.Dtos.Request;
using BalanceService.Presentation.Dtos.Response;
using Microsoft.AspNetCore.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using StreamTail.DI;

namespace BalanceService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddLogging();

        if (!builder.Environment.IsEnvironment("Testing"))
        {
            var factory = builder.Services
                .AddRabbitMq(builder.Configuration["RabbitMq:Host"]);

            var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();

            builder.Services
                .AddMongoDb(mongoDbOptions)
                .AddEventStore(builder.Configuration["EventStoreDb:ConnectionString"]);

            await RegisterConnection(factory);
        }

        builder.Services.AddStreamTail();

        builder.Services.AddHostedService<CreatedConsolidationConsumer>();

        builder.Services.AddScoped<ICommandHandler<CreateBalanceCommand>, CreateBalanceCommandHandler>();

        builder.Services.AddScoped<IQueryHandler<BalanceRequest, BalanceResponse>, BalanceQueryHandler>();

        builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();

        builder.Services.AddScoped<IEventHandler<BalanceCreatedEvent>, BalanceCreatedProjectionHandler>();

        builder.Services.AddScoped<IPublisherHandler<BalanceCreatedEvent>, BalanceCreatedPublisherHandler>();

        builder.Services.AddHostedService<EventStoreSubscriptionService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddExceptionHandler(options =>
        {
            options.AllowStatusCode404Response = false;

            options.ExceptionHandler = async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                                                    .CreateLogger("GlobalExceptionHandler");

                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                if (exception is BadHttpRequestException badHttpRequest)
                {
                    // handle bad HTTP request
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = $"Bad request: {badHttpRequest?.InnerException?.Message ?? badHttpRequest?.Message}"
                    });
                    return;
                }

                logger.LogError(exception, "An unhandled exception occurred.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    Message = "An unexpected error occurred. Please try again later."
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            };
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapGet("/api/balance", async (Guid accountId, IQueryHandler<BalanceRequest, BalanceResponse> handler, CancellationToken cancellationToken) =>
        {
            var request = new BalanceRequest(accountId.ToString());
            var result = await handler.HandleAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetBalance")
        .WithDescription("")
        .WithSummary("Get balance by accountId");

        await app.RunAsync();

        async Task RegisterConnection(ConnectionFactory factory)
        {
            var shouldRetry = true;
            var retries = 0;

            var brokerConnection = default(IConnection);

            while (shouldRetry)
            {
                try
                {
                    var connection = await factory.CreateConnectionAsync();

                    brokerConnection = connection;
                    shouldRetry = false;
                }
                catch (BrokerUnreachableException e)
                {
                    retries++;

                    shouldRetry = retries < 3;

                    await Task.Delay(10000);
                }
            }

            builder.Services.AddSingleton(brokerConnection);
        }
    }
}


