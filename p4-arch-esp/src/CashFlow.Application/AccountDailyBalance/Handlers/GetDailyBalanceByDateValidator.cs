using FluentValidation;

namespace CashFlow.Application.AccountDailyBalance.Handlers
{
    public class GetDailyBalanceByDateValidator : AbstractValidator<GetAccountDailyBalanceQuery>
    {
        public GetDailyBalanceByDateValidator()
        {
            RuleFor(x => x.Date)
                .GreaterThan(DateOnly.MinValue)
                    .WithMessage("Date must be a valid date.");

           RuleFor(x => x.Date)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("Date cannot be in the future.");
        }
    }
}
