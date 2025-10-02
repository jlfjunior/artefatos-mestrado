using EasyCash.Abstractions.Messaging.Queries.PagedQueries;
using EasyCash.Shared.Enums;

namespace EasyCash.Query.Transactions.Get;

public sealed class GetDailyTransactionsQuery : PagedQueryRequestBase<GetDailyTransactionsQueryResult>
{
    public DateTime Date { get; set; }
}

public sealed class GetDailyTransactionsQueryResult : IPagedQueryResponse
{
    public GetDailyTransactionsQueryResultItem[] Items { get; set; }
    public short Page { get; set; }
    public byte PageSize { get; set; }
    public int Total { get; set; }

    public static GetDailyTransactionsQueryResult Empty => new GetDailyTransactionsQueryResult
    {
        Items = [],
        Page = 1,
        PageSize = 15,
        Total = 0
    };

    public static GetDailyTransactionsQueryResult Create(GetDailyTransactionsQueryResultItem[] items, GetDailyTransactionsQuery query, int total) =>
        new()
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            Total = total
        };
}

public sealed class GetDailyTransactionsQueryResultItem
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public ETransactionType Type { get; set; }
    public string TypeDescription => Type.ToString();
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTimeOffset Date { get; set; }
}