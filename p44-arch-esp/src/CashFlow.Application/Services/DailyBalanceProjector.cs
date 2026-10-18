using CashFlow.Application.Abstractions;
using CashFlow.Domain.Services;
using CashFlow.Shared.Messaging;

namespace CashFlow.Application.Services;

/// <summary>
/// Projetor de Event Messages de lançamento na tabela de saldo diário.
/// </summary>
/// <param name="repository">Repositório de saldos diários.</param>
public sealed class DailyBalanceProjector(
    IDailyBalanceRepository repository)
{
    /// <summary>
    /// Processa evento de criação de lançamento.
    /// </summary>
    /// <param name="message">Mensagem de evento de criação de lançamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public Task ProjectAsync(TransactionCreatedMessage message, CancellationToken cancellationToken = default)
    {
        var data = message.Data;
        var eventId = Guid.Parse(message.Id);
        var (creditDelta, debitDelta) = DailyBalanceCalculator.ComputeAdditionDelta(data.Amount, data.EntryType);

        return repository.ApplyDeltaAsync(data.UserId, data.TransactionDate, creditDelta, debitDelta, eventId, cancellationToken);
    }
}
