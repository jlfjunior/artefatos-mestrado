using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Commands;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Errors;
using CashFlow.Consolidation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Application.CommandHandlers;

public sealed class ProcessLaunchRegisteredEventCommandHandler(
    IDailyConsolidationRepository repository,
    IProcessedLaunchEventRepository processedLaunchEventRepository,
    IUnitOfWork unitOfWork,
    ILogger<ProcessLaunchRegisteredEventCommandHandler> logger)
    : IRequestHandler<ProcessLaunchRegisteredEventCommand, Result>
{
    public async Task<Result> Handle(
        ProcessLaunchRegisteredEventCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing launch registered event command. LaunchId: {LaunchId}, Type: {Type}, Amount: {Amount}, OccurredOnUtc: {OccurredOnUtc}",
            request.LaunchId,
            request.Type,
            request.Amount,
            request.OccurredOnUtc);

        var alreadyProcessed = await processedLaunchEventRepository.ExistsByLaunchIdAsync(
            request.LaunchId,
            cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Launch event ignored due to idempotency. LaunchId: {LaunchId}", request.LaunchId);
            return Result.Success();
        }

        var consolidationDate = DateOnly.FromDateTime(request.OccurredOnUtc);

        var consolidation = await repository.GetByDateForUpdateAsync(consolidationDate, cancellationToken);

        if (consolidation is null)
        {
            var newConsolidationResult = DailyConsolidation.Create(consolidationDate, 0, 0);

            if (newConsolidationResult.IsFailure)
                return Result.Failure(newConsolidationResult.Error);

            consolidation = newConsolidationResult.Value;

            await repository.AddAsync(consolidation, cancellationToken);

            logger.LogInformation("Daily consolidation created. Date: {Date}", consolidationDate);
        }

        var applyResult = ApplyLaunch(consolidation, request.Type, request.Amount);

        if (applyResult.IsFailure)
        {
            logger.LogWarning(
                "Launch event processing failed during consolidation apply. Code: {Code}, Message: {Message}, LaunchId: {LaunchId}",
                applyResult.Error.Code,
                applyResult.Error.Message,
                request.LaunchId);
            return applyResult;
        }

        await processedLaunchEventRepository.AddAsync(
            ProcessedLaunchEvent.Create(request.LaunchId, DateTime.UtcNow),
            cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsProcessedLaunchEventUniqueViolation(ex))
        {
            logger.LogInformation(
                "Launch event ignored due to concurrent idempotency check. LaunchId: {LaunchId}",
                request.LaunchId);

            return Result.Success();
        }

        logger.LogInformation(
            "Launch event processed successfully. LaunchId: {LaunchId}, Date: {Date}",
            request.LaunchId,
            consolidationDate);

        return Result.Success();
    }

    private static Result ApplyLaunch(
        DailyConsolidation consolidation,
        string type,
        decimal amount)
    {
        if (type.Equals("credit", StringComparison.OrdinalIgnoreCase))
            return consolidation.ApplyCredit(amount);

        if (type.Equals("debit", StringComparison.OrdinalIgnoreCase))
            return consolidation.ApplyDebit(amount);

        return Result.Failure(DailyConsolidationErrors.InvalidLaunchType);
    }

    private static bool IsProcessedLaunchEventUniqueViolation(Exception exception)
    {
        return exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
               && exception.Message.Contains("processed_launch_events", StringComparison.OrdinalIgnoreCase)
               && exception.Message.Contains("LaunchId", StringComparison.OrdinalIgnoreCase);
    }
}