namespace Domain.Models
{
    public class PagnatedResult<T>
    {
        public PagnatedResult(List<T> data)
        {
            this.Data = data;
        }
        public List<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
