namespace CashFlow.Consolidation.Api.Requests;

public sealed record GetConsolidationQueryRequest(
    string StartDate,
    string EndDate);
