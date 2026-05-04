using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Queries;
using CashFlow.Consolidation.Application.Responses;
using CashFlow.Consolidation.Domain.Errors;
using CashFlow.Consolidation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Application.QueryHandlers;

public sealed class GetDailyConsolidationQueryHandler(
    IDailyConsolidationRepository repository,
    ILogger<GetDailyConsolidationQueryHandler> logger)
    : IRequestHandler<GetDailyConsolidationQuery, Result<DailyConsolidationResponse>>
{
    public async Task<Result<DailyConsolidationResponse>> Handle(
        GetDailyConsolidationQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting daily consolidation. Date: {Date}", request.Date);

        var consolidation = await repository.GetByDateAsync(request.Date, cancellationToken);

        if (consolidation is null)
        {
            logger.LogInformation("Daily consolidation not found. Date: {Date}", request.Date);
            return Result<DailyConsolidationResponse>.Failure(DailyConsolidationErrors.NotFound);
        }

        logger.LogInformation("Daily consolidation found. Date: {Date}", request.Date);
        return Result<DailyConsolidationResponse>.Success(
            new DailyConsolidationResponse(
                consolidation.Date,
                consolidation.TotalCredits,
                consolidation.TotalDebits,
                consolidation.Balance));
    }
}