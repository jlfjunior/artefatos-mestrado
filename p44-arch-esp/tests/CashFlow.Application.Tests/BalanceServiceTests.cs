using CashFlow.Application.Abstractions;
using CashFlow.Application.Exceptions;
using CashFlow.Application.Services;
using CashFlow.Domain.Entities;
using CashFlow.Shared.Authorization;
using FluentAssertions;
using Moq;

namespace CashFlow.Application.Tests;

public class BalanceServiceTests
{
    private readonly Mock<IDailyBalanceRepository> _repository = new();
    private readonly Mock<ICurrentUserContext> _currentUser = new();
    private readonly BalanceService _sut;

    public BalanceServiceTests()
    {
        _sut = new BalanceService(_repository.Object, _currentUser.Object);
    }

    [Fact]
    public async Task GetByDateAsync_ShouldReturnMappedBalance_WhenRecordExists()
    {
        var date = new DateOnly(2026, 6, 13);
        var balance = new DailyBalance
        {
            UserId = SeedIdentifiers.ClientUserId,
            BalanceDate = date,
            TotalCredits = 200,
            TotalDebits = 50,
            Balance = 150,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _repository
            .Setup(r => r.GetByDateAsync(SeedIdentifiers.ClientUserId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var result = await _sut.GetByDateAsync(SeedIdentifiers.ClientUserId, date);

        result.TotalCredits.Should().Be(200);
        result.TotalDebits.Should().Be(50);
        result.Balance.Should().Be(150);
    }

    [Fact]
    public async Task GetByDateAsync_ShouldReturnEmptyBalance_WhenRecordDoesNotExist()
    {
        var date = new DateOnly(2026, 6, 13);

        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _repository
            .Setup(r => r.GetByDateAsync(SeedIdentifiers.ClientUserId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);
        _repository
            .Setup(r => r.GetLastBeforeDateAsync(SeedIdentifiers.ClientUserId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        var result = await _sut.GetByDateAsync(SeedIdentifiers.ClientUserId, date);

        result.TotalCredits.Should().Be(0);
        result.TotalDebits.Should().Be(0);
        result.Balance.Should().Be(0);
    }

    [Fact]
    public async Task GetByDateAsync_ShouldCarryForwardPreviousBalance_WhenDateHasNoMovements()
    {
        var date = new DateOnly(2026, 6, 15);
        var previous = new DailyBalance
        {
            UserId = SeedIdentifiers.ClientUserId,
            BalanceDate = new DateOnly(2026, 6, 10),
            TotalCredits = 200,
            TotalDebits = 50,
            Balance = 150,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _repository
            .Setup(r => r.GetByDateAsync(SeedIdentifiers.ClientUserId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);
        _repository
            .Setup(r => r.GetLastBeforeDateAsync(SeedIdentifiers.ClientUserId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);

        var result = await _sut.GetByDateAsync(SeedIdentifiers.ClientUserId, date);

        result.Date.Should().Be(date);
        result.TotalCredits.Should().Be(0);
        result.TotalDebits.Should().Be(0);
        result.Balance.Should().Be(150);
    }

    [Fact]
    public async Task GetByDateAsync_ShouldThrowForbidden_WhenClientAccessesOtherUser()
    {
        _currentUser.SetupGet(u => u.IsClient).Returns(true);
        _currentUser.SetupGet(u => u.UserId).Returns(SeedIdentifiers.ClientUserId);

        var act = () => _sut.GetByDateAsync(SeedIdentifiers.EmployeeUserId, new DateOnly(2026, 6, 13));

        await act.Should().ThrowAsync<ForbiddenException>()
            .Where(ex => ex.Error == "access_denied");
    }

    [Fact]
    public async Task ListAsync_ShouldClampPageSize_ToMaximum()
    {
        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _repository
            .Setup(r => r.ListAsync(
                SeedIdentifiers.ClientUserId,
                1,
                100,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<DailyBalance>(), 0));

        await _sut.ListAsync(SeedIdentifiers.ClientUserId, page: 1, pageSize: 500, from: null, to: null);

        _repository.Verify(r => r.ListAsync(
            SeedIdentifiers.ClientUserId,
            1,
            100,
            null,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTodayAsync_ShouldDelegateToGetByDateAsync()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var balance = new DailyBalance
        {
            UserId = SeedIdentifiers.ClientUserId,
            BalanceDate = date,
            TotalCredits = 10,
            TotalDebits = 2,
            Balance = 8,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _repository
            .Setup(r => r.GetByDateAsync(SeedIdentifiers.ClientUserId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var result = await _sut.GetTodayAsync(SeedIdentifiers.ClientUserId);

        result.Balance.Should().Be(8);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPaginatedResponse()
    {
        var balance = new DailyBalance
        {
            UserId = SeedIdentifiers.ClientUserId,
            BalanceDate = new DateOnly(2026, 6, 1),
            TotalCredits = 50,
            TotalDebits = 10,
            Balance = 40,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _repository
            .Setup(r => r.ListAsync(
                SeedIdentifiers.ClientUserId,
                2,
                10,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { balance }, 25));

        var result = await _sut.ListAsync(SeedIdentifiers.ClientUserId, page: 2, pageSize: 10, from: null, to: null);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.Items.Should().ContainSingle();
    }
}
