using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Launches.Application.CommandHandlers;
using CashFlow.Launches.Application.Commands;
using CashFlow.Launches.Application.Interfaces;
using CashFlow.Launches.Domain.Entities;
using CashFlow.Launches.Domain.Enums;
using CashFlow.Launches.Domain.Errors;
using CashFlow.Launches.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Launches.UnitTests.Application;

public sealed class RegisterLaunchCommandHandlerTests
{
    private readonly ILaunchRepository _launchRepository = Substitute.For<ILaunchRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILaunchIntegrationEventPublisher _publisher = Substitute.For<ILaunchIntegrationEventPublisher>();
    private readonly ILogger<RegisterLaunchCommandHandler> _logger = Substitute.For<ILogger<RegisterLaunchCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldPersistLaunchSaveChangesAndPublishEventInOrder_WhenCommandIsValid()
    {
        // Arrange
        var handler = CreateHandler();
        var occurredOnUtc = DateTime.UtcNow;
        var command = new RegisterLaunchCommand(125m, "credit", occurredOnUtc);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _launchRepository.Received(1).AddAsync(
            Arg.Is<Launch>(launch =>
                launch.Amount == 125m &&
                launch.Type == LaunchType.Credit &&
                launch.OccurredOnUtc == occurredOnUtc),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _publisher.Received(1).PublishRegisteredAsync(
            Arg.Is<LaunchRegisteredIntegrationEvent>(evt =>
                evt.Amount == 125m &&
                evt.Type == "Credit" &&
                evt.OccurredOnUtc == occurredOnUtc),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));

        Received.InOrder(async () =>
        {
            await _launchRepository.AddAsync(
                Arg.Is<Launch>(launch =>
                    launch.Amount == 125m &&
                    launch.Type == LaunchType.Credit &&
                    launch.OccurredOnUtc == occurredOnUtc),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
            await _unitOfWork.SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
            await _publisher.PublishRegisteredAsync(
                Arg.Is<LaunchRegisteredIntegrationEvent>(evt =>
                    evt.Amount == 125m &&
                    evt.Type == "Credit" &&
                    evt.OccurredOnUtc == occurredOnUtc),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        });
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTypeIsInvalid()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new RegisterLaunchCommand(100m, "other", DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LaunchErrors.InvalidType);
        await _launchRepository.Received(0).AddAsync(
            Arg.Any<Launch>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(0).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _publisher.Received(0).PublishRegisteredAsync(
            Arg.Any<LaunchRegisteredIntegrationEvent>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAmountIsInvalid()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new RegisterLaunchCommand(0m, "credit", DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LaunchErrors.InvalidAmount);
        await _launchRepository.Received(0).AddAsync(
            Arg.Any<Launch>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(0).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    private RegisterLaunchCommandHandler CreateHandler()
    {
        return new RegisterLaunchCommandHandler(_launchRepository, _unitOfWork, _publisher, _logger);
    }
}
