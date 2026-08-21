using CashFlow.Domain.Entities;

namespace CashFlow.Application.Abstractions;

/// <summary>
/// Porta de persistência de lançamentos (somente leitura e inserção).
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Obtém um lançamento por ID.
    /// </summary>
    /// <param name="id">ID do lançamento.</param>
    /// <param name="userId">ID do usuário (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento ou null se não encontrado.</returns>
    Task<Transaction?> GetByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista lançamentos paginados.
    /// </summary>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="userId">ID do usuário (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista paginada de lançamentos.</returns>
    Task<IReadOnlyList<Transaction>> ListAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere um lançamento.
    /// </summary>
    /// <param name="transaction">Lançamento a ser inserido.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task InsertAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
