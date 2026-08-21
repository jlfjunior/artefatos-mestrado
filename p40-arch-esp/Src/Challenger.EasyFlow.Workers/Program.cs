using Challenger.EasyFlow.Infrastructure.Extensions;
using Challenger.EasyFlow.Workers.Processors;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TransactionProcessor>();

builder.Services
    .AddApplicationServices()
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddEventBus(builder.Configuration);

var host = builder.Build();
host.Run();
