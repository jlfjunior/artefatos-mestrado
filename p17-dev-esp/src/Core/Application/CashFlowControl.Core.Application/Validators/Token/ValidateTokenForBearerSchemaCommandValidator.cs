using CashFlowControl.Core.Application.Commands;
using FluentValidation;

namespace CashFlowControl.Core.Application.Validators
{
    public class ValidateTokenForBearerSchemaCommandValidator : AbstractValidator<ValidateTokenCommand>
    {
        public ValidateTokenForBearerSchemaCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.Scheme).NotEmpty();
        }
    }
}
