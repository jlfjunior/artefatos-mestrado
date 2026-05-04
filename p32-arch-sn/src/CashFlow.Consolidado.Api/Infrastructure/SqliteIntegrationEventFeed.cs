using System.Globalization;
using CashFlow.Shared;
using Microsoft.Data.Sqlite;

namespace CashFlow.Consolidado.Api.Infrastructure;

public sealed class SqliteIntegrationEventFeed : IIntegrationEventFeed
{
    private readonly ConsolidadoStoragePaths _storagePaths;

    public SqliteIntegrationEventFeed(ConsolidadoStoragePaths storagePaths)
    {
        _storagePaths = storagePaths;
    }

    public Task<IReadOnlyList<IntegrationEnvelope>> ReadNextAsync(
        long afterSequenceId,
        int batchSize,
        CancellationToken cancellationToken)
        => ReadAsync(
            """
            SELECT sequence_id, payload
            FROM integration_events
            WHERE sequence_id > $afterSequenceId
            ORDER BY sequence_id
            LIMIT $batchSize;
            """,
            cmd =>
            {
                cmd.Parameters.AddWithValue("$afterSequenceId", afterSequenceId);
                cmd.Parameters.AddWithValue("$batchSize", batchSize);
            },
            cancellationToken);

    public Task<IReadOnlyList<IntegrationEnvelope>> ReadAllAsync(CancellationToken cancellationToken)
        => ReadAsync(
            """
            SELECT sequence_id, payload
            FROM integration_events
            ORDER BY sequence_id;
            """,
            _ => { },
            cancellationToken);

    private async Task<IReadOnlyList<IntegrationEnvelope>> ReadAsync(
        string sql,
        Action<SqliteCommand> configure,
        CancellationToken cancellationToken)
    {
        var items = new List<IntegrationEnvelope>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure(command);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<EntryRegisteredIntegrationEvent>(
                reader.GetString(1),
                JsonDefaults.SerializerOptions);

            if (payload is null)
            {
                throw new InvalidOperationException("O evento armazenado na fila está inválido.");
            }

            items.Add(new IntegrationEnvelope(reader.GetInt64(0), payload));
        }

        return items;
    }

    private SqliteConnection CreateConnection()
        => new($"Data Source={_storagePaths.IntegrationDbPath}");
}

