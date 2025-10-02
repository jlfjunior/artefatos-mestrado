using EasyCash.Abstractions;
using EasyCash.Shared.ValueObjects;

namespace EasyCash.Report.Domain.Consolidations.Entity;

public sealed class DailyConsolidationEntity : EntityBase<Guid>
{
    public DateTimeOffset Date { get; private set; }

    public Money TotalCredit { get; private set; }

    public Money TotalDebit { get; private set; }

    public Money FinalBalance { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private DailyConsolidationEntity() { }

    public static DailyConsolidationEntity Create(DateOnly date
        , decimal totalCredit
        , decimal totalDebit)
    {
        var entity = new DailyConsolidationEntity()
        {
            Id = Guid.NewGuid(),
            Date = date.ToDateTime(TimeOnly.MinValue),
            TotalCredit = Money.FromDecimal(totalCredit),
            TotalDebit = Money.FromDecimal(totalDebit),
            CreatedAt = DateTimeOffset.Now
        };

        entity.CalculateFinalBalance();

        entity.Validate();

        return entity;
    }

    private void CalculateFinalBalance()
    {
        FinalBalance = TotalCredit - TotalDebit;
    }

    protected override void Validate()
    {

    }
}

