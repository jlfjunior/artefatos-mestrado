using EasyCash.Abstractions.Messaging.Queries;

namespace EasyCash.Abstractions.Messaging.Queries.PagedQueries;

public interface IPagedQuery
{
    short Page { get; set; }

    byte PageSize { get; set; }
}

public interface IPagedQueryResponse
{
    short Page { get; set; }


    byte PageSize { get; set; }

    int Total { get; set; }

    public ushort TotalPages => (ushort)Math.Ceiling(Total / (double)PageSize);
}

public interface IPagedQueryRequest<TQueryResponse> : IQueryRequest<TQueryResponse>, IPagedQuery
    where TQueryResponse : class, IPagedQueryResponse, new()
{
    public short GetPageOffset() =>
        (short)((Page - 1) * PageSize);
}

public abstract class PagedQueryRequestBase<TQueryResponse> : IPagedQueryRequest<TQueryResponse>
    where TQueryResponse : class, IPagedQueryResponse, new()
{
    public const int MAX_ITENS_PER_PAGE = 100;
    public const int DEFAULT_ITENS_PER_PAGE = 15;


    private short _page = 1;
    public short Page
    {
        get => (short)(_page == 0 ? 1 : _page);
        set => _page = value;
    }

    private uint _pageSize = DEFAULT_ITENS_PER_PAGE;
    public byte PageSize
    {
        get => (byte)(_pageSize == 0 ? DEFAULT_ITENS_PER_PAGE : MaxItensPorPagina(_pageSize));
        set => _pageSize = value;
    }

    public static uint MaxItensPorPagina(uint pageSize) =>
        pageSize > MAX_ITENS_PER_PAGE ? MAX_ITENS_PER_PAGE : pageSize;
}

public static class PagedQueryRequestExtensions
{
    public static short GetPageOffset<TQueryResponse>(this IPagedQueryRequest<TQueryResponse> queryRequest)
        where TQueryResponse : class, IPagedQueryResponse, new()
        => queryRequest.GetPageOffset();
}
