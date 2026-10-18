using Consolidation.Application.DTOs;
using Consolidation.Domain.Repositories;

namespace Consolidation.Application.UseCases.GetDailyReport
{
    public sealed class GetDailyReportHandler(IDailyBalanceRepository repository)
    {
        public async Task<DailyBalanceResponse?> HandleAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var dailyBalance = await repository.GetByDateAsync(date, cancellationToken);

            if (dailyBalance is null)
                return null;

            return new DailyBalanceResponse(
                dailyBalance.Id,
                dailyBalance.Date,
                dailyBalance.TotalCredits,
                dailyBalance.TotalDebits,
                dailyBalance.Balance,
                dailyBalance.Currency,
                dailyBalance.UpdatedAt);
        }

        public async Task<IReadOnlyList<DailyBalanceResponse>> HandleRangeAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var balances = await repository.GetByDateRangeAsync(from, to, cancellationToken);

            return balances.Select(dailyBalance => new DailyBalanceResponse(
                dailyBalance.Id,
                dailyBalance.Date,
                dailyBalance.TotalCredits,
                dailyBalance.TotalDebits,
                dailyBalance.Balance,
                dailyBalance.Currency,
                dailyBalance.UpdatedAt)).ToList();
        }
    }
}
