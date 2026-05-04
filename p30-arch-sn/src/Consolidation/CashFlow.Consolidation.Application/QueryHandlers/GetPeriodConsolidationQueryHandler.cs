using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Queries;
using CashFlow.Consolidation.Application.Responses;
using CashFlow.Consolidation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Application.QueryHandlers;

public sealed class GetPeriodConsolidationQueryHandler(
    IDailyConsolidationRepository repository,
    ILogger<GetPeriodConsolidationQueryHandler> logger)
    : IRequestHandler<GetPeriodConsolidationQuery, Result<IReadOnlyCollection<DailyConsolidationResponse>>>
{
    public async Task<Result<IReadOnlyCollection<DailyConsolidationResponse>>> Handle(
        GetPeriodConsolidationQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = DateOnly.Parse(request.StartDate);
        var endDate = DateOnly.Parse(request.EndDate);

        logger.LogInformation(
            "Getting period consolidations. StartDate: {StartDate}, EndDate: {EndDate}",
            startDate,
            endDate);

        var consolidations = await repository.GetByPeriodAsync(
            startDate,
            endDate,
            cancellationToken);

        var response = consolidations
            .Select(x => new DailyConsolidationResponse(
                x.Date,
                x.TotalCredits,
                x.TotalDebits,
                x.Balance))
            .ToArray();

        logger.LogInformation(
            "Period consolidations retrieved. StartDate: {StartDate}, EndDate: {EndDate}, Count: {Count}",
            startDate,
            endDate,
            response.Length);

        return Result<IReadOnlyCollection<DailyConsolidationResponse>>.Success(response);
    }
}