using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using Shared.DTOs;
using Transactions.API.Application.Services;
using Transactions.API.Domain.Models;
using Transactions.API.src.Domain.Interfaces;
using Xunit;

namespace Transactions.API.Tests.Unit.Application;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<TransactionService>> _mockLogger;
    private readonly TransactionService _transaction_service;

    public TransactionServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<TransactionService>>();

        _transaction_service = new TransactionService(
            _mockUnitOfWork.Object,
            _mockPublishEndpoint.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnTransactionDto()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            Lojista = Shared.Enums.Lojista.LojaNorte,
            Amount = 100.00m,
            Type = "Credit",
            Description = "Test",
            TransactionDate = DateTime.UtcNow
        };

        // Mock setup
        _mockUnitOfWork.Setup(x => x.Transactions.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction t, CancellationToken ct) => t);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transaction_service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(100.00m);
        result.Type.Should().Be("Credit");
        result.IsProcessed.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Transactions.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = Transaction.Create(Shared.Enums.Lojista.LojaNorte, 50m, TransactionType.Debit, "Test", DateTime.UtcNow);

        _mockUnitOfWork.Setup(x => x.Transactions.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transaction_service.GetByIdAsync(transactionId);

        // Assert
        result.Should().NotBeNull();
        result?.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mockUnitOfWork.Setup(x => x.Transactions.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _transaction_service.GetByIdAsync(transactionId);

        // Assert
        result.Should().BeNull();
    }
}
