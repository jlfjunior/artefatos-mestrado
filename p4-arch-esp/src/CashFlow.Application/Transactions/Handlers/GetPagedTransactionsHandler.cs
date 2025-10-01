using CashFlow.Application.Common;
using CashFlow.Application.Common.Handlers;
using CashFlow.Application.Common.Interfaces;
using CashFlow.Domain.Transactions;
using ErrorOr;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Application.Transactions.Handlers;

public class GetPagedTransactionsQuery : PaginationFilter
{
    public ETransactionType? Type { get; init; }
    public EPaymentMethod? PaymentMethod { get; init; }
}

public interface IGetPagedTransactionHandler : IHandler
{
    Task<ErrorOr<PagedResult<GetTransactionResponse>>> HandleAsync(
         Guid userId,
         Guid accountId,
         GetPagedTransactionsQuery query,
         CancellationToken cancellationToken);
}

public class GetPagedTransactionsHandler : IGetPagedTransactionHandler
{
    private readonly ICashFlowDbContext _cashFlowDbContext;

    public GetPagedTransactionsHandler(ICashFlowDbContext cashFlowDbContext)
    {
        _cashFlowDbContext = cashFlowDbContext;
    }

    public async Task<ErrorOr<PagedResult<GetTransactionResponse>>> HandleAsync(
        Guid userId,
        Guid accountId,
        GetPagedTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        var queryble = _cashFlowDbContext.Accounts
            .Where(s => s.Id == accountId && s.IdentityUserId == userId)
            .Include(s => s.Transactions)
            .SelectMany(s => s.Transactions);

        if (query.Type.HasValue)
            queryble = queryble.Where(t => t.Type == query.Type.Value);

        if (query.PaymentMethod.HasValue)
            queryble = queryble.Where(t => t.PaymentMethod == query.PaymentMethod.Value);

        var total = queryble.Count();

        queryble = queryble
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);            

        var results = await queryble.ToListAsync(cancellationToken);

        var mapper = new TransactionMapper();
        var items = results
            .Select(t => mapper.ToResponse(t))
            .ToList();

        return new PagedResult<GetTransactionResponse>
        {
            Items = items,
            CurrentPage = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };
    }
}
