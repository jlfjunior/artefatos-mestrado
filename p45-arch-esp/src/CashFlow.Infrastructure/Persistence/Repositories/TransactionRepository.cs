using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Extensions;

namespace CashFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório Dapper para lançamentos.
/// </summary>
/// <param name="connection">Conexão com o banco de dados.</param>
public sealed class TransactionRepository(ScopedDbConnection connection)
    : RepositoryBase(connection), ITransactionRepository
{
    /// <summary>
    /// Obtém um lançamento por ID.
    /// </summary>
    /// <param name="id">ID do lançamento.</param>
    /// <param name="userId">ID do usuário (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento ou null se não encontrado.</returns>
    public async Task<Transaction?> GetByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, description, amount, entry_type, transaction_date, user_id, created_at, created_by
            FROM transactions
            WHERE id = @Id
                AND (@UserId IS NULL OR user_id = @UserId)
            """;

        var row = await QuerySingleOrDefaultAsync<TransactionRow>(
            sql,
            new { Id = id, UserId = userId },
            cancellationToken);

        return row?.ToDomain();
    }

    /// <summary>
    /// Lista lançamentos opcionalmente filtrados por intervalo de datas.
    /// </summary>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="userId">ID do usuário (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de lançamentos.</returns>
    public async Task<IReadOnlyList<Transaction>> ListAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, description, amount, entry_type, transaction_date, user_id, created_at, created_by
            FROM transactions
            WHERE (@From::date IS NULL OR transaction_date >= @From::date)
                AND (@To::date IS NULL OR transaction_date <= @To::date)
                AND (@UserId IS NULL OR user_id = @UserId)
            ORDER BY transaction_date DESC, created_at DESC
            """;

        var rows = await QueryAsync<TransactionRow>(
            sql,
            new { From = from, To = to, UserId = userId },
            cancellationToken);

        return rows.ToReadOnlyList(r => r.ToDomain());
    }

    /// <summary>
    /// Insere um lançamento.
    /// </summary>
    /// <param name="transaction">Lançamento a ser inserido.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public Task InsertAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO transactions (id, description, amount, entry_type, transaction_date, user_id, created_at, created_by)
            VALUES (@Id, @Description, @Amount, @EntryType, @TransactionDate, @UserId, @CreatedAt, @CreatedBy)
            """;

        return ExecuteCommandAsync(sql, MapParams(transaction), cancellationToken);
    }

    private static object MapParams(Transaction transaction) => new
    {
        transaction.Id,
        transaction.Description,
        transaction.Amount,
        EntryType = transaction.EntryType.ToString(),
        transaction.TransactionDate,
        transaction.UserId,
        transaction.CreatedAt,
        transaction.CreatedBy
    };

    private sealed record TransactionRow(
        Guid Id,
        string Description,
        decimal Amount,
        string EntryType,
        DateOnly TransactionDate,
        Guid UserId,
        DateTime CreatedAt,
        Guid CreatedBy)
    {
        public Transaction ToDomain() =>
            Transaction.Restore(
                Id,
                Description,
                Amount,
                Enum.Parse<EntryType>(EntryType, ignoreCase: true),
                TransactionDate,
                UserId,
                CreatedAt,
                CreatedBy);
    }
}
