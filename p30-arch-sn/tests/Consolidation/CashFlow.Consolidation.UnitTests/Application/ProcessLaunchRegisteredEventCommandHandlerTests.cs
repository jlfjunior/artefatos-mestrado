using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.Consolidation.Application.CommandHandlers;
using CashFlow.Consolidation.Application.Commands;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Errors;
using CashFlow.Consolidation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Consolidation.UnitTests.Application;

public sealed class ProcessLaunchRegisteredEventCommandHandlerTests
{
    private readonly IDailyConsolidationRepository _dailyRepository = Substitute.For<IDailyConsolidationRepository>();
    private readonly IProcessedLaunchEventRepository _processedRepository = Substitute.For<IProcessedLaunchEventRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<ProcessLaunchRegisteredEventCommandHandler> _logger = Substitute.For<ILogger<ProcessLaunchRegisteredEventCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldSkipPersistence_WhenEventWasAlreadyProcessed()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(true);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 10m, "credit", DateTime.UtcNow),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _dailyRepository.Received(0).GetByDateForUpdateAsync(
            Arg.Any<DateOnly>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _dailyRepository.Received(0).AddAsync(
            Arg.Any<DailyConsolidation>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _processedRepository.Received(0).AddAsync(
            Arg.Any<ProcessedLaunchEvent>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(0).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCreateDailyConsolidationPersistProcessedEventAndSave_WhenNoConsolidationExists()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var dateTime = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);
        var expectedDate = DateOnly.FromDateTime(dateTime);
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(false);
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == expectedDate),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns((DailyConsolidation?)null);

        decimal totalCreditsWhenAdded = -1m;
        decimal totalDebitsWhenAdded = -1m;
        _dailyRepository
            .When(r => r.AddAsync(Arg.Any<DailyConsolidation>(), Arg.Any<CancellationToken>()))
            .Do(call =>
            {
                var c = call.Arg<DailyConsolidation>();
                totalCreditsWhenAdded = c.TotalCredits;
                totalDebitsWhenAdded = c.TotalDebits;
            });

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 10m, "credit", dateTime),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        totalCreditsWhenAdded.Should().Be(0m, because: "fresh consolidation is added before credit is applied");
        totalDebitsWhenAdded.Should().Be(0m);
        await _dailyRepository.Received(1).AddAsync(
            Arg.Is<DailyConsolidation>(x => x.Date == expectedDate),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _processedRepository.Received(1).AddAsync(
            Arg.Is<ProcessedLaunchEvent>(x => x.LaunchId == launchId),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldApplyCredit_WhenConsolidationExists()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 0m, 0m).Value;
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(false);
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == new DateOnly(2026, 4, 21)),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(consolidation);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 50m, "credit", new DateTime(2026, 4, 21, 8, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        consolidation.TotalCredits.Should().Be(50m);
        consolidation.Balance.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_ShouldApplyDebit_WhenConsolidationExists()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 80m, 0m).Value;
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(false);
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == new DateOnly(2026, 4, 21)),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(consolidation);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 20m, "debit", new DateTime(2026, 4, 21, 8, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        consolidation.TotalDebits.Should().Be(20m);
        consolidation.Balance.Should().Be(60m);
    }

    [Fact]
    public async Task Handle_ShouldRegisterProcessedLaunchEventAndSave_WhenFlowCompletes()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 0m, 0m).Value;
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(false);
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == new DateOnly(2026, 4, 21)),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(consolidation);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 15m, "credit", new DateTime(2026, 4, 21, 8, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _processedRepository.Received(1).AddAsync(
            Arg.Is<ProcessedLaunchEvent>(x => x.LaunchId == launchId),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWithoutPersistingProcessedEvent_WhenTypeIsInvalid()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 0m, 0m).Value;
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(false);
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == new DateOnly(2026, 4, 21)),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(consolidation);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 15m, "invalid", new DateTime(2026, 4, 21, 8, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DailyConsolidationErrors.InvalidLaunchType);
        await _processedRepository.Received(0).AddAsync(
            Arg.Any<ProcessedLaunchEvent>(),
            Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
        await _unitOfWork.Received(0).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenSaveChangesThrowsProcessedLaunchUniqueConstraint()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 0m, 0m).Value;
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(false);
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == new DateOnly(2026, 4, 21)),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(consolidation);
        _unitOfWork
            .When(x => x.SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None)))
            .Do(_ => throw new Exception("SQLite Error 19: 'UNIQUE constraint failed: processed_launch_events.LaunchId'."));
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ProcessLaunchRegisteredEventCommand(launchId, 15m, "credit", new DateTime(2026, 4, 21, 8, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldNotDuplicateBalance_WhenInvokedTwiceForSameLaunchId()
    {
        // Arrange
        var launchId = Guid.NewGuid();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 0m, 0m).Value;
        var existsSequence = new Queue<bool>(new[] { false, true });
        _processedRepository.ExistsByLaunchIdAsync(
                Arg.Is<Guid>(id => id == launchId),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(_ => existsSequence.Dequeue());
        _dailyRepository.GetByDateForUpdateAsync(
                Arg.Is<DateOnly>(d => d == new DateOnly(2026, 4, 21)),
                Arg.Is<CancellationToken>(ct => ct == CancellationToken.None))
            .Returns(consolidation);
        var handler = CreateHandler();
        var command = new ProcessLaunchRegisteredEventCommand(
            launchId,
            40m,
            "credit",
            new DateTime(2026, 4, 21, 8, 0, 0, DateTimeKind.Utc));

        // Act
        var firstResult = await handler.Handle(command, CancellationToken.None);
        var secondResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        consolidation.TotalCredits.Should().Be(40m);
        consolidation.Balance.Should().Be(40m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Is<CancellationToken>(ct => ct == CancellationToken.None));
    }

    private ProcessLaunchRegisteredEventCommandHandler CreateHandler()
    {
        return new ProcessLaunchRegisteredEventCommandHandler(_dailyRepository, _processedRepository, _unitOfWork, _logger);
    }
}
