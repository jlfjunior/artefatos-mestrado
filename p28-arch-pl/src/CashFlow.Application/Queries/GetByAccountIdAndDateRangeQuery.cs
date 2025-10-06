using CashFlow.Application.Commands;
using MediatR;

namespace CashFlow.Application.Queries;

public class GetByAccountIdAndDateRangeQuery : CashFlowDailyCommand,
    IRequest<CommandResponse<GetByAccountIdAndDateRangeQueryResponse>>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}