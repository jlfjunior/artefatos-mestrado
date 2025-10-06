using FluentValidation;

namespace CashFlow.Application.Queries;

public class GetDailyBalanceQueryValidator : AbstractValidator<GetDailyBalanceQuery>
{
    public GetDailyBalanceQueryValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
    }
}