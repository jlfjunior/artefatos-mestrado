namespace Cashflow.Transactions.Application.EventService
{
    public interface IEventPublisherService<T>
    {
        Task PublishMessageAsync(T message);
    }
}
