namespace Cashflow.Transactions.Application.Shared
{
    public class PagedResponse<T>
    {
        public long TotalItems { get; set; }
        public long TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
