using System.Collections;
using CashFlow.Application.Commands;

namespace CashFlow.UnitTest.Application.Commands;

public class AddTransactionCommandValidatorTests
{
    private readonly AddTransactionCommandValidator _validator;

    public AddTransactionCommandValidatorTests()
    {
        _validator = new AddTransactionCommandValidator();
    }

    [Theory]
    [ClassData(typeof(AddTransactionCommandTestData))]
    public void Validate_ReturnSuccess(AddTransactionDailyCommand dailyCommand)
    {
        var validate = _validator.Validate(dailyCommand);

        Assert.True(validate.IsValid);
    }

    [Theory]
    [ClassData(typeof(AddTransactionCommandFailTestData))]
    public void Validate_ReturnError(AddTransactionDailyCommand dailyCommand)
    {
        var validate = _validator.Validate(dailyCommand);

        Assert.False(validate.IsValid);
    }
}

internal class AddTransactionCommandTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new AddTransactionDailyCommand
            {
                AccountId = Guid.NewGuid(),
                Type = TransactionType.Credit,
                Amount = 10M
            }
        };

        yield return new object[]
        {
            new AddTransactionDailyCommand
            {
                AccountId = Guid.NewGuid(),
                Type = TransactionType.Credit,
                Amount = 10M
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class AddTransactionCommandFailTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new AddTransactionDailyCommand
            {
                AccountId = Guid.NewGuid(),
                Type = TransactionType.Debit,
                Amount = -10M
            }
        };

        yield return new object[] { new AddTransactionDailyCommand() };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}