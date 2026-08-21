
namespace Consolidation.Domain.Interfaces
{
    public interface IMessageConsumer
    {
        Task StartConsumingAsync(CancellationToken cancellationToken = default);
    }
}
