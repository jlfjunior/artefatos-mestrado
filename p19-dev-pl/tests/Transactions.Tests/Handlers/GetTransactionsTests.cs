using Application.Transaction.Handlers;
using Domain.Entities;
using Shared.Enums;

namespace Transactions.Tests.Handlers;

[TestFixture]
public class GetTransactionsHandlerTests : BaseTests
{
    private GetTransactions _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _handler = new GetTransactions(DbContext, Mapper);
    }

    [Test]
    public async Task Handle_WhenTransactionsExist_ShouldReturnTransactionDtos()
    {
        // Arrange
        var transaction1 = TransactionEntity.Create(200.00m, TransactionType.Credit);
        var transaction2 = TransactionEntity.Create(150.00m, TransactionType.Debit);
        DbContext.Transactions.AddRange(transaction1, transaction2);
        await DbContext.SaveChangesAsync();

        var query = new GetTransactions.GetTransactionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result.Any(t => t.Id == transaction1.Id && t.Amount == transaction1.Amount), Is.True);
            Assert.That(result.Any(t => t.Id == transaction2.Id && t.Amount == transaction2.Amount), Is.True);
        });
    }

    [Test]
    public async Task Handle_WhenNoTransactionsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetTransactions.GetTransactionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}
