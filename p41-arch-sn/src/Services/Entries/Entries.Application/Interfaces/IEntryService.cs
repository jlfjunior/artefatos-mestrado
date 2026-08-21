using Entries.Application.DTOs;

namespace Entries.Application.Interfaces
{
    public interface IEntryService
    {
        Task<EntryResponse> CreateAsync(CreateEntryRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<EntryResponse>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}
