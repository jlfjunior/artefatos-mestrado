using CashFlow.BuildingBlocks.Application.Interfaces;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Responses;

namespace CashFlow.Consolidation.Application.Queries;

public sealed record GetDailyConsolidationQuery(DateOnly Date) : IQuery<Result<DailyConsolidationResponse>>;