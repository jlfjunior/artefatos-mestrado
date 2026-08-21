using Consolidation.API.src.Application.Interface;
using MassTransit;
using Shared.Contracts;

namespace Consolidation.API.Infrastructure.MessageConsumers;

/// <summary>
/// Consumidor de eventos de transações (via RabbitMq).
/// Processa eventos e atualiza as consolidações diárias.
/// </summary>
public class TransactionCreatedEventConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly IConsolidationService _consolidationService;
    private readonly ILogger<TransactionCreatedEventConsumer> _logger;

    public TransactionCreatedEventConsumer(
        IConsolidationService consolidationService,
        ILogger<TransactionCreatedEventConsumer> logger)
    {
        _consolidationService = consolidationService ?? throw new ArgumentNullException(nameof(consolidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        try
        {
            _logger.LogInformation(
                "Consumindo evento TransactionCreated: {TransactionId}",
                context.Message.TransactionId);

            await _consolidationService.ProcessTransactionAsync(context.Message, context.CancellationToken);

            _logger.LogInformation("Transação processada com sucesso: {TransactionId}", context.Message.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar transação: {TransactionId}", context.Message.TransactionId);
            throw;
        }
    }
}
