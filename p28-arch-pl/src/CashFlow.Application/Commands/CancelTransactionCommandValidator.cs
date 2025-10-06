using FluentValidation;

namespace CashFlow.Application.Commands;

public class CancelTransactionCommandValidator : AbstractValidator<CancelTransactionCommand>
{
    public CancelTransactionCommandValidator()
    {
        RuleFor(command => command.TransactionId).NotEmpty();
    }
}