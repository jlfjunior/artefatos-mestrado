using EasyCash.Abstractions;

namespace EasyCash.Report.Api.ConsumerInitializer;

public sealed class IntegrationConsumerInitializer : IIntegrationConsumerInitializer
{
    private readonly List<IntegrationEventBase> _consumers = new();

    public IntegrationConsumerInitializer()
    {
        Publish<Domain.Consolidations.Events.TransactionCreatedIntegrationEvent>();
    }

    public IReadOnlyList<IntegrationEventBase> Consumers => _consumers.AsReadOnly();

    public IIntegrationConsumerInitializer Publish<TEvent>() where TEvent : IntegrationEventBase, new()
    {
        _consumers.Add(new TEvent());
        return this;
    }
}
