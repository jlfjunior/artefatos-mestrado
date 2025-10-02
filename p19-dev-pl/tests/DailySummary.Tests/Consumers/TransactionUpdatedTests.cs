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
public class TransactionUpdatedConsumerTests : BaseTests
{
    private TransactionUpdatedConsumer _consumer = null!;
    private Mock<IDistributedCache> _cacheMock = null!;

    [SetUp]
    public void SetUp()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _consumer = new TransactionUpdatedConsumer(DbContext, _cacheMock.Object);
    }

    [Test]
    public async Task Consume_WhenSummaryExists_ShouldUpdateSummary()
    {
        // Arrange
        var updatedAt = DateTime.UtcNow.Date;
        var originalAmount = 500.00m;
        var newAmount = 200.00m;
        var oldType = TransactionType.Credit;
        var newType = TransactionType.Debit;

        var summary = DailySummaryEntity.Create(updatedAt, 500.00m, 300.00m);
        DbContext.DailySummaries.Add(summary);

        var transaction = DailyTransactionEntity.Create(Guid.NewGuid(), updatedAt, originalAmount, oldType);
        DbContext.DailyTransactions.Add(transaction);

        await DbContext.SaveChangesAsync();

        var message = new TransactionUpdated(transaction.Id, newAmount, newType, updatedAt);
        var contextMock = new Mock<ConsumeContext<TransactionUpdated>>();
        contextMock.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var updatedSummary = await DbContext.DailySummaries.FirstOrDefaultAsync(s => s.Date == updatedAt);
        Assert.That(updatedSummary, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedSummary!.TotalCredits, Is.EqualTo(500.00m - originalAmount));
            Assert.That(updatedSummary.TotalDebits, Is.EqualTo(300.00m + newAmount));
        });

        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(key => key == $"daily-summary:{updatedAt:yyyy-MM-dd}"),
            It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task Consume_WhenSummaryDoesNotExist_ShouldCreateNewSummary()
    {
        // Arrange
        var updatedAt = DateTime.UtcNow.Date;
        var amount = 300.00m;
        var type = TransactionType.Credit;
        var transactionId = Guid.NewGuid();

        var transaction = DailyTransactionEntity.Create(transactionId, updatedAt, amount, type);
        DbContext.DailyTransactions.Add(transaction);
        await DbContext.SaveChangesAsync();

        var message = new TransactionUpdated(transactionId, amount, type, updatedAt);
        var contextMock = new Mock<ConsumeContext<TransactionUpdated>>();
        contextMock.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        var summary = await DbContext.DailySummaries.FirstOrDefaultAsync(s => s.Date == updatedAt);
        Assert.That(summary, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(summary!.TotalCredits, Is.EqualTo(amount));
            Assert.That(summary.TotalDebits, Is.EqualTo(0));
        });

        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(key => key == $"daily-summary:{updatedAt:yyyy-MM-dd}"),
            It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}