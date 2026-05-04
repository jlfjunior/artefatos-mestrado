using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Launches.Application.Commands;
using CashFlow.Launches.Application.Interfaces;
using CashFlow.Launches.Domain.Entities;
using CashFlow.Launches.Domain.Enums;
using CashFlow.Launches.Domain.Errors;
using CashFlow.Launches.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Launches.Application.CommandHandlers;

public sealed class RegisterLaunchCommandHandler(
    ILaunchRepository launchRepository,
    IUnitOfWork unitOfWork,
    ILaunchIntegrationEventPublisher integrationEventPublisher,
    ILogger<RegisterLaunchCommandHandler> logger)
    : IRequestHandler<RegisterLaunchCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        RegisterLaunchCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing launch registration command. Type: {Type}, Amount: {Amount}, OccurredOnUtc: {OccurredOnUtc}",
            request.Type,
            request.Amount,
            request.OccurredOnUtc);

        var launchTypeResult = ParseLaunchType(request.Type);
        if (launchTypeResult.IsFailure)
        {
            logger.LogWarning(
                "Launch registration failed due to invalid type. Type: {Type}",
                request.Type);

            return Result<Guid>.Failure(launchTypeResult.Error);
        }

        var launchResult = Launch.Create(
            request.Amount,
            launchTypeResult.Value,
            request.OccurredOnUtc);

        if (launchResult.IsFailure)
        {
            logger.LogWarning(
                "Launch registration failed during domain creation. Code: {Code}, Message: {Message}",
                launchResult.Error.Code,
                launchResult.Error.Message);

            return Result<Guid>.Failure(launchResult.Error);
        }

        var launch = launchResult.Value;

        await launchRepository.AddAsync(launch, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Launch persisted successfully. LaunchId: {LaunchId}",
            launch.Id);

        var integrationEvent = new LaunchRegisteredIntegrationEvent(
            launch.Id,
            launch.Amount,
            launch.Type.ToString(),
            launch.OccurredOnUtc);

        await integrationEventPublisher.PublishRegisteredAsync(integrationEvent, cancellationToken);

        logger.LogInformation(
            "Launch integration event published successfully. LaunchId: {LaunchId}",
            launch.Id);

        return Result<Guid>.Success(launch.Id);
    }

    private static Result<LaunchType> ParseLaunchType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return Result<LaunchType>.Failure(LaunchErrors.InvalidType);

        var normalizedType = type.Trim();

        if (Enum.TryParse<LaunchType>(normalizedType, ignoreCase: true, out var launchType) &&
            Enum.IsDefined(launchType))
        {
            return Result<LaunchType>.Success(launchType);
        }

        return Result<LaunchType>.Failure(LaunchErrors.InvalidType);
    }
}