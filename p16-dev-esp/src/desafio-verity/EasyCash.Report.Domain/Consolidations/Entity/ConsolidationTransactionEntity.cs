using EasyCash.Abstractions;
using EasyCash.Shared.Enums;
using EasyCash.Shared.ValueObjects;

namespace EasyCash.Report.Domain.Consolidations.Entity;

public sealed class ConsolidationTransactionEntity : EntityBase<Guid>
{
    public DateTimeOffset Date { get; private set; }

    public string Description { get; private set; }

    public ETransactionType Type { get; private set; }

    public Money Amount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private ConsolidationTransactionEntity() { }

    public static ConsolidationTransactionEntity Create(
        string description,
        ETransactionType type,
        decimal amount,
        string category,
        DateTimeOffset date)
    {
        var entity = new ConsolidationTransactionEntity()
        {
            Id = Guid.NewGuid(),
            Description = description,
            Type = type,
            Amount = Money.FromDecimal(amount),
            Date = date,
            CreatedAt = DateTimeOffset.Now
        };

        entity.Validate();

        return entity;
    }

    protected override void Validate()
    {
    }
}