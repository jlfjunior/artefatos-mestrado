using EasyCash.Abstractions.Messaging.Queries.PagedQueries;

namespace EasyCash.Report.Query.Consolidations;

public sealed class GetReportByDatesQuery : PagedQueryRequestBase<GetReportByDatesQueryResult>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

}

public sealed class GetReportByDatesQueryResult : IPagedQueryResponse
{
    public GetReportByDatesQueryItemResult[] Items { get; set; } = [];
    public decimal FinalAmount => Items.Sum(x => x.FinalBalance);

    public short Page { get; set; }
    public byte PageSize { get; set; }
    public int Total { get; set; }

    public static GetReportByDatesQueryResult Empty => new GetReportByDatesQueryResult
    {
        Items = [],
        Page = 1,
        PageSize = 15,
        Total = 0
    };

    public static GetReportByDatesQueryResult Ok(GetReportByDatesQueryItemResult[] items, GetReportByDatesQuery query, int total) =>
        new()
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            Total = total
        };
}

public sealed class GetReportByDatesQueryItemResult
{
    public DateOnly Date { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal FinalBalance { get; set; }
}