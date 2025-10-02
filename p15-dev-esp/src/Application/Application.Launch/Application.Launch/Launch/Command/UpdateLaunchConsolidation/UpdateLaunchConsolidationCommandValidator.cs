using FluentValidation;

namespace Application.Launch.Launch.Command.UpdateLaunchConsolidation;
public class UpdateLaunchConsolidationCommandValidator : AbstractValidator<UpdateLaunchConsolidationCommand>
{
    public UpdateLaunchConsolidationCommandValidator()
    {
        RuleFor(command => command.Launches)
            .NotNull().WithMessage("The Launches list cannot be null.")
            .NotEmpty().WithMessage("At least one Launch ID must be provided.");
    }
}