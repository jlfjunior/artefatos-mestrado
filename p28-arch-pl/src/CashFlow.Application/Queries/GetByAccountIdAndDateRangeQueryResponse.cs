namespace CashFlow.Application.Queries;

public class GetByAccountIdAndDateRangeQueryResponse
{
    public List<GetDailyBalanceQueryResponse> Content { get; set; }
    public long TotalItems { get; set; }
    public int TotalPages { get; set; }
}