using Domain;
using Domain.Entities;
using Shared.Enums;

namespace Transactions.Tests.Domain;

[TestFixture]
public class TransactionTests
{
    [Test]
    public void CreateTransaction_ValidData_ShouldCreateTransaction()
    {
        // Arrange
        decimal amount = 100.00m;
        var type = TransactionType.Credit;

        // Act
        var transaction = TransactionEntity.Create(amount, type);

        // Assert
        Assert.That(transaction, Is.Not.Null);
        Assert.That(transaction.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Amount, Is.EqualTo(amount));
            Assert.That(transaction.Type, Is.EqualTo(type));
            Assert.That(transaction.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public void CreateTransaction_InvalidAmount_ShouldThrowException()
    {
        // Arrange
        decimal amount = -50.00m;
        var type = TransactionType.Debit;

        // Act & Assert
        var ex = Assert.Throws<TransactionDomainException>(() => TransactionEntity.Create(amount, type));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.EqualTo("O valor deve ser maior que zero."));
    }

    [Test]
    public void UpdateTransaction_ValidData_ShouldUpdateTransaction()
    {
        // Arrange
        var transaction = TransactionEntity.Create(50.00m, TransactionType.Debit);
        decimal newAmount = 200.00m;
        var newType = TransactionType.Credit;

        // Act
        transaction.Update(newAmount, newType);

        // Assert
        Assert.That(transaction.UpdatedAt, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Amount, Is.EqualTo(newAmount));
            Assert.That(transaction.Type, Is.EqualTo(newType));
        });
    }

    [Test]
    public void UpdateTransaction_InvalidAmount_ShouldThrowException()
    {
        // Arrange
        var transaction = TransactionEntity.Create(100.00m, TransactionType.Credit);
        decimal invalidAmount = 0.00m;

        // Act & Assert
        var ex = Assert.Throws<TransactionDomainException>(() => transaction.Update(invalidAmount, TransactionType.Debit));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.EqualTo("O valor deve ser maior que zero."));
    }
}