using Application.Transaction.Handlers;
using Domain.Entities;
using Moq;
using Shared.Enums;
using Shared.Messages;

namespace Transactions.Tests.Handlers;

[TestFixture]
public class DeleteTransactionHandlerTests : BaseTests
{
    private DeleteTransaction _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _handler = new DeleteTransaction(DbContext, PublishEndpointMock.Object);
    }

    [Test]
    public async Task Handle_ExistingTransaction_ShouldDeleteTransactionAndPublishEvent()
    {
        // Arrange
        var transaction = TransactionEntity.Create(100.00m, TransactionType.Credit);
        DbContext.Transactions.Add(transaction);
        await DbContext.SaveChangesAsync();

        var command = new DeleteTransaction.DeleteTransactionCommand(transaction.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);

        var deletedTransaction = await DbContext.Transactions.FindAsync(transaction.Id);
        Assert.That(deletedTransaction, Is.Null);

        PublishEndpointMock.Verify(x =>
            x.Publish(It.IsAny<TransactionDeleted>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_NonExistingTransaction_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeleteTransaction.DeleteTransactionCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);

        PublishEndpointMock.Verify(x =>
            x.Publish(It.IsAny<TransactionDeleted>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
