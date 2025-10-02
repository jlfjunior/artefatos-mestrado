using Cashflow.Management.Application.Shared;
using MediatR;

namespace Cashflow.Management.Application.Requests.GetCashStatementReportConsolidated
{
    public class GetCashStatementReportConsolidatedRequest : IRequest<PagedResponse<GetCashStatementReportConsolidatedResponse>>
    {
        public string Interval { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
