using CashFlow.Application.Abstractions;
using CashFlow.Application.Services;
using CashFlow.Shared.Authorization;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Messaging;
using Moq;

namespace CashFlow.Application.Tests;

public class DailyBalanceProjectorTests
{
    private readonly Mock<IDailyBalanceRepository> _repository = new();
    private readonly DailyBalanceProjector _sut;

    public DailyBalanceProjectorTests()
    {
        _sut = new DailyBalanceProjector(_repository.Object);
    }

    [Fact]
    public async Task ProjectAsync_ShouldApplyCreditDelta_ForCreditEntry()
    {
        var transactionId = Guid.NewGuid();
        var date = new DateOnly(2026, 6, 13);
        var message = TransactionCreatedMessage.Create(
            transactionId,
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId,
            "Venda",
            100m,
            EntryType.Credit,
            date);

        var eventId = Guid.Parse(message.Id);

        await _sut.ProjectAsync(message);

        _repository.Verify(r => r.ApplyDeltaAsync(
            SeedIdentifiers.ClientUserId,
            date,
            100m,
            0m,
            eventId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProjectAsync_ShouldApplyDebitDelta_ForDebitEntry()
    {
        var transactionId = Guid.NewGuid();
        var date = new DateOnly(2026, 6, 13);
        var message = TransactionCreatedMessage.Create(
            transactionId,
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId,
            "Despesa",
            75m,
            EntryType.Debit,
            date);

        var eventId = Guid.Parse(message.Id);

        await _sut.ProjectAsync(message);

        _repository.Verify(r => r.ApplyDeltaAsync(
            SeedIdentifiers.ClientUserId,
            date,
            0m,
            75m,
            eventId,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
