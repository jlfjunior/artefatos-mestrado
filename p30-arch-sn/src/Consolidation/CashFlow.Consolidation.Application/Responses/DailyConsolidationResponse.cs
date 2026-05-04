namespace CashFlow.Consolidation.Application.Responses;

public sealed record DailyConsolidationResponse(
    DateOnly Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance);