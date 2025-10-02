using Domain.Entities;
using Shared.Enums;

namespace DailySummary.Tests.Domain;

[TestFixture]
public class DailyTransactionTests
{
    [Test]
    public void CreateDailyTransaction_ValidData_ShouldCreateTransaction()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        DateTime date = new(2024, 2, 20);
        decimal amount = 250.00m;
        TransactionType type = TransactionType.Credit;

        // Act
        var transaction = DailyTransactionEntity.Create(id, date, amount, type);

        // Assert
        Assert.That(transaction, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Id, Is.EqualTo(id));
            Assert.That(transaction.Date, Is.EqualTo(date));
            Assert.That(transaction.Amount, Is.EqualTo(amount));
            Assert.That(transaction.Type, Is.EqualTo(type));
        });
    }

    [Test]
    public void UpdateDailyTransaction_ValidData_ShouldUpdateTransaction()
    {
        // Arrange
        var transaction = DailyTransactionEntity.Create(Guid.NewGuid(), new DateTime(2024, 2, 20), 250.00m, TransactionType.Credit);
        decimal newAmount = 400.00m;
        TransactionType newType = TransactionType.Debit;

        // Act
        transaction.Update(newAmount, newType, DateTime.UtcNow);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Amount, Is.EqualTo(newAmount));
            Assert.That(transaction.Type, Is.EqualTo(newType));
        });
    }
}