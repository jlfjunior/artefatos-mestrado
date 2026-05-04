using System.Text.Json.Serialization;
using CashFlow.Lancamentos.Api.Application;
using CashFlow.Lancamentos.Api.Infrastructure;
using CashFlow.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var dataRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
var storagePaths = new LancamentosStoragePaths(
    Path.Combine(dataRoot, "lancamentos.db"),
    Path.Combine(dataRoot, "integration.db"));

builder.Services.AddSingleton(storagePaths);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IEntryCommandStore, SqliteEntryStore>();
builder.Services.AddSingleton<ILedgerQueryService, SqliteEntryStore>();
builder.Services.AddSingleton<IOutboxRepository, SqliteOutboxRepository>();
builder.Services.AddSingleton<IIntegrationEventBus, SqliteIntegrationEventBus>();
builder.Services.AddSingleton<LancamentosStorageBootstrapper>();
builder.Services.AddScoped<RegisterEntryHandler>();
builder.Services.AddScoped<ListEntriesHandler>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<LancamentosStorageBootstrapper>();
    await bootstrapper.InitializeAsync(CancellationToken.None);
}

app.MapGet("/health", () => Results.Ok(new { situacao = "ok", servico = "lancamentos" }));
app.MapGet("/saude", () => Results.Ok(new { situacao = "ok", servico = "lancamentos" }));

app.MapPost("/v1/lancamentos", async (
    HttpRequest httpRequest,
    RegisterEntryRequest request,
    RegisterEntryHandler handler,
    CancellationToken cancellationToken) =>
{
    var idempotencyKey = httpRequest.Headers["Chave-Idempotencia"].ToString();
    if (string.IsNullOrWhiteSpace(idempotencyKey))
    {
        idempotencyKey = httpRequest.Headers["Idempotency-Key"].ToString();
    }

    var result = await handler.HandleAsync(request, idempotencyKey, cancellationToken);

    return result.Status switch
    {
        RegisterEntryStatus.Criado => Results.Created($"/v1/lancamentos/{result.Response!.EntryId}", result.Response),
        RegisterEntryStatus.ReenvioIdempotente => Results.Ok(result.Response),
        RegisterEntryStatus.Conflito => Results.Conflict(new { erro = result.ErrorMessage }),
        _ => Results.BadRequest(new { erro = result.ErrorMessage })
    };
});

app.MapGet("/v1/lancamentos", async (
    string comercianteId,
    DateOnly dataNegocio,
    ListEntriesHandler handler,
    CancellationToken cancellationToken) =>
{
    var entries = await handler.HandleAsync(comercianteId, dataNegocio, cancellationToken);
    return Results.Ok(new { itens = entries });
});

app.Run();

public partial class Program
{
}
