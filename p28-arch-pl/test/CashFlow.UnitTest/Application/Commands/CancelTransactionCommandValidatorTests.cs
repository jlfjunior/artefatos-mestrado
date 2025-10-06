using CashFlow.Application.Commands;

namespace CashFlow.UnitTest.Application.Commands;

public class CancelTransactionCommandValidatorTests
{
    private readonly CancelTransactionCommandValidator _validator;

    public CancelTransactionCommandValidatorTests()
    {
        _validator = new CancelTransactionCommandValidator();
    }

    [Fact]
    public void Validate_ReturnSuccess()
    {
        var validate = _validator.Validate(new CancelTransactionCommand { TransactionId = Guid.NewGuid() });

        Assert.True(validate.IsValid);
    }

    [Fact]
    public void Validate_ReturnError()
    {
        var validate = _validator.Validate(new CancelTransactionCommand());

        Assert.False(validate.IsValid);
    }
}