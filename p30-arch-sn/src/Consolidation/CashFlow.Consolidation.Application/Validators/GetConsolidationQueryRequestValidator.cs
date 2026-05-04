using CashFlow.Consolidation.Application.Queries;
using FluentValidation;

namespace CashFlow.Consolidation.Application.Validators;

public sealed class GetConsolidationQueryRequestValidator : AbstractValidator<GetPeriodConsolidationQuery>
{
    public GetConsolidationQueryRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .Must(BeValidDate)
            .WithMessage("StartDate must be a valid date (yyyy-MM-dd).");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .Must(BeValidDate)
            .WithMessage("EndDate must be a valid date (yyyy-MM-dd).");
    }

    private static bool BeValidDate(string date)
    {
        return DateOnly.TryParse(date, out _);
    }
}
