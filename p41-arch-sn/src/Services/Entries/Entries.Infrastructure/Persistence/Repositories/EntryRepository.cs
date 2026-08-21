using Entries.Domain.Entities;
using Entries.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Entries.Infrastructure.Persistence.Repositories
{
    public sealed class EntryRepository(AppDbContext context) : IEntryRepository
    {
        public async Task AddAsync(Entry entry, CancellationToken cancellationToken = default)
        {
            await context.Entries.AddAsync(entry, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await context.Entries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public async Task<IReadOnlyList<Entry>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default) =>
            await context.Entries
                .Where(e => e.Date.Date == date.Date)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(cancellationToken);
    }
}
