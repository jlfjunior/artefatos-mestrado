using FluentValidation;

namespace Application.Authentication.Authentication.Command.Login;
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("The username is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("The password is required.");
    }
}