using Cashflow.Management.Data.AppContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Application.Requests.GetCashStatementSummary
{
    public class GetCashStatementSummaryHandler(AppDbContext appDbContext) : IRequestHandler<GetCashStatementSummaryRequest, GetCashStatementSummaryResponse>
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<GetCashStatementSummaryResponse> Handle(GetCashStatementSummaryRequest request, CancellationToken cancellationToken)
        {
            var transactions = await _appDbContext.CashStatements.Where(c => c.OpeningDate >= DateTime.Now.AddDays(-30) && c.OpeningDate <= DateTime.Now).ToListAsync(cancellationToken);

            var totalInflows = transactions.Sum(t => t.Inflow);
            var totalOutflows = transactions.Sum(t => t.Outflow);
            var totalBalance = totalInflows - totalOutflows;

            var response = new GetCashStatementSummaryResponse()
            {
                CurrentBalance = totalBalance,
                TotalInflows = totalInflows,
                TotalOutflows = totalOutflows,
            };

            return response;
        }
    }
}
