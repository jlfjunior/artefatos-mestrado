using CashFlow.Application;
using CashFlow.Consumer.Consolidation;
using CashFlow.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddConsumerInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ConsolidationConsumer>();

var host = builder.Build();
host.Run();
