using CashFlow.Application.Abstractions;

namespace CashFlow.Infrastructure.Persistence;

/// <summary>
/// Unidade de trabalho transacional.
/// </summary>
/// <param name="connection">Conexão scoped compartilhada no escopo da requisição.</param>
public sealed class PostgresUnitOfWork(ScopedDbConnection connection) : IUnitOfWork
{
    /// <summary>
    /// Executa a ação dentro de uma transação PostgreSQL.
    /// </summary>
    /// <param name="action">Ação a executar com transação ambiente ativa.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task concluída após commit ou rollback.</returns>
    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        var dbConnection = await connection.GetConnectionAsync(cancellationToken);
        await using var transaction = await dbConnection.BeginTransactionAsync(cancellationToken);

        try
        {
            DbConnectionFactory.SetAmbientTransaction(transaction);
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            DbConnectionFactory.ClearAmbientTransaction();
        }
    }
}
