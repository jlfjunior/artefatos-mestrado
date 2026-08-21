using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using FluentValidation;

namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.CreateTransaction;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.CashBoxId)
            .NotEmpty();
        RuleFor(x => x.OccurredAt)
            .NotEmpty();
        RuleFor(x => x.Type)
            .IsInEnum();
        RuleFor(x => x.Amount)
            .GreaterThan(0);
        RuleFor(x => x.Description)
            .MaximumLength(500);
        RuleFor(x => x.CategoryId)
            .GreaterThan(0);
        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0);
    }
}
