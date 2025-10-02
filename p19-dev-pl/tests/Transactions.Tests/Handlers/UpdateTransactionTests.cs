using Application.Transaction.Handlers;
using Domain.Entities;
using Moq;
using Shared.Enums;
using Shared.Messages;

namespace Transactions.Tests.Handlers;

[TestFixture]
public class UpdateTransactionHandlerTests : BaseTests
{
    private UpdateTransaction _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _handler = new UpdateTransaction(DbContext, PublishEndpointMock.Object);
    }

    [Test]
    public async Task Handle_ExistingTransaction_ShouldUpdateTransactionAndPublishEvent()
    {
        // Arrange
        var transaction = TransactionEntity.Create(100.00m, TransactionType.Credit);
        DbContext.Transactions.Add(transaction);
        await DbContext.SaveChangesAsync();

        var command = new UpdateTransaction.UpdateTransactionCommand(transaction.Id, 250.00m, TransactionType.Debit);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);

        var updatedTransaction = await DbContext.Transactions.FindAsync(transaction.Id);
        Assert.That(updatedTransaction, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedTransaction!.Amount, Is.EqualTo(250.00m));
            Assert.That(updatedTransaction.Type, Is.EqualTo(TransactionType.Debit));
            Assert.That(updatedTransaction.UpdatedAt, Is.Not.Null);
        });

        PublishEndpointMock.Verify(x =>
            x.Publish(It.IsAny<TransactionUpdated>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_NonExistingTransaction_ShouldReturnFalse()
    {
        // Arrange
        var command = new UpdateTransaction.UpdateTransactionCommand(Guid.NewGuid(), 300.00m, TransactionType.Credit);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);

        PublishEndpointMock.Verify(x =>
            x.Publish(It.IsAny<TransactionUpdated>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}