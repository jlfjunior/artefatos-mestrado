using CashFlow.Launches.Application.Commands;
using FluentValidation;

namespace CashFlow.Launches.Application.Validators;

public sealed class RegisterLaunchCommandValidator : AbstractValidator<RegisterLaunchCommand>
{
    public RegisterLaunchCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Type must be either 'credit' or 'debit'.")
            .Must(BeAValidType)
            .WithMessage("Type must be either 'credit' or 'debit'.");

        RuleFor(x => x.OccurredOnUtc)
            .Must(d => d != default)
            .WithMessage("OccurredOnUtc is required.");
    }

    private static bool BeAValidType(string type)
    {
        return type.Equals("credit", StringComparison.OrdinalIgnoreCase)
               || type.Equals("debit", StringComparison.OrdinalIgnoreCase);
    }
}