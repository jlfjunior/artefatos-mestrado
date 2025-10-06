using FluentValidation;

namespace CashFlow.Application.Queries;

public class GetByAccountIdAndDateRangeQueryValidator : AbstractValidator<GetByAccountIdAndDateRangeQuery>
{
    public GetByAccountIdAndDateRangeQueryValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();

        RuleFor(command => command.StartDate)
            .GreaterThan(new DateOnly(2023, 6, 1));

        RuleFor(command => command.EndDate)
            .Must((dateRange, endDate) => endDate >= dateRange.StartDate)
            .WithMessage("EndDate cannot be earlier than StartDate.");

        RuleFor(command => command.PageSize)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(command => command.PageNumber)
            .NotEmpty()
            .GreaterThan(0);
    }
}