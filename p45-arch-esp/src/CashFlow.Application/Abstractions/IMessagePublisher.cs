using CashFlow.Shared.Messaging;

namespace CashFlow.Application.Abstractions;

/// <summary>
/// Porta de publicação de mensagens (Enterprise Integration Patterns — Message Channel).
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publica uma mensagem.
    /// </summary>
    /// <param name="message">Mensagem a ser publicada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task PublishAsync(IIntegrationMessage message, CancellationToken cancellationToken = default);
}
