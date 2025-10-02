using EasyCash.Abstractions;

namespace EasyCash.Abstractions.Interfaces;

public interface IIntegrationEventSender<TEvent> where TEvent : IntegrationEventBase
{
    Task Send(TEvent message, CancellationToken cancellationToken);
}