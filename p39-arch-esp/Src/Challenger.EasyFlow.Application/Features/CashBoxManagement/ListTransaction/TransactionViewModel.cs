namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.ListTransaction;

public sealed record TransactionViewModel(Guid Id, Guid CashBoxId, DateTimeOffset OccurredAt, int Type, decimal Amount, string Description, int CategoryId, int PaymentMethodId);
