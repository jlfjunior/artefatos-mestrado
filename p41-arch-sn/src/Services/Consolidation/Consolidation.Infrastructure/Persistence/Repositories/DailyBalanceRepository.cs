using Consolidation.Domain.Entities;
using Consolidation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Consolidation.Infrastructure.Persistence.Repositories
{
    public sealed class DailyBalanceRepository(AppDbContext context) : IDailyBalanceRepository
    {
        public async Task<DailyBalance?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default) =>
            await context.DailyBalances
            .FirstOrDefaultAsync(d => d.Date.Date == date.Date, cancellationToken);

        public async Task<IReadOnlyList<DailyBalance>> GetByDateRangeAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default) =>
            await context.DailyBalances
                .Where(d => d.Date.Date >= from.Date && d.Date.Date <= to.Date)
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default)
        {
            await context.DailyBalances.AddAsync(dailyBalance, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default)
        {
            context.DailyBalances.Update(dailyBalance);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
