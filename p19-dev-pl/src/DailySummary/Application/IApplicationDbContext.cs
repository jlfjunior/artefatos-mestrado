using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application;

public interface IApplicationDbContext
{
    DbSet<DailySummaryEntity> DailySummaries { get; }
    DbSet<DailyTransactionEntity> DailyTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}