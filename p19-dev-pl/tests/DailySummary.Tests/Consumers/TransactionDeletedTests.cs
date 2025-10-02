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
public class TransactionDeletedConsumerTests : BaseTests
{
    private TransactionDeletedConsumer _consumer = null!;
    private Mock<IDistributedCache> _cacheMock = null!;

    [SetUp]
    public void SetUp()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _consumer = new TransactionDeletedConsumer(DbContext, _cacheMock.Object);
    }

    [Test]
    public async Task Consume_WhenTransactionExists_ShouldRemoveTransactionAndUpdateSummary()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var amount = 150.00m;
        var type = TransactionType.Credit;

        var transaction = DailyTransactionEntity.Create(transactionId, date, amount, type);
        var summary = DailySummaryEntity.Create(date, 150.00m, 50.00m);

        DbContext.DailyTransactions.Add(transaction);
        DbContext.DailySummaries.Add(summary);
        await DbContext.SaveChangesAsync();

        var message = new TransactionDeleted(transactionId);
        var contextMock = new Mock<ConsumeContext<TransactionDeleted>>();
        contextMock.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var deletedTransaction = await DbContext.DailyTransactions.FindAsync(transactionId);
        Assert.That(deletedTransaction, Is.Null);

        var updatedSummary = await DbContext.DailySummaries.FirstOrDefaultAsync(s => s.Date == date);
        Assert.That(updatedSummary, Is.Not.Null);
        Assert.That(updatedSummary!.TotalCredits, Is.EqualTo(0));

        var transactionCount = await DbContext.DailyTransactions.CountAsync();
        Assert.That(transactionCount, Is.EqualTo(0));

        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(key => key == $"daily-summary:{date:yyyy-MM-dd}"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_WhenTransactionDoesNotExist_ShouldNotChangeDatabase()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var message = new TransactionDeleted(transactionId);
        var contextMock = new Mock<ConsumeContext<TransactionDeleted>>();
        contextMock.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var transactionCount = await DbContext.DailyTransactions.CountAsync();
        Assert.That(transactionCount, Is.EqualTo(0));

        var summaryCount = await DbContext.DailySummaries.CountAsync();
        Assert.That(summaryCount, Is.EqualTo(0));

        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
