using CashFlow.Application.Commands;

namespace CashFlow.UnitTest.Application.Commands;

public class RegisterNewCashFlowCommandValidatorTests
{
    private readonly RegisterNewCashFlowCommandValidator _validator;


    public RegisterNewCashFlowCommandValidatorTests()
    {
        _validator = new RegisterNewCashFlowCommandValidator();
    }

    [Fact]
    public void Validate_ReturnSuccess()
    {
        var validate = _validator.Validate(new RegisterNewCashFlowCommand
        {
            AccountId = Guid.NewGuid()
        });

        Assert.True(validate.IsValid);
    }

    [Fact]
    public void Validate_ReturnError()
    {
        var validate = _validator.Validate(new RegisterNewCashFlowCommand());

        Assert.False(validate.IsValid);
    }
}