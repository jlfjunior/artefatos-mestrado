using CashFlow.Domain.Entities;

namespace CashFlow.Application.Abstractions;

/// <summary>
/// Porta de leitura/escrita da projeção de saldo diário.
/// </summary>
public interface IDailyBalanceRepository
{
    /// <summary>
    /// Obtém o saldo diário de um usuário em uma data específica.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data contábil do saldo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Saldo diário ou null se não encontrado.</returns>
    Task<DailyBalance?> GetByDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o último saldo consolidado anterior à data informada (estritamente menor).
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data contábil de referência.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Último saldo anterior ou null se não houver histórico.</returns>
    Task<DailyBalance?> GetLastBeforeDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista saldos diários paginados.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="page">Número da página (base 1).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista paginada de saldos diários.</returns>
    Task<(IReadOnlyList<DailyBalance> Items, int TotalCount)> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere ou atualiza um saldo diário.
    /// </summary>
    /// <param name="balance">Saldo diário a ser inserido ou atualizado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task UpsertAsync(DailyBalance balance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica um delta de crédito e débito a um saldo diário.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data contábil do saldo.</param>
    /// <param name="creditDelta">Delta de crédito.</param>
    /// <param name="debitDelta">Delta de débito.</param>
    /// <param name="eventId">ID do evento associado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task ApplyDeltaAsync(
        Guid userId,
        DateOnly date,
        decimal creditDelta,
        decimal debitDelta,
        Guid eventId,
        CancellationToken cancellationToken = default);
}
