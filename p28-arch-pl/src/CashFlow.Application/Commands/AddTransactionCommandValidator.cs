using FluentValidation;

namespace CashFlow.Application.Commands;

public class AddTransactionCommandValidator : AbstractValidator<AddTransactionDailyCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.Amount).GreaterThan(0);
        RuleFor(command => command.Type).IsInEnum();
    }
}