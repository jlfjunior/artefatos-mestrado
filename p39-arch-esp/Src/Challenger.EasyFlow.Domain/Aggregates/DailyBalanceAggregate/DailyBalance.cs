using Challenger.EasyFlow.Domain.Common;

namespace Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;

public sealed class DailyBalance() : Entity<Guid>(Guid.CreateVersion7())
{
    public required Guid CashBoxId { get; init; }
    public required decimal OpeningBalance { get; init; }
    public required decimal ClosingBalance { get; init; }
    public required decimal TotalInflow { get; init; }
    public required decimal TotalOutflow { get; init; }
}
