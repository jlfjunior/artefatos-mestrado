using EasyCash.Abstractions;
using EasyCash.Domain.Transactions.Events;

namespace EasyCash.Api.ConsumerInitializer;

public sealed class IntegrationConsumerInitializer : IIntegrationConsumerInitializer
{
    private readonly List<IntegrationEventBase> _consumers = new();

    public IntegrationConsumerInitializer()
    {
        Publish<TransactionCreatedIntegrationEvent>();
    }

    public IReadOnlyList<IntegrationEventBase> Consumers => _consumers.AsReadOnly();

    public IIntegrationConsumerInitializer Publish<TEvent>() where TEvent : IntegrationEventBase, new()
    {
        _consumers.Add(new TEvent());
        return this;
    }
}
