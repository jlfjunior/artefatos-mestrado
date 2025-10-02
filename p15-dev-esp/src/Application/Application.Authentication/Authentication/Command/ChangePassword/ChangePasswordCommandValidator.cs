using FluentValidation;

namespace Application.Authentication.Authentication.Command.ChangePassword;
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("The username is required.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("The current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("The new password is required.")
            .MinimumLength(8).WithMessage("The password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("The password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("The password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("The password must contain at least one number.")
            .Matches(@"[\W_]").WithMessage("The password must contain at least one special character (!@#$%^&* etc.).")
            .NotEqual(x => x.CurrentPassword).WithMessage("The new password cannot be the same as the current password.");
    }
}