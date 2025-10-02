namespace EasyCash.Abstractions;

public interface IIntegrationConsumerInitializer
{
    IReadOnlyList<IntegrationEventBase> Consumers { get; }

    IIntegrationConsumerInitializer Publish<TEvent>() where TEvent : IntegrationEventBase, new();
}