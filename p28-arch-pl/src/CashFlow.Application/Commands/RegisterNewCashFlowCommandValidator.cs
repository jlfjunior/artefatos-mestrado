using FluentValidation;

namespace CashFlow.Application.Commands;

public class RegisterNewCashFlowCommandValidator : AbstractValidator<RegisterNewCashFlowCommand>
{
    public RegisterNewCashFlowCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
    }
}