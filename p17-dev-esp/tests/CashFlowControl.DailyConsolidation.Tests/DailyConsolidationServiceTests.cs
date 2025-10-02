using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Application.Services;
using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace DailyConsolidation.Tests;

public class DailyConsolidationServiceTests
{
    private readonly Mock<ITransactionHttpClientService> _mockTransactionHttpClientService;
    private readonly Mock<IConsolidatedBalanceRepository> _mockBalanceRepo;
    private readonly Mock<ILogger<DailyConsolidationService>> _mockLogger;
    private readonly IDailyConsolidationService _service;
    private Mock<IMediator> _mediator;

    public DailyConsolidationServiceTests()
    {
        _mockTransactionHttpClientService = new Mock<ITransactionHttpClientService>();
        _mockBalanceRepo = new Mock<IConsolidatedBalanceRepository>();
        _mockLogger = new Mock<ILogger<DailyConsolidationService>>();
        _mediator = new Mock<IMediator>();

        _service = new DailyConsolidationService(_mockTransactionHttpClientService.Object, _mockBalanceRepo.Object, _mockLogger.Object, _mediator.Object);
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldCreateBalance_WhenTransactionIsProcessed()
    {
        // Arrange
        var transaction = new CreateTransactionDTO
        {
            Id = Guid.NewGuid(),
            Amount = 100,
            Type = TransactionType.Credit.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _mockBalanceRepo
            .Setup(repo => repo.GetBalanceByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((ConsolidatedBalance?)null); 

        // Act
        await _service.ProcessTransactionAsync(transaction);

        // Assert
        _mockBalanceRepo.Verify(repo => repo.CreateBalanceAsync(It.IsAny<ConsolidatedBalance>()), Times.Once);
    }


    [Fact]
    public async Task ConsolidateDailyBalanceAsync_ShouldCreateBalanceForDay_WhenCalled()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        _mockTransactionHttpClientService.Setup(api => api.GetTransactionsByDateAsync(date))
            .ReturnsAsync(new List<Transaction> { new Transaction { Amount = 100, Type = "Credit", CreatedAt = date } });

        // Act
        await _service.ConsolidateDailyBalanceAsync(date);

        // Assert
        _mockBalanceRepo.Verify(repo => repo.CreateBalanceAsync(It.IsAny<ConsolidatedBalance>()), Times.Once);
    }
}


