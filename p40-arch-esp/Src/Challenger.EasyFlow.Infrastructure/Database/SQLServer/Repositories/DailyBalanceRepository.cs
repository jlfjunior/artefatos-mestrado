using Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;
using Microsoft.EntityFrameworkCore;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Repositories;

internal sealed class DailyBalanceRepository(EasyFlowContext context) : IDailyBalanceRepository
{
    private readonly DbSet<DailyBalance> _dailyBalances = context.Set<DailyBalance>();

    public async ValueTask<List<DailyBalance>> ListAllBydDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dailyBalances
            .Where(db => db.CreatedAt.Date == date)
            .ToListAsync(cancellationToken);
    }
}