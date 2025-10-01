using CashFlow.Consumer.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

await app.RunAsync();


