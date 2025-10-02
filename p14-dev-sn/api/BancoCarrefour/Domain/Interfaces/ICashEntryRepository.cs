using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface ICashEntryRepository
    {
        Task<CashEntry> CreateAsync(CashEntry cashEntry);
        Task<IEnumerable<CashEntry>> GetDailyCashEntriesAsync(int pageNumber, int pageSize, Expression<Func<CashEntry, object>> orderBy, bool descending = false, CancellationToken cancellationToken = default);
        Task<int> GetTotalCount();
    }
}
