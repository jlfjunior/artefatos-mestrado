using Entries.Domain.Entities;

namespace Entries.Domain.Repositories
{
    public interface IEntryRepository
    {
        Task AddAsync(Entry entry, CancellationToken cancellationToken = default);
        Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Entry>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}
