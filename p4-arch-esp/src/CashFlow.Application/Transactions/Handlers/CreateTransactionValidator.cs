using FluentValidation;

namespace CashFlow.Application.Transactions.Handlers
{
    public class CreateTransactionValidator : AbstractValidator<CreateTransactionRequest>
    {
        public CreateTransactionValidator()
        {
            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Type must be a valid transaction type.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithErrorCode("PaymentMethod must be a valid payment method.");
        }
    }
}
