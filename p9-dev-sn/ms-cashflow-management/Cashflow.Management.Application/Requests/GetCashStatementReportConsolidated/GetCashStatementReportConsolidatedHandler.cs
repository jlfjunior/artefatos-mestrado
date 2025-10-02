using Cashflow.Management.Application.Shared;
using Cashflow.Management.Data.AppContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Application.Requests.GetCashStatementReportConsolidated
{
    public class GetCashStatementReportConsolidatedHandler(AppDbContext appDbContext) : IRequestHandler<GetCashStatementReportConsolidatedRequest, PagedResponse<GetCashStatementReportConsolidatedResponse>>
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<PagedResponse<GetCashStatementReportConsolidatedResponse>> Handle(GetCashStatementReportConsolidatedRequest request, CancellationToken cancellationToken)
        {
            if (request.Interval == "D")
            {
                var query = _appDbContext.CashStatements
                .Select(g => new
                {
                    g.OpeningDate.Date,
                    g.Inflow,
                    g.Outflow,
                    g.Balance
                })
                .OrderByDescending(r => r.Date)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

                var cashStatements = await query.ToListAsync(cancellationToken);

                long totalItems = 0;
                long totalPages = 0;

                if (cashStatements.Any())
                {
                    totalItems = await _appDbContext.CashStatements.CountAsync(cancellationToken);
                    totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
                }

                var result = new PagedResponse<GetCashStatementReportConsolidatedResponse>()
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = cashStatements.Select(c => new GetCashStatementReportConsolidatedResponse
                    {
                        Date = c.Date,
                        TotalInflows = c.Inflow,
                        TotalOutflows = c.Outflow,
                        TotalBalance = c.Balance
                    }).ToList()
                };

                return result;
            }
            else
            {
                var query = _appDbContext.CashStatements
                .GroupBy(c => new DateTime(c.OpeningDate.Year, c.OpeningDate.Month, 1))
                .Select(g => new
                {
                    Period = g.Key,
                    Inflow = g.Sum(x => x.Inflow),
                    Outflow = g.Sum(x => x.Outflow),
                    Balance = g.Sum(x => x.Balance)
                })
                .OrderBy(r => r.Period)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

                var cashStatements = await query.ToListAsync(cancellationToken);

                long totalItems = 0;
                long totalPages = 0;

                if (cashStatements.Any())
                {
                    totalItems = await _appDbContext.CashStatements.GroupBy(c => new DateTime(c.OpeningDate.Year, c.OpeningDate.Month, 1)).CountAsync(cancellationToken);
                    totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
                }

                var result = new PagedResponse<GetCashStatementReportConsolidatedResponse>()
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = cashStatements.Select(c => new GetCashStatementReportConsolidatedResponse
                    {
                        Date = c.Period,
                        TotalInflows = c.Inflow,
                        TotalOutflows = c.Outflow,
                        TotalBalance = c.Balance
                    }).ToList()
                };

                return result;
            }
        }
    }
}
