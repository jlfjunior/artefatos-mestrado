using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Commands;
using CashFlow.Consolidation.Worker.Consumers;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Consolidation.UnitTests.Worker;

public sealed class LaunchRegisteredConsumerTests
{
    [Fact]
    public async Task ConsumeAsync_ShouldSucceed_WhenCommandSucceeds()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<LaunchRegisteredConsumer>>();
        var integrationEvent = new LaunchRegisteredIntegrationEvent(
            Guid.NewGuid(),
            10m,
            "credit",
            DateTime.UtcNow);
        sender.Send(
                Arg.Is<ProcessLaunchRegisteredEventCommand>(command =>
                    command.LaunchId == integrationEvent.LaunchId &&
                    command.Amount == integrationEvent.Amount &&
                    command.Type == integrationEvent.Type &&
                    command.OccurredOnUtc == integrationEvent.OccurredOnUtc),
                Arg.Is<CancellationToken>(token => token == CancellationToken.None))
            .Returns(Result.Success());
        var consumer = new LaunchRegisteredConsumer(sender, logger);

        // Act
        var result = await consumer.ConsumeAsync(integrationEvent, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConsumeAsync_ShouldReturnFailure_WhenCommandFails()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<LaunchRegisteredConsumer>>();
        var integrationEvent = new LaunchRegisteredIntegrationEvent(
            Guid.NewGuid(),
            10m,
            "credit",
            DateTime.UtcNow);
        var error = new Error("consolidation.failed", "failure", ErrorType.Unexpected);
        sender.Send(
                Arg.Is<ProcessLaunchRegisteredEventCommand>(command =>
                    command.LaunchId == integrationEvent.LaunchId &&
                    command.Amount == integrationEvent.Amount &&
                    command.Type == integrationEvent.Type &&
                    command.OccurredOnUtc == integrationEvent.OccurredOnUtc),
                Arg.Is<CancellationToken>(token => token == CancellationToken.None))
            .Returns(Result.Failure(error));
        var consumer = new LaunchRegisteredConsumer(sender, logger);

        // Act
        var result = await consumer.ConsumeAsync(integrationEvent, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
