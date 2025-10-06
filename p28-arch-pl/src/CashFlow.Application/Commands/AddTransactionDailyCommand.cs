using CashFlow.Domain.Entities;
using MediatR;

namespace CashFlow.Application.Commands;

public class AddTransactionDailyCommand : CashFlowDailyCommand, IRequest<CommandResponse<Guid>>
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
}