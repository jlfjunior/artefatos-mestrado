using System.Text.Json.Serialization;
using CashFlow.Consolidado.Api.Application;
using CashFlow.Consolidado.Api.Infrastructure;
using CashFlow.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var dataRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
var storagePaths = new ConsolidadoStoragePaths(
    Path.Combine(dataRoot, "consolidado.db"),
    Path.Combine(dataRoot, "integration.db"));

builder.Services.AddSingleton(storagePaths);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<DailyBalanceProjector>();
builder.Services.AddSingleton<IIntegrationEventFeed, SqliteIntegrationEventFeed>();
builder.Services.AddSingleton<IDailyBalanceProjectionStore, SqliteDailyBalanceProjectionStore>();
builder.Services.AddSingleton<ConsolidadoStorageBootstrapper>();
builder.Services.AddScoped<GetDailyBalanceHandler>();
builder.Services.AddScoped<ReprocessDailyBalanceHandler>();
builder.Services.AddHostedService<IntegrationEventConsumerWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<ConsolidadoStorageBootstrapper>();
    await bootstrapper.InitializeAsync(CancellationToken.None);
}

app.MapGet("/health", () => Results.Ok(new { situacao = "ok", servico = "consolidado" }));
app.MapGet("/saude", () => Results.Ok(new { situacao = "ok", servico = "consolidado" }));

app.MapGet("/v1/consolidado/diario", async (
    string comercianteId,
    DateOnly dataNegocio,
    GetDailyBalanceHandler handler,
    CancellationToken cancellationToken) =>
{
    var response = await handler.HandleAsync(comercianteId, dataNegocio, cancellationToken);
    return Results.Ok(response);
});

app.MapPost("/v1/consolidado/diario/reprocessamentos", async (
    ReprocessDailyBalanceRequest request,
    ReprocessDailyBalanceHandler handler,
    CancellationToken cancellationToken) =>
{
    var response = await handler.HandleAsync(request, cancellationToken);
    return Results.Accepted("/v1/consolidado/diario/reprocessamentos", response);
});

app.Run();

public partial class Program
{
}
