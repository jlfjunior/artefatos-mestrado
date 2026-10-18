using CashFlow.Application.Abstractions;
using CashFlow.Contracts;
using CashFlow.Application.Exceptions;
using CashFlow.Application.Services;
using CashFlow.Shared.Authorization;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Messaging;
using FluentAssertions;
using Moq;

namespace CashFlow.Application.Tests;

/// <summary>
/// Testes de casos de uso de lançamentos com TDD e mocks de portas.
/// </summary>
public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _repository = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IMessagePublisher> _publisher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserContext> _currentUser = new();
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        _unitOfWork
            .Setup(u => u.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));

        _currentUser.SetupGet(u => u.IsEmployee).Returns(true);
        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _currentUser.SetupGet(u => u.UserId).Returns(SeedIdentifiers.EmployeeUserId);

        _users.Setup(u => u.GetByIdAsync(SeedIdentifiers.ClientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.User
            {
                Id = SeedIdentifiers.ClientUserId,
                Email = SeedIdentifiers.ClientEmail,
                FullName = "Cliente Demo",
                Role = UserRole.Client,
                Enabled = true,
                CreatedAt = DateTime.UtcNow
            });

        _sut = new TransactionService(
            _repository.Object,
            _users.Object,
            _publisher.Object,
            _unitOfWork.Object,
            _currentUser.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndPublishMessage()
    {
        var request = new CreateTransactionRequest(
            SeedIdentifiers.ClientUserId,
            "Venda",
            150m,
            "Credit");

        var result = await _sut.CreateAsync(request);

        result.Description.Should().Be("Venda");
        result.UserId.Should().Be(SeedIdentifiers.ClientUserId);
        result.CreatedBy.Should().Be(SeedIdentifiers.EmployeeUserId);
        _repository.Verify(r => r.InsertAsync(It.IsAny<Domain.Entities.Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync(
            It.Is<TransactionCreatedMessage>(m =>
                m.Data.Amount == 150m &&
                m.Data.UserId == SeedIdentifiers.ClientUserId &&
                m.Data.CreatedBy == SeedIdentifiers.EmployeeUserId &&
                m.Data.EntryType == EntryType.Credit &&
                m.Type == EventExchanges.TransactionCreated),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForbidden_WhenNotEmployee()
    {
        _currentUser.SetupGet(u => u.IsEmployee).Returns(false);

        var act = () => _sut.CreateAsync(new CreateTransactionRequest(
            SeedIdentifiers.ClientUserId,
            "Venda",
            10m,
            "Credit"));

        await act.Should().ThrowAsync<ForbiddenException>()
            .Where(ex => ex.Error == "employee_required");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidation_WhenOwnerIsInvalid()
    {
        _users.Setup(u => u.GetByIdAsync(SeedIdentifiers.ClientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        var act = () => _sut.CreateAsync(new CreateTransactionRequest(
            SeedIdentifiers.ClientUserId,
            "Venda",
            10m,
            "Credit"));

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Error == "invalid_user_id");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidation_WhenEntryTypeIsInvalid()
    {
        var act = () => _sut.CreateAsync(new CreateTransactionRequest(
            SeedIdentifiers.ClientUserId,
            "Venda",
            10m,
            "Invalid"));

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Error == "invalid_entry_type");
    }

    [Fact]
    public async Task ListAsync_ShouldFilterByClientUser_WhenCallerIsClient()
    {
        var transaction = Domain.Entities.Transaction.Create(
            "Compra",
            25m,
            EntryType.Debit,
            new DateOnly(2026, 6, 13),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId);

        _currentUser.SetupGet(u => u.IsClient).Returns(true);
        _currentUser.SetupGet(u => u.UserId).Returns(SeedIdentifiers.ClientUserId);
        _repository.Setup(r => r.ListAsync(null, null, SeedIdentifiers.ClientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { transaction });

        var result = await _sut.ListAsync(null, null);

        result.Should().ContainSingle()
            .Which.Description.Should().Be("Compra");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Transaction?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
