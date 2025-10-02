using Domain.DTOs;
using Domain.DTOs.Launch;
using MediatR;

namespace Application.Launch.Product.Query.GetPaginatedProducts;

public class GetPaginatedProductsQuery : IRequest<PaginatedResult<ProductDTO>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
