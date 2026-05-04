using CashFlow.BuildingBlocks.Results;

namespace CashFlow.Consolidation.Domain.Errors;

public static class DailyConsolidationErrors
{
    public static readonly Error InvalidCredits =
        new("daily_consolidation.invalid_credits", "Total credits cannot be negative.", ErrorType.Business);

    public static readonly Error InvalidDebits =
        new("daily_consolidation.invalid_debits", "Total debits cannot be negative.", ErrorType.Business);

    public static readonly Error NotFound =
        new("daily_consolidation.not_found", "Daily consolidation was not found.", ErrorType.NotFound);
    
    public static readonly Error InvalidCreditAmount =
        new("daily_consolidation.invalid_credit_amount", "Credit amount must be greater than zero.", ErrorType.Business);

    public static readonly Error InvalidDebitAmount =
        new("daily_consolidation.invalid_debit_amount", "Debit amount must be greater than zero.", ErrorType.Business);
    
    public static readonly Error InvalidLaunchType =
        new("daily_consolidation.invalid_launch_type", "Launch type is invalid.", ErrorType.Business);
}