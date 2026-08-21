namespace Lancamentos.Application.Ports;

/// <summary>
/// Porta de saída para publicação de eventos de integração. A implementação
/// usa o outbox do MassTransit, de modo que o publish entra na mesma transação
/// da escrita do lançamento.
/// </summary>
public interface IEventPublisher
{
    Task PublicarAsync<TEvento>(TEvento evento, CancellationToken cancellationToken = default)
        where TEvento : class;
}
