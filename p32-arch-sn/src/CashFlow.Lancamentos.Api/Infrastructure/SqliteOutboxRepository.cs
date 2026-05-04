using System.Globalization;
using CashFlow.Shared;
using Microsoft.Data.Sqlite;

namespace CashFlow.Lancamentos.Api.Infrastructure;

public sealed class SqliteOutboxRepository : IOutboxRepository
{
    private readonly LancamentosStoragePaths _storagePaths;

    public SqliteOutboxRepository(LancamentosStoragePaths storagePaths)
    {
        _storagePaths = storagePaths;
    }

    public async Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var items = new List<OutboxEnvelope>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_id, payload, created_at
            FROM outbox_events
            WHERE published_at IS NULL
            ORDER BY created_at
            LIMIT $batchSize;
            """;
        command.Parameters.AddWithValue("$batchSize", batchSize);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OutboxEnvelope(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                DateTimeOffset.Parse(reader.GetString(2), CultureInfo.InvariantCulture)));
        }

        return items;
    }

    public async Task MarkAsPublishedAsync(Guid eventId, DateTimeOffset publishedAt, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE outbox_events
            SET published_at = $publishedAt
            WHERE event_id = $eventId;
            """;
        command.Parameters.AddWithValue("$publishedAt", publishedAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$eventId", eventId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private SqliteConnection CreateConnection()
        => new($"Data Source={_storagePaths.LancamentosDbPath}");
}
