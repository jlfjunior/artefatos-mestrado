namespace CashFlow.Application.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
        public required int TotalItems { get; set; }
        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling(TotalItems / (double)PageSize);
            }
        }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
