using CashFlow.Application.Commands;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetDailyBalanceQuery : CashFlowDailyCommand, IRequest<CommandResponse<GetDailyBalanceQueryResponse>>
{
}