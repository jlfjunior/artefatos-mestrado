using Consolidation.API.Application.Services;
using Consolidation.API.Domain.Interfaces;
using Consolidation.API.src.Domain.Interfaces;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using Xunit;

namespace Consolidation.API.Tests.Unit.Application;

public class ConsolidationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<ConsolidationService>> _mockLogger;
    private readonly ConsolidationService _consolidation_service;

    public ConsolidationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<ConsolidationService>>();

        _consolidation_service = new ConsolidationService(
            _mockUnitOfWork.Object,
            _mockPublishEndpoint.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WithNewDate_ShouldCreateConsolidation()
    {
        // Arrange
        var msg = new TransactionCreatedEvent
        {
            TransactionId = Guid.NewGuid(),
            Lojista = Shared.Enums.Lojista.LojaNorte,
            Amount = 100.00m,
            Type = "Credit",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var mockRepository = new Mock<IConsolidationRepository>();
        mockRepository.Setup(x => x.GetByLojistaAsync(It.IsAny<Shared.Enums.Lojista>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Consolidation.API.Domain.Models.Consolidation?)null);

        mockRepository.Setup(x => x.AddAsync(It.IsAny<Consolidation.API.Domain.Models.Consolidation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Consolidation.API.Domain.Models.Consolidation c, CancellationToken ct) => c);

        mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Consolidation.API.Domain.Models.Consolidation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Consolidation.API.Domain.Models.Consolidation c, CancellationToken ct) => c);

        _mockUnitOfWork.Setup(x => x.Consolidations).Returns(mockRepository.Object);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _consolidation_service.ProcessTransactionAsync(msg);

        // Assert
        result.Should().NotBeNull();
        result.CreditTotal.Should().Be(100.00m);
        result.ProcessedCount.Should().Be(1);

        mockRepository.Verify(x => x.AddAsync(It.IsAny<Consolidation.API.Domain.Models.Consolidation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WithExistingConsolidation_ShouldUpdate()
    {
        // Arrange
        var existingConsolidation = Consolidation.API.Domain.Models.Consolidation.Create(Shared.Enums.Lojista.LojaNorte, DateTime.UtcNow);
        existingConsolidation.AddTransaction(50m, "Debit");

        var msg = new TransactionCreatedEvent
        {
            TransactionId = Guid.NewGuid(),
            Lojista = Shared.Enums.Lojista.LojaNorte,
            Amount = 100.00m,
            Type = "Credit",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var mockRepository = new Mock<IConsolidationRepository>();
        mockRepository.Setup(x => x.GetByLojistaAsync(It.IsAny<Shared.Enums.Lojista>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsolidation);

        mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Consolidation.API.Domain.Models.Consolidation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Consolidation.API.Domain.Models.Consolidation c, CancellationToken ct) => c);

        _mockUnitOfWork.Setup(x => x.Consolidations).Returns(mockRepository.Object);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _consolidation_service.ProcessTransactionAsync(msg);

        // Assert
        result.Should().NotBeNull();
        result.CreditTotal.Should().Be(100.00m);
        result.DebitTotal.Should().Be(50.00m);
        result.DailyBalance.Should().Be(50.00m);
        result.ProcessedCount.Should().Be(2);
    }
}
