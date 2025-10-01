using CashFlow.DailyConsolidated.Application.Services;
using CashFlow.DailyConsolidated.Domain.Interfaces;
using CashFlow.DailyConsolidated.Infrastructure.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379")
);

builder.Services.AddScoped<ICacheRepository, CacheRepository>();
builder.Services.AddSingleton<IEntryRepository, EntryRepository>();
builder.Services.AddScoped<IDailyConsolidatedService, DailyConsolidatedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
