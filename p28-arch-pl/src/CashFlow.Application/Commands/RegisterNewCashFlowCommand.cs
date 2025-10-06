using MediatR;

namespace CashFlow.Application.Commands;

public class RegisterNewCashFlowCommand : IRequest<CommandResponse<Guid>>
{
    public Guid AccountId { get; set; }
}