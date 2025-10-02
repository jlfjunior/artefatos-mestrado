using EasyCash.Abstractions.Exceptions;
using EasyCash.Domain.Transactions.Entities;
using EasyCash.Shared.Enums;
using EasyCash.Shared.ValueObjects;
using FluentAssertions;

namespace EasyCash.Unit.Tests.Domain.Transactions;

public sealed class TransactionEntityTests
{
    [Fact]
    public void Create_ShouldReturnTransactionEntity_WhenValidParameters()
    {
        // Arrange
        var description = "Test Transaction";
        var type = ETransactionType.Debit;
        var amount = 100m;
        var category = "Test Category";
        var date = DateTimeOffset.Now;

        // Act
        var transaction = TransactionEntity.Create(description, type, amount, category, date);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Description.Should().Be(description);
        transaction.Type.Should().Be(type);
        transaction.Amount.Should().Be(Money.FromDecimal(amount));
        transaction.Category.Should().Be(category);
        transaction.Date.Should().BeCloseTo(date, TimeSpan.FromSeconds(1));
        transaction.CreatedAt.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldThrowValidationException_WhenInvalidParameters()
    {
        // Arrange
        var description = "";
        var type = ETransactionType.Debit;
        var amount = -100m;
        var category = "";

        // Act
        Action act = () => TransactionEntity.Create(description, type, amount, category);

        // Assert
        act.Should().Throw<ValidationException>()
            .And.Errors.Should().Contain(e => e.PropertyName == "Description")
            .And.Contain(e => e.PropertyName == "Amount")
            .And.Contain(e => e.PropertyName == "Category");
    }

    [Fact]
    public void Update_ShouldUpdateTransactionEntity_WhenValidParameters()
    {
        // Arrange
        var transaction = TransactionEntity.Create("Test Transaction", ETransactionType.Debit, 100m, "Test Category");
        var newDescription = "Updated Transaction";
        var newType = ETransactionType.Credit;
        var newAmount = 200m;
        var newCategory = "Updated Category";
        var newDate = DateTime.Now;

        // Act
        transaction.Update(newDescription, newType, newAmount, newCategory, newDate);

        // Assert
        transaction.Description.Should().Be(newDescription);
        transaction.Type.Should().Be(newType);
        transaction.Amount.Should().Be(Money.FromDecimal(newAmount));
        transaction.Category.Should().Be(newCategory);
        transaction.Date.Should().Be(newDate.Date);
        transaction.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_ShouldThrowValidationException_WhenInvalidParameters()
    {
        // Arrange
        var transaction = TransactionEntity.Create("Test Transaction", ETransactionType.Debit, 100m, "Test Category");
        var newDescription = "";
        var newType = ETransactionType.Credit;
        var newAmount = -200m;
        var newCategory = "";
        var newDate = DateTime.Now;

        // Act
        Action act = () => transaction.Update(newDescription, newType, newAmount, newCategory, newDate);

        // Assert
        act.Should().Throw<ValidationException>()
            .And.Errors.Should().Contain(e => e.PropertyName == "Description")
            .And.Contain(e => e.PropertyName == "Amount")
            .And.Contain(e => e.PropertyName == "Category");
    }
}