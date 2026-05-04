using System.Globalization;
using CashFlow.Consolidado.Api.Application;
using CashFlow.Shared;
using Microsoft.Data.Sqlite;

namespace CashFlow.Consolidado.Api.Infrastructure;

public sealed class SqliteDailyBalanceProjectionStore : IDailyBalanceProjectionStore
{
    private readonly ConsolidadoStoragePaths _storagePaths;
    private readonly DailyBalanceProjector _projector;
    private readonly TimeProvider _timeProvider;

    public SqliteDailyBalanceProjectionStore(
        ConsolidadoStoragePaths storagePaths,
        DailyBalanceProjector projector,
        TimeProvider timeProvider)
    {
        _storagePaths = storagePaths;
        _projector = projector;
        _timeProvider = timeProvider;
    }

    public async Task<long> GetCheckpointAsync(string consumerName, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT last_sequence_id
            FROM consumer_checkpoints
            WHERE consumer_name = $consumerName
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$consumerName", consumerName);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is null or DBNull ? 0L : Convert.ToInt64(scalar, CultureInfo.InvariantCulture);
    }

    public async Task ApplyAsync(string consumerName, IntegrationEnvelope envelope, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        if (await EventAlreadyProcessedAsync(connection, transaction, envelope.Payload.EventId, cancellationToken))
        {
            await UpsertCheckpointAsync(connection, transaction, consumerName, envelope.SequenceId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var current = await GetSnapshotInternalAsync(
            connection,
            transaction,
            envelope.Payload.MerchantId,
            envelope.Payload.BusinessDate,
            cancellationToken);

        var updated = _projector.Apply(current, envelope.Payload, envelope.SequenceId, _timeProvider.GetUtcNow());

        await UpsertSnapshotAsync(connection, transaction, updated, cancellationToken);
        await InsertProcessedEventAsync(connection, transaction, envelope, cancellationToken);
        await UpsertCheckpointAsync(connection, transaction, consumerName, envelope.SequenceId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<DailyBalanceSnapshot?> GetAsync(
        string merchantId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return await GetSnapshotInternalAsync(connection, null, merchantId, businessDate, cancellationToken);
    }

    public async Task ReplaceAsync(DailyBalanceSnapshot snapshot, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await UpsertSnapshotAsync(connection, transaction, snapshot, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<bool> EventAlreadyProcessedAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT 1
            FROM processed_events
            WHERE event_id = $eventId
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$eventId", eventId.ToString());
        var exists = await command.ExecuteScalarAsync(cancellationToken);
        return exists is not null;
    }

    private static async Task InsertProcessedEventAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        IntegrationEnvelope envelope,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO processed_events (event_id, sequence_id, processed_at)
            VALUES ($eventId, $sequenceId, $processedAt);
            """;
        command.Parameters.AddWithValue("$eventId", envelope.Payload.EventId.ToString());
        command.Parameters.AddWithValue("$sequenceId", envelope.SequenceId);
        command.Parameters.AddWithValue("$processedAt", envelope.Payload.OccurredAt.ToString("O", CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpsertCheckpointAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string consumerName,
        long sequenceId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO consumer_checkpoints (consumer_name, last_sequence_id)
            VALUES ($consumerName, $sequenceId)
            ON CONFLICT(consumer_name) DO UPDATE SET
                last_sequence_id = CASE
                    WHEN excluded.last_sequence_id > consumer_checkpoints.last_sequence_id
                    THEN excluded.last_sequence_id
                    ELSE consumer_checkpoints.last_sequence_id
                END;
            """;
        command.Parameters.AddWithValue("$consumerName", consumerName);
        command.Parameters.AddWithValue("$sequenceId", sequenceId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpsertSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        DailyBalanceSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO daily_balances (
                merchant_id, business_date, total_credits, total_debits, balance, last_processed_event_id, last_sequence_id, updated_at
            ) VALUES (
                $merchantId, $businessDate, $totalCredits, $totalDebits, $balance, $lastProcessedEventId, $lastSequenceId, $updatedAt
            )
            ON CONFLICT(merchant_id, business_date) DO UPDATE SET
                total_credits = excluded.total_credits,
                total_debits = excluded.total_debits,
                balance = excluded.balance,
                last_processed_event_id = excluded.last_processed_event_id,
                last_sequence_id = excluded.last_sequence_id,
                updated_at = excluded.updated_at;
            """;
        command.Parameters.AddWithValue("$merchantId", snapshot.MerchantId);
        command.Parameters.AddWithValue("$businessDate", snapshot.BusinessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$totalCredits", snapshot.TotalCredits.ToString(CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$totalDebits", snapshot.TotalDebits.ToString(CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$balance", snapshot.Balance.ToString(CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$lastProcessedEventId", snapshot.LastProcessedEventId?.ToString() ?? string.Empty);
        command.Parameters.AddWithValue("$lastSequenceId", snapshot.LastSequenceId);
        command.Parameters.AddWithValue("$updatedAt", snapshot.UpdatedAt.ToString("O", CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<DailyBalanceSnapshot?> GetSnapshotInternalAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string merchantId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT merchant_id, business_date, total_credits, total_debits, balance, last_processed_event_id, last_sequence_id, updated_at
            FROM daily_balances
            WHERE merchant_id = $merchantId AND business_date = $businessDate
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$merchantId", merchantId);
        command.Parameters.AddWithValue("$businessDate", businessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var lastProcessedEventId = reader.GetString(5);
        return new DailyBalanceSnapshot(
            reader.GetString(0),
            DateOnly.Parse(reader.GetString(1), CultureInfo.InvariantCulture),
            decimal.Parse(reader.GetString(2), CultureInfo.InvariantCulture),
            decimal.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
            decimal.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
            string.IsNullOrWhiteSpace(lastProcessedEventId) ? null : Guid.Parse(lastProcessedEventId),
            reader.GetInt64(6),
            DateTimeOffset.Parse(reader.GetString(7), CultureInfo.InvariantCulture));
    }

    private SqliteConnection CreateConnection()
        => new($"Data Source={_storagePaths.ConsolidadoDbPath}");
}
