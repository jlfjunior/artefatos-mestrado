using FluentValidation;

namespace Application.Consolidation.Consolidation.Command.CreateConsolidation;
public class CreateConsolidationCommandValidator : AbstractValidator<CreateConsolidationCommand>
{
    public CreateConsolidationCommandValidator()
    {
        RuleFor(command => command.Launches)
                .NotNull().WithMessage("The Launches list cannot be null.")
                .NotEmpty().WithMessage("At least one launch must be provided.");
    }
}
