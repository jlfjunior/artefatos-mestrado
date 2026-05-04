using CashFlow.BuildingBlocks.Application.Interfaces;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Responses;

namespace CashFlow.Consolidation.Application.Queries;

public sealed record GetPeriodConsolidationQuery(
    string StartDate,
    string EndDate)
    : IQuery<Result<IReadOnlyCollection<DailyConsolidationResponse>>>;