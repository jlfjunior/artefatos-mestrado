using Microsoft.Data.Sqlite;

namespace CashFlow.Consolidado.Api.Infrastructure;

public sealed class ConsolidadoStorageBootstrapper
{
    private readonly ConsolidadoStoragePaths _storagePaths;

    public ConsolidadoStorageBootstrapper(ConsolidadoStoragePaths storagePaths)
    {
        _storagePaths = storagePaths;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_storagePaths.ConsolidadoDbPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_storagePaths.IntegrationDbPath)!);

        await InitializeConsolidadoDatabaseAsync(cancellationToken);
        await InitializeIntegrationDatabaseAsync(cancellationToken);
    }

    private async Task InitializeConsolidadoDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_storagePaths.ConsolidadoDbPath}");
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS daily_balances (
                merchant_id TEXT NOT NULL,
                business_date TEXT NOT NULL,
                total_credits TEXT NOT NULL,
                total_debits TEXT NOT NULL,
                balance TEXT NOT NULL,
                last_processed_event_id TEXT NULL,
                last_sequence_id INTEGER NOT NULL,
                updated_at TEXT NOT NULL,
                PRIMARY KEY (merchant_id, business_date)
            );

            CREATE TABLE IF NOT EXISTS processed_events (
                event_id TEXT PRIMARY KEY,
                sequence_id INTEGER NOT NULL,
                processed_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS consumer_checkpoints (
                consumer_name TEXT PRIMARY KEY,
                last_sequence_id INTEGER NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task InitializeIntegrationDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_storagePaths.IntegrationDbPath}");
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS integration_events (
                sequence_id INTEGER PRIMARY KEY AUTOINCREMENT,
                event_id TEXT NOT NULL UNIQUE,
                payload TEXT NOT NULL,
                occurred_at TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_integration_events_sequence
                ON integration_events(sequence_id);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

