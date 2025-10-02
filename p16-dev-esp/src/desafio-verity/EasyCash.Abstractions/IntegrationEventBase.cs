using MediatR;

namespace EasyCash.Abstractions;

public abstract class IntegrationEventBase : INotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public abstract string EndpointName { get; }
}