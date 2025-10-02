using Application.Consumers;
using Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Shared.Enums;
using Shared.Messages;

namespace DailySummary.Tests.Consumers;

[TestFixture]
public class TransactionCreatedConsumerTests : BaseTests
{
    private TransactionCreatedConsumer _consumer = null!;
    private Mock<IDistributedCache> _cacheMock = null!;

    [SetUp]
    public void SetUp()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _consumer = new TransactionCreatedConsumer(DbContext, _cacheMock.Object);
    }

    [Test]
    public async Task Consume_WhenTransactionDoesNotExist_ShouldCreateTransactionAndUpdateSummary()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var amount = 150.00m;
        var type = TransactionType.Credit;

        var message = new TransactionCreated(transactionId, amount, type, createdAt);
        var contextMock = new Mock<ConsumeContext<TransactionCreated>>();
        contextMock.Setup(c => c.Message).Returns(message);

        var expectedCacheKey = $"daily-summary:{createdAt:yyyy-MM-dd}";

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var createdTransaction = await DbContext.DailyTransactions.FirstOrDefaultAsync(t => t.Id == transactionId);
        Assert.That(createdTransaction, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdTransaction!.Amount, Is.EqualTo(amount));
            Assert.That(createdTransaction.Type, Is.EqualTo(type));
        });

        var createdSummary = await DbContext.DailySummaries.FirstOrDefaultAsync();
        Assert.That(createdSummary, Is.Not.Null);

        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(key => key == expectedCacheKey), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_WhenTransactionAlreadyExists_ShouldNotCreateOrUpdate()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var amount = 100.00m;
        var type = TransactionType.Debit;

        var existingTransaction = DailyTransactionEntity.Create(transactionId, createdAt, amount, type);

        DbContext.DailyTransactions.Add(existingTransaction);
        await DbContext.SaveChangesAsync();

        var message = new TransactionCreated(transactionId, amount, type, createdAt);
        var contextMock = new Mock<ConsumeContext<TransactionCreated>>();
        contextMock.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var transactionCount = await DbContext.DailyTransactions.CountAsync();
        Assert.That(transactionCount, Is.EqualTo(1));

        var summaryCount = await DbContext.DailySummaries.CountAsync();
        Assert.That(summaryCount, Is.EqualTo(0));

        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Consume_WhenTransactionExistsOnSameDate_ShouldUpdateSummary()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.Date;
        var amount = 200.00m;
        var type = TransactionType.Credit;

        var existingSummary = DailySummaryEntity.Create(createdAt, 500.00m, 300.00m);
        DbContext.DailySummaries.Add(existingSummary);
        await DbContext.SaveChangesAsync();

        var message = new TransactionCreated(transactionId, amount, type, createdAt);
        var contextMock = new Mock<ConsumeContext<TransactionCreated>>();
        contextMock.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var updatedSummary = await DbContext.DailySummaries.FirstOrDefaultAsync(s => s.Date == createdAt);
        Assert.That(updatedSummary, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(updatedSummary!.TotalCredits, Is.EqualTo(700.00m));
            Assert.That(updatedSummary.TotalDebits, Is.EqualTo(300.00m));
        });

        var transactionCount = await DbContext.DailyTransactions.CountAsync();
        Assert.That(transactionCount, Is.EqualTo(1));

        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}