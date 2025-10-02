namespace Domain.DTOs
{
    public class PaginatedResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PaginatedResult(List<T> items, int totalCount, int pageSize, int pageNumber)
        {
            Items = items;
            TotalCount = totalCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
    }
}
