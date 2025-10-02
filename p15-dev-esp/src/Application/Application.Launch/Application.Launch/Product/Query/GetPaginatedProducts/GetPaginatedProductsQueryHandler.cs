using Domain.DTOs;
using Domain.DTOs.Launch;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Product.Query.GetPaginatedProducts;
public class GetPaginatedProductsQueryHandler(IProductService _service) : IRequestHandler<GetPaginatedProductsQuery, PaginatedResult<ProductDTO>>
{
    public async Task<PaginatedResult<ProductDTO>> Handle(GetPaginatedProductsQuery query, CancellationToken cancellationToken) => await _service.GetPaginated(query.PageNumber, query.PageSize);
}
