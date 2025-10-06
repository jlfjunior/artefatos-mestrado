namespace BCash.Domain.DTOs
{
    public record PagedResponseOffsetDto<T>(int PageNumber, int PageSize, int TotalPages, int TotalRecords, List<T> Data);
}
