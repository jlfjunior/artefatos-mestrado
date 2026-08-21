using Challenger.EasyFlow.Domain.Common;

namespace Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

public sealed class Transaction() : Entity<Guid>(Guid.CreateVersion7())
{
    public required Guid CashBoxId { get; init; }
    public required TransactionType Type { get; init; }
    public required decimal Amount { get; init; }
    public required TransactionCategory Category { get; init; }
    public required PaymentMethod PaymentMethod { get; init; }
    public required string Description { get; init; }
}
