using FluentValidation;

namespace Application.Launch.Launch.Command.CreateLaunch;
public class CreateLaunchCommandValidator : AbstractValidator<CreateLaunchCommand>
{
    public CreateLaunchCommandValidator()
    {
        RuleFor(x => x.LaunchType).IsInEnum().WithMessage("Invalid Launch Type.");
        RuleFor(x => x.ProductsOrder).NotEmpty().WithMessage("Products are required.");
    }
}