using CashFlow.BuildingBlocks.Behaviors;
using CashFlow.Consolidation.Application.CommandHandlers;
using CashFlow.Consolidation.Infrastructure;
using CashFlow.Consolidation.Infrastructure.Persistence;
using CashFlow.Consolidation.Worker;
using CashFlow.Consolidation.Worker.Consumers;
using CashFlow.Consolidation.Worker.Messaging;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessLaunchRegisteredEventCommandHandler).Assembly);
});

builder.Services.AddValidatorsFromAssemblyContaining<ProcessLaunchRegisteredEventCommandHandler>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.Configure<RabbitMqOptions>(options =>
{
    var section = builder.Configuration.GetSection("RabbitMq");
    options.HostName = section["HostName"] ?? options.HostName;
    options.Port = int.TryParse(section["Port"], out var port) ? port : options.Port;
    options.UserName = section["UserName"] ?? options.UserName;
    options.Password = section["Password"] ?? options.Password;
    options.VirtualHost = section["VirtualHost"] ?? options.VirtualHost;
    options.ExchangeName = section["ExchangeName"] ?? options.ExchangeName;
    options.QueueName = section["QueueName"] ?? options.QueueName;
    options.RoutingKey = section["RoutingKey"] ?? options.RoutingKey;
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<LaunchRegisteredConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
    dbContext.Database.Migrate();
}

host.Run();
