using Entries.Application.DTOs;
using Entries.Domain.Repositories;

namespace Entries.Application.UseCases.GetEntriesByDate
{
    public sealed class GetEntriesByDateHandler(IEntryRepository repository)
    {
        public async Task<IReadOnlyList<EntryResponse>> HandleAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var entries = await repository.GetByDateAsync(date, cancellationToken);

            return entries.Select(entry => new EntryResponse(
                entry.Id,
                entry.Amount.Amount,
                entry.Amount.Currency,
                entry.Type,
                entry.Description,
                entry.Date,
                entry.CreatedAt)).ToList();
        }
    }
}
