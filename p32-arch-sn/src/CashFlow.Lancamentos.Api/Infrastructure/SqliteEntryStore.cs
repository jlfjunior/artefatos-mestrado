using System.Globalization;
using CashFlow.Shared;
using Microsoft.Data.Sqlite;

namespace CashFlow.Lancamentos.Api.Infrastructure;

public sealed class SqliteEntryStore : IEntryCommandStore, ILedgerQueryService
{
    private readonly LancamentosStoragePaths _storagePaths;

    public SqliteEntryStore(LancamentosStoragePaths storagePaths)
    {
        _storagePaths = storagePaths;
    }

    public async Task<StoredEntryRecord?> FindByIdempotencyAsync(
        string merchantId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT entry_id, merchant_id, business_date, entry_type, amount, description, source, idempotency_key, created_at, request_hash
            FROM ledger_entries
            WHERE merchant_id = $merchantId AND idempotency_key = $idempotencyKey
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$merchantId", merchantId);
        command.Parameters.AddWithValue("$idempotencyKey", idempotencyKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new StoredEntryRecord(
            new LedgerEntry(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                DateOnly.Parse(reader.GetString(2), CultureInfo.InvariantCulture),
                Enum.Parse<EntryType>(reader.GetString(3)),
                decimal.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                DateTimeOffset.Parse(reader.GetString(8), CultureInfo.InvariantCulture)),
            reader.GetString(9));
    }

    public async Task SaveAsync(
        LedgerEntry entry,
        string requestHash,
        EntryRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await using var insertEntry = connection.CreateCommand();
        insertEntry.Transaction = transaction;
        insertEntry.CommandText = """
            INSERT INTO ledger_entries (
                entry_id, merchant_id, business_date, entry_type, amount, description, source, idempotency_key, request_hash, created_at
            ) VALUES (
                $entryId, $merchantId, $businessDate, $entryType, $amount, $description, $source, $idempotencyKey, $requestHash, $createdAt
            );
            """;
        insertEntry.Parameters.AddWithValue("$entryId", entry.EntryId.ToString());
        insertEntry.Parameters.AddWithValue("$merchantId", entry.MerchantId);
        insertEntry.Parameters.AddWithValue("$businessDate", entry.BusinessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        insertEntry.Parameters.AddWithValue("$entryType", entry.Type.ToString());
        insertEntry.Parameters.AddWithValue("$amount", entry.Amount.ToString(CultureInfo.InvariantCulture));
        insertEntry.Parameters.AddWithValue("$description", entry.Description);
        insertEntry.Parameters.AddWithValue("$source", entry.Source);
        insertEntry.Parameters.AddWithValue("$idempotencyKey", entry.IdempotencyKey);
        insertEntry.Parameters.AddWithValue("$requestHash", requestHash);
        insertEntry.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        await insertEntry.ExecuteNonQueryAsync(cancellationToken);

        await using var insertOutbox = connection.CreateCommand();
        insertOutbox.Transaction = transaction;
        insertOutbox.CommandText = """
            INSERT INTO outbox_events (event_id, payload, created_at, published_at)
            VALUES ($eventId, $payload, $createdAt, NULL);
            """;
        insertOutbox.Parameters.AddWithValue("$eventId", integrationEvent.EventId.ToString());
        insertOutbox.Parameters.AddWithValue("$payload", System.Text.Json.JsonSerializer.Serialize(integrationEvent, JsonDefaults.SerializerOptions));
        insertOutbox.Parameters.AddWithValue("$createdAt", integrationEvent.OccurredAt.ToString("O", CultureInfo.InvariantCulture));
        await insertOutbox.ExecuteNonQueryAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RegisterEntryResponse>> ListByBusinessDateAsync(
        string merchantId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        var entries = new List<RegisterEntryResponse>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT entry_id, merchant_id, business_date, entry_type, amount, description, source, created_at
            FROM ledger_entries
            WHERE merchant_id = $merchantId AND business_date = $businessDate
            ORDER BY created_at;
            """;
        command.Parameters.AddWithValue("$merchantId", merchantId);
        command.Parameters.AddWithValue("$businessDate", businessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new RegisterEntryResponse(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                DateOnly.Parse(reader.GetString(2), CultureInfo.InvariantCulture),
                Enum.Parse<EntryType>(reader.GetString(3)),
                decimal.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
                reader.GetString(5),
                reader.GetString(6),
                DateTimeOffset.Parse(reader.GetString(7), CultureInfo.InvariantCulture),
                "REGISTRADO"));
        }

        return entries;
    }

    private SqliteConnection CreateConnection()
        => new($"Data Source={_storagePaths.LancamentosDbPath}");
}
