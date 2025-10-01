namespace CashFlow.Application.Common.PubSub
{
    public interface IBusPublisher
    {
        Task SendAsRawJsonAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
    }
}
