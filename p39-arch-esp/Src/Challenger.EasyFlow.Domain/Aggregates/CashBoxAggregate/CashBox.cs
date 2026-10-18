using Challenger.EasyFlow.Domain.Common;

namespace Challenger.EasyFlow.Domain.Aggregates.CashBoxAggregate;

public sealed class CashBox(Guid merchantId, string name, decimal initialBalance) : Entity<Guid>(Guid.CreateVersion7())
{
    public Guid MerchantId { get; private set; } = merchantId;
    public string Name { get; private set; } = name;
    public decimal Balance { get; private set; } = initialBalance;
    public DateTimeOffset OpenedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; private set; }
    public bool IsOpen => ClosedAt == null;

    private CashBox() : this(Guid.Empty, string.Empty, decimal.Zero) { }
}
