using Flow.Core.DTOs;
using FluentValidation;
using Flow.Exceptions;

namespace Flow.Application.UseCases.Login
{
    public class DoLoginValidator : AbstractValidator<DoLoginRequest>
    {
        public DoLoginValidator()
        {
            RuleFor(_ => _.Login).NotEmpty().WithMessage(ResourceMessageError.login_empty_error);
            RuleFor(_ => _.Password).NotEmpty().WithMessage(ResourceMessageError.password_empty_error);
        }
    }
}
