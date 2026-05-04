using System.Text.Json.Serialization;

namespace CashFlow.Shared;

public enum EntryType
{
    Debito = 1,
    Credito = 2
}

public sealed record RegisterEntryRequest(
    [property: JsonPropertyName("comercianteId")] string MerchantId,
    [property: JsonPropertyName("dataNegocio")] DateOnly BusinessDate,
    [property: JsonPropertyName("tipo")] EntryType Type,
    [property: JsonPropertyName("valor")] decimal Amount,
    [property: JsonPropertyName("descricao")] string Description,
    [property: JsonPropertyName("origem")] string Source);

public sealed record RegisterEntryResponse(
    [property: JsonPropertyName("lancamentoId")] Guid EntryId,
    [property: JsonPropertyName("comercianteId")] string MerchantId,
    [property: JsonPropertyName("dataNegocio")] DateOnly BusinessDate,
    [property: JsonPropertyName("tipo")] EntryType Type,
    [property: JsonPropertyName("valor")] decimal Amount,
    [property: JsonPropertyName("descricao")] string Description,
    [property: JsonPropertyName("origem")] string Source,
    [property: JsonPropertyName("criadoEm")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("situacao")] string Status);

public sealed record ReprocessDailyBalanceRequest(
    [property: JsonPropertyName("comercianteId")] string MerchantId,
    [property: JsonPropertyName("dataNegocio")] DateOnly BusinessDate);

public sealed record DailyBalanceResponse(
    [property: JsonPropertyName("comercianteId")] string MerchantId,
    [property: JsonPropertyName("dataNegocio")] DateOnly BusinessDate,
    [property: JsonPropertyName("totalCreditos")] decimal TotalCredits,
    [property: JsonPropertyName("totalDebitos")] decimal TotalDebits,
    [property: JsonPropertyName("saldo")] decimal Balance,
    [property: JsonPropertyName("atualizadoEm")] DateTimeOffset? UpdatedAt,
    [property: JsonPropertyName("situacaoConsistencia")] string ConsistencyStatus);

public sealed record LedgerEntry(
    Guid EntryId,
    string MerchantId,
    DateOnly BusinessDate,
    EntryType Type,
    decimal Amount,
    string Description,
    string Source,
    string IdempotencyKey,
    DateTimeOffset CreatedAt);

public sealed record EntryRegisteredIntegrationEvent(
    Guid EventId,
    Guid EntryId,
    string MerchantId,
    DateOnly BusinessDate,
    EntryType Type,
    decimal Amount,
    string Description,
    string Source,
    DateTimeOffset OccurredAt);

public sealed record OutboxEnvelope(
    Guid EventId,
    string Payload,
    DateTimeOffset CreatedAt);

public sealed record IntegrationEnvelope(
    long SequenceId,
    EntryRegisteredIntegrationEvent Payload);

public sealed record DailyBalanceSnapshot(
    string MerchantId,
    DateOnly BusinessDate,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    Guid? LastProcessedEventId,
    long LastSequenceId,
    DateTimeOffset UpdatedAt);

public static class JsonDefaults
{
    public static System.Text.Json.JsonSerializerOptions SerializerOptions { get; } = Create();

    private static System.Text.Json.JsonSerializerOptions Create()
    {
        var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        return options;
    }
}
