using CashFlow.Application.Queries;

namespace CashFlow.UnitTest.Application.Queries;

public class GetByAccountIdAndDateRangeQueryValidatorTests
{
    private readonly GetByAccountIdAndDateRangeQueryValidator _validator;

    public GetByAccountIdAndDateRangeQueryValidatorTests()
    {
        _validator = new GetByAccountIdAndDateRangeQueryValidator();
    }

    [Fact]
    public void Validate_ReturnSuccess()
    {
        var validate = _validator.Validate(new GetByAccountIdAndDateRangeQuery
        {
            AccountId = Guid.NewGuid(),
            PageSize = 50,
            PageNumber = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        });

        Assert.True(validate.IsValid);
    }

    [Fact]
    public void Validate_ReturnError1()
    {
        var validate = _validator.Validate(new GetByAccountIdAndDateRangeQuery());

        Assert.False(validate.IsValid);
    }

    [Fact]
    public void Validate_ReturnError2()
    {
        var validate = _validator.Validate(new GetByAccountIdAndDateRangeQuery
        {
            AccountId = Guid.NewGuid(),
            PageSize = -50,
            PageNumber = 0,
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        });

        Assert.False(validate.IsValid);
    }
}