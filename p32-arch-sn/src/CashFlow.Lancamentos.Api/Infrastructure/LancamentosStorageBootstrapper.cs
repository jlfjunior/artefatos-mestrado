using Microsoft.Data.Sqlite;

namespace CashFlow.Lancamentos.Api.Infrastructure;

public sealed class LancamentosStorageBootstrapper
{
    private readonly LancamentosStoragePaths _storagePaths;

    public LancamentosStorageBootstrapper(LancamentosStoragePaths storagePaths)
    {
        _storagePaths = storagePaths;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_storagePaths.LancamentosDbPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_storagePaths.IntegrationDbPath)!);

        await InitializeLancamentosDatabaseAsync(cancellationToken);
        await InitializeIntegrationDatabaseAsync(cancellationToken);
    }

    private async Task InitializeLancamentosDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_storagePaths.LancamentosDbPath}");
        await connection.OpenAsync(cancellationToken);

        var commandText = """
            CREATE TABLE IF NOT EXISTS ledger_entries (
                entry_id TEXT PRIMARY KEY,
                merchant_id TEXT NOT NULL,
                business_date TEXT NOT NULL,
                entry_type TEXT NOT NULL,
                amount TEXT NOT NULL,
                description TEXT NOT NULL,
                source TEXT NOT NULL,
                idempotency_key TEXT NOT NULL,
                request_hash TEXT NOT NULL,
                created_at TEXT NOT NULL,
                UNIQUE(merchant_id, idempotency_key)
            );

            CREATE INDEX IF NOT EXISTS ix_ledger_entries_merchant_date
                ON ledger_entries(merchant_id, business_date);

            CREATE TABLE IF NOT EXISTS outbox_events (
                event_id TEXT PRIMARY KEY,
                payload TEXT NOT NULL,
                created_at TEXT NOT NULL,
                published_at TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_outbox_events_unpublished
                ON outbox_events(published_at, created_at);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
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

