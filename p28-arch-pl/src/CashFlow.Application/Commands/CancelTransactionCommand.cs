using MediatR;

namespace CashFlow.Application.Commands;

public class CancelTransactionCommand : IRequest<CommandResponse<Guid>>
{
    public Guid TransactionId { get; set; }
}