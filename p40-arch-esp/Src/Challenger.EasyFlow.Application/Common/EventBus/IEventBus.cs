namespace Challenger.EasyFlow.Application.Common.EventBus;

public interface IEventBus
{
    ValueTask PublishAsync<TEvent>(TEvent @event, string queueName, CancellationToken cancellationToken = default)
        where TEvent : notnull;

    ValueTask ConsumeAsync<TEvent>(Func<TEvent, CancellationToken, Task> onMessageReceived, string queueName, CancellationToken cancellationToken = default)
        where TEvent : notnull;
}