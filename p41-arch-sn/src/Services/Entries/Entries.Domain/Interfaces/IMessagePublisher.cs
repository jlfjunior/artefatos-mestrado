
namespace Entries.Domain.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
    }
}
