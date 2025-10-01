using CashFlow.Application.Common;
using CashFlow.Application.Common.Handlers;
using CashFlow.Application.Common.Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Application.Account.Handlers
{
    public class GetPagedAccountsQuery : PaginationFilter;

    public interface IGetPagedAccountsHandler : IHandler
    {
        Task<ErrorOr<PagedResult<GetAccountResponse>>> HandleAsync(
         Guid userId,
         GetPagedAccountsQuery query,
         CancellationToken cancellationToken);
    }

    public class GetPagedAccountsHandler : IGetPagedAccountsHandler
    {
        private readonly ICashFlowDbContext _cashFlowDbContext;

        public GetPagedAccountsHandler(ICashFlowDbContext cashFlowDbContext)
        {
            _cashFlowDbContext = cashFlowDbContext;
        }

        public async Task<ErrorOr<PagedResult<GetAccountResponse>>> HandleAsync(
            Guid userId, GetPagedAccountsQuery query,
            CancellationToken cancellationToken)
        {
            var queryble = _cashFlowDbContext.Accounts
                .Where(s => s.IdentityUserId == userId);

            var total = queryble.Count();

            queryble = queryble
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var results = await queryble.ToListAsync(cancellationToken);

            var mapper = new AccountMapper();
            var items = results
                .Select(t => mapper.ToGetAccountResponse(t))
                .ToList();

            return new PagedResult<GetAccountResponse>
            {
                Items = items,
                CurrentPage = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }
    }
}
