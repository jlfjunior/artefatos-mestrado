using Lancamentos.Application.Ports;
using MassTransit;

namespace Lancamentos.Infrastructure.Messaging;

/// <summary>
/// Adapter da porta de publicação para o MassTransit. Com o outbox EF Core
/// configurado, o <see cref="IPublishEndpoint"/> grava a mensagem na outbox
/// dentro da transação atual; o delivery service entrega depois do commit.
/// </summary>
public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        => _publishEndpoint = publishEndpoint;

    public Task PublicarAsync<TEvento>(TEvento evento, CancellationToken cancellationToken = default)
        where TEvento : class
        => _publishEndpoint.Publish(evento, cancellationToken);
}
