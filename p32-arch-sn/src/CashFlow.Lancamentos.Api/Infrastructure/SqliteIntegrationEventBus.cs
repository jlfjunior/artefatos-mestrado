using System.Globalization;
using CashFlow.Shared;
using Microsoft.Data.Sqlite;

namespace CashFlow.Lancamentos.Api.Infrastructure;

public sealed class SqliteIntegrationEventBus : IIntegrationEventBus
{
    private readonly LancamentosStoragePaths _storagePaths;

    public SqliteIntegrationEventBus(LancamentosStoragePaths storagePaths)
    {
        _storagePaths = storagePaths;
    }

    public async Task PublishAsync(OutboxEnvelope envelope, CancellationToken cancellationToken)
    {
        var integrationEvent = System.Text.Json.JsonSerializer.Deserialize<EntryRegisteredIntegrationEvent>(
            envelope.Payload,
            JsonDefaults.SerializerOptions);

        if (integrationEvent is null)
        {
            throw new InvalidOperationException("Não foi possível desserializar o evento de integração.");
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT OR IGNORE INTO integration_events (event_id, payload, occurred_at)
            VALUES ($eventId, $payload, $occurredAt);
            """;
        command.Parameters.AddWithValue("$eventId", envelope.EventId.ToString());
        command.Parameters.AddWithValue("$payload", envelope.Payload);
        command.Parameters.AddWithValue("$occurredAt", integrationEvent.OccurredAt.ToString("O", CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private SqliteConnection CreateConnection()
        => new($"Data Source={_storagePaths.IntegrationDbPath}");
}

