using CashFlow.Application.Services;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Shared.Messaging;

namespace CashFlow.Consumer.Consolidation;

/// <summary>
/// Consumer que projeta saldo diário a partir de lançamentos criados.
/// </summary>
/// <param name="serviceProvider">Provedor de serviços para o consumer genérico.</param>
public sealed class ConsolidationConsumer(IServiceProvider serviceProvider)
    : MessageConsumer<DailyBalanceProjector, TransactionCreatedMessage>(serviceProvider)
{
    /// <summary>
    /// Nome da fila de consolidação de saldos.
    /// </summary>
    protected override string QueueName => MessagingConstants.ConsolidationQueue;

    /// <summary>
    /// Exchanges vinculadas à fila de consolidação.
    /// </summary>
    protected override IReadOnlyList<string> BindExchanges => [EventExchanges.TransactionCreated];

    /// <summary>
    /// Delega a projeção do evento ao serviço de aplicação.
    /// </summary>
    /// <param name="service">Projetor de saldo diário.</param>
    /// <param name="message">Evento de lançamento criado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a projeção do evento.</returns>
    protected override Task ReceivedAsync(
        DailyBalanceProjector service,
        TransactionCreatedMessage message,
        CancellationToken cancellationToken) =>
        service.ProjectAsync(message, cancellationToken);
}
