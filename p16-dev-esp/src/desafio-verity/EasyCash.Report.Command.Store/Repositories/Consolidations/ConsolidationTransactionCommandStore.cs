using EasyCash.Report.Command.Store.Contexts;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Report.Domain.Consolidations.Interfaces;
using EasyCash.Report.Domain.Consolidations.Models;
using EasyCash.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyCash.Report.Command.Store.Repositories.Consolidations;

internal sealed class ConsolidationTransactionCommandStore : Repository<ConsolidationTransactionEntity, Guid>, IConsolidationTransactionCommandStore
{
    public ConsolidationTransactionCommandStore(ReportDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<DailyConsolidationModel> GetDailyConsolidationAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var dailyConsolidation = await DbContext
            .Set<ConsolidationTransactionEntity>()
            .AsNoTracking()
            .Where(x => DateOnly.FromDateTime(x.Date.Date) == date)
            .GroupBy(x => DateOnly.FromDateTime(x.Date.Date))
            .Select(x => new DailyConsolidationModel
            {
                Date = x.Key,
                TotalCredit = x.Where(t => t.Type == ETransactionType.Credit).Sum(v => v.Amount),
                TotalDebit = x.Where(t => t.Type == ETransactionType.Debit).Sum(v => v.Amount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dailyConsolidation ?? DailyConsolidationModel.Empty(date);

    }
}