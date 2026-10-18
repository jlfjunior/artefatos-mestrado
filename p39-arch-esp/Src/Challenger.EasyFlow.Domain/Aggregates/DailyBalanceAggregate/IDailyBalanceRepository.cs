namespace Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;

public interface IDailyBalanceRepository
{
    ValueTask<List<DailyBalance>> ListAllBydDateAsync(DateTime date, CancellationToken cancellationToken = default);
}