using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Services;
using CashFlow.Shared.Extensions;
using Dapper;

namespace CashFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório Dapper para projeção de saldo diário.
/// </summary>
/// <param name="connection">Conexão com o banco de dados.</param>
public sealed class DailyBalanceRepository(ScopedDbConnection connection)
    : RepositoryBase(connection), IDailyBalanceRepository
{
    /// <summary>
    /// Obtém o saldo consolidado de um dia.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data contábil do consolidado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Saldo consolidado do dia ou null se não encontrado.</returns>
    public async Task<DailyBalance?> GetByDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT user_id, balance_date, total_credits, total_debits, balance, last_event_id, updated_at
            FROM daily_balances
            WHERE user_id = @UserId AND balance_date = @Date
            """;

        var row = await QuerySingleOrDefaultAsync<DailyBalanceRow>(
            sql,
            new { UserId = userId, Date = date },
            cancellationToken);

        return row?.ToDomain();
    }

    /// <inheritdoc />
    public async Task<DailyBalance?> GetLastBeforeDateAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT user_id, balance_date, total_credits, total_debits, balance, last_event_id, updated_at
            FROM daily_balances
            WHERE user_id = @UserId AND balance_date < @Date
            ORDER BY balance_date DESC
            LIMIT 1
            """;

        var row = await QuerySingleOrDefaultAsync<DailyBalanceRow>(
            sql,
            new { UserId = userId, Date = date },
            cancellationToken);

        return row?.ToDomain();
    }

    /// <summary>
    /// Lista saldos consolidados paginados.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="page">Número da página (base 1).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista paginada de saldos consolidados.</returns>
    public Task<(IReadOnlyList<DailyBalance> Items, int TotalCount)> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(async (dbConnection, ct) =>
        {
            const string countSql = """
                SELECT COUNT(*)
                FROM daily_balances
                WHERE user_id = @UserId
                    AND (@From::date IS NULL OR balance_date >= @From::date)
                    AND (@To::date IS NULL OR balance_date <= @To::date)
                """;

            const string listSql = """
                SELECT user_id, balance_date, total_credits, total_debits, balance, last_event_id, updated_at
                FROM daily_balances
                WHERE user_id = @UserId
                    AND (@From::date IS NULL OR balance_date >= @From::date)
                    AND (@To::date IS NULL OR balance_date <= @To::date)
                ORDER BY balance_date DESC
                OFFSET @Offset LIMIT @Limit
                """;

            var parameters = new
            {
                UserId = userId,
                From = from,
                To = to,
                Offset = (page - 1) * pageSize,
                Limit = pageSize
            };

            var totalCount = await dbConnection.ExecuteScalarAsync<int>(Command(countSql, parameters, ct));
            var rows = await dbConnection.QueryAsync<DailyBalanceRow>(Command(listSql, parameters, ct));

            return (rows.ToReadOnlyList(r => r.ToDomain()), totalCount);
        }, cancellationToken);

    /// <summary>
    /// Insere ou atualiza um saldo consolidado.
    /// </summary>
    /// <param name="balance">Saldo consolidado a ser inserido ou atualizado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public Task UpsertAsync(DailyBalance balance, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO daily_balances (user_id, balance_date, total_credits, total_debits, balance, last_event_id, updated_at)
            VALUES (@UserId, @BalanceDate, @TotalCredits, @TotalDebits, @Balance, @LastEventId, @UpdatedAt)
            ON CONFLICT (user_id, balance_date) DO UPDATE SET
                total_credits = EXCLUDED.total_credits,
                total_debits = EXCLUDED.total_debits,
                balance = EXCLUDED.balance,
                last_event_id = EXCLUDED.last_event_id,
                updated_at = EXCLUDED.updated_at
            """;

        return ExecuteCommandAsync(sql, new
        {
            balance.UserId,
            balance.BalanceDate,
            balance.TotalCredits,
            balance.TotalDebits,
            balance.Balance,
            balance.LastEventId,
            balance.UpdatedAt
        }, cancellationToken);
    }

    /// <summary>
    /// Aplica um delta de crédito e débito a um saldo consolidado.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data contábil do consolidado.</param>
    /// <param name="creditDelta">Delta de crédito.</param>
    /// <param name="debitDelta">Delta de débito.</param>
    /// <param name="eventId">ID do evento associado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public Task ApplyDeltaAsync(
        Guid userId,
        DateOnly date,
        decimal creditDelta,
        decimal debitDelta,
        Guid eventId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(async (dbConnection, ct) =>
        {
            const string selectSql = """
                SELECT total_credits, total_debits
                FROM daily_balances
                WHERE user_id = @UserId AND balance_date = @Date
                FOR UPDATE
                """;

            const string previousBalanceSql = """
                SELECT balance
                FROM daily_balances
                WHERE user_id = @UserId AND balance_date < @Date
                ORDER BY balance_date DESC
                LIMIT 1
                """;

            var existing = await dbConnection.QuerySingleOrDefaultAsync<TotalsRow>(
                Command(selectSql, new { UserId = userId, Date = date }, ct));

            var previousBalance = await dbConnection.QuerySingleOrDefaultAsync<decimal?>(
                Command(previousBalanceSql, new { UserId = userId, Date = date }, ct)) ?? 0m;

            var currentCredits = existing?.TotalCredits ?? 0;
            var currentDebits = existing?.TotalDebits ?? 0;

            var (totalCredits, totalDebits, _) = DailyBalanceCalculator.ApplyDelta(
                currentCredits,
                currentDebits,
                creditDelta,
                debitDelta);

            var balance = DailyBalanceCalculator.ComputeCumulativeBalance(
                previousBalance,
                totalCredits,
                totalDebits);

            const string upsertSql = """
                INSERT INTO daily_balances (user_id, balance_date, total_credits, total_debits, balance, last_event_id, updated_at)
                VALUES (@UserId, @Date, @TotalCredits, @TotalDebits, @Balance, @EventId, @UpdatedAt)
                ON CONFLICT (user_id, balance_date) DO UPDATE SET
                    total_credits = EXCLUDED.total_credits,
                    total_debits = EXCLUDED.total_debits,
                    balance = EXCLUDED.balance,
                    last_event_id = EXCLUDED.last_event_id,
                    updated_at = EXCLUDED.updated_at
                """;

            await dbConnection.ExecuteAsync(Command(upsertSql, new
            {
                UserId = userId,
                Date = date,
                TotalCredits = totalCredits,
                TotalDebits = totalDebits,
                Balance = balance,
                EventId = eventId,
                UpdatedAt = DateTime.UtcNow
            }, ct));
        }, cancellationToken);

    private sealed record DailyBalanceRow(
        Guid UserId,
        DateOnly BalanceDate,
        decimal TotalCredits,
        decimal TotalDebits,
        decimal Balance,
        Guid? LastEventId,
        DateTime UpdatedAt)
    {
        public DailyBalance ToDomain() => new()
        {
            UserId = UserId,
            BalanceDate = BalanceDate,
            TotalCredits = TotalCredits,
            TotalDebits = TotalDebits,
            Balance = Balance,
            LastEventId = LastEventId,
            UpdatedAt = UpdatedAt
        };
    }

    private sealed record TotalsRow(decimal TotalCredits, decimal TotalDebits);
}
