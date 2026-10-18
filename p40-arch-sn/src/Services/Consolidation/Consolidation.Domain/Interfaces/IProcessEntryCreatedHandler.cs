using Consolidation.Domain.DTOs;

namespace Consolidation.Domain.Interfaces
{
    public interface IProcessEntryCreatedHandler
    {
        Task HandleAsync(EntryCreatedMessage message, CancellationToken cancellationToken = default);
    }
}
