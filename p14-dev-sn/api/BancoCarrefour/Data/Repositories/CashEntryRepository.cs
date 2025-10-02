using Data.Context;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories
{
    public class CashEntryRepository : ICashEntryRepository
    {
        private readonly CarrefourContext _context;

        public CashEntryRepository(CarrefourContext context)
        {
            _context = context;
        }

        public async Task<CashEntry> CreateAsync(CashEntry cashEntry)
        {
            cashEntry.DateCreated = DateTime.Now;
            await _context.CashEntry.AddAsync(cashEntry);
            await _context.SaveChangesAsync();
            return cashEntry;
        }

        public async Task<IEnumerable<CashEntry>> GetDailyCashEntriesAsync(int pageNumber, int pageSize, Expression<Func<CashEntry, object>> orderBy, bool descending = false, CancellationToken cancellationToken = default)
        {
            var result = _context.CashEntry.AsNoTracking().AsQueryable();

            result = result.Where(
                        c => c.DateCreated.Day == DateTime.Today.Day &&
                        c.DateCreated.Month == DateTime.Today.Month &&
                        c.DateCreated.Year == DateTime.Today.Year);

            result = descending ? result.OrderByDescending(orderBy) : result.OrderBy(orderBy);

            result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return await result.ToListAsync();
        }

        public async Task<int> GetTotalCount()
        {
            var result = _context.CashEntry.AsQueryable();

            result = result.Where(
             c => c.DateCreated.Day == DateTime.Today.Day &&
             c.DateCreated.Month == DateTime.Today.Month &&
             c.DateCreated.Year == DateTime.Today.Year);

            return await result.CountAsync();
        }
    }
}
