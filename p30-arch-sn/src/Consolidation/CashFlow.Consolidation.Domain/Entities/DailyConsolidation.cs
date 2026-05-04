using CashFlow.BuildingBlocks.Domain.Entities;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Domain.Errors;

namespace CashFlow.Consolidation.Domain.Entities;

public sealed class DailyConsolidation : Entity
{
    public DateOnly Date { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal Balance { get; private set; }

    private DailyConsolidation(): base(Guid.Empty)
    {
    }

    private DailyConsolidation(
        Guid id,
        DateOnly date,
        decimal totalCredits,
        decimal totalDebits)
        : base(id)
    {
        Date = date;
        TotalCredits = totalCredits;
        TotalDebits = totalDebits;
        Balance = totalCredits - totalDebits;
    }

    public static Result<DailyConsolidation> Create(
        DateOnly date,
        decimal totalCredits,
        decimal totalDebits)
    {
        if (totalCredits < 0)
            return Result<DailyConsolidation>.Failure(DailyConsolidationErrors.InvalidCredits);

        if (totalDebits < 0)
            return Result<DailyConsolidation>.Failure(DailyConsolidationErrors.InvalidDebits);

        return Result<DailyConsolidation>.Success(
            new DailyConsolidation(Guid.NewGuid(), date, totalCredits, totalDebits));
    }

    public Result ApplyCredit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(DailyConsolidationErrors.InvalidCreditAmount);

        TotalCredits += amount;
        RecalculateBalance();

        return Result.Success();
    }

    public Result ApplyDebit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(DailyConsolidationErrors.InvalidDebitAmount);

        TotalDebits += amount;
        RecalculateBalance();

        return Result.Success();
    }

    private void RecalculateBalance()
    {
        Balance = TotalCredits - TotalDebits;
    }
}