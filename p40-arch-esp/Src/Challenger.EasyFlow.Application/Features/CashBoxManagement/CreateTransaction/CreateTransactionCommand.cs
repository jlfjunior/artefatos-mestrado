using Challenger.EasyFlow.Application.Common.CQRS;
using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using FluentResults;

namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid CashBoxId,
    DateTimeOffset OccurredAt,
    TransactionType Type,
    decimal Amount,
    string Description,
    int CategoryId,
    int PaymentMethodId
) : Command<Result>;
