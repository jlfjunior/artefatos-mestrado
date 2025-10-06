using CashFlow.Application.Queries;

namespace CashFlow.UnitTest.Application.Queries;

public class GetDailyBalanceQueryValidatorTests
{
    private readonly GetDailyBalanceQueryValidator _validator;

    public GetDailyBalanceQueryValidatorTests()
    {
        _validator = new GetDailyBalanceQueryValidator();
    }

    [Fact]
    public void Validate_ReturnSuccess()
    {
        var validate = _validator.Validate(new GetDailyBalanceQuery { AccountId = Guid.NewGuid() });

        Assert.True(validate.IsValid);
    }

    [Fact]
    public void Validate_ReturnError()
    {
        var validate = _validator.Validate(new GetDailyBalanceQuery());

        Assert.False(validate.IsValid);
    }
}