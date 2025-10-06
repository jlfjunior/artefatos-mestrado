using BCash.Domain.Entities;
using BCash.Domain.Repositories;
using BCash.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BCash.Infrastructure.Repositories
{
    public class BalanceRepository : IBalanceRepository
    {
        private readonly SqlDbContext _context;

        public BalanceRepository(SqlDbContext context)
        {
            _context = context;
        }

        public async Task<Balance> AddAsync(Balance balance)
        {
            await _context.Balances.AddAsync(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task<Balance> GetByDateAsync(DateTime date)
        {
            return await _context.Balances.Where(it => it.Date.Equals(date)).FirstOrDefaultAsync();
        }

        public async Task<PagedResponseOffset<Balance>> GetByDatePagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize)
        {
            var totalRecords = await _context.Balances.AsNoTracking()
                .Where(f => f.Date >= initDate && f.Date <= endDate)
                .CountAsync();

            var balances = await _context.Balances.AsNoTracking()
                .Where(f => f.Date >= initDate && f.Date <= endDate)
                .OrderByDescending(x => x.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponseOffset<Balance>(balances, pageNumber, pageSize, totalRecords);
        }

        public async Task<Balance> UpdateAsync(Balance balance)
        {
            _context.Entry(balance).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return balance;
        }
    }
}
