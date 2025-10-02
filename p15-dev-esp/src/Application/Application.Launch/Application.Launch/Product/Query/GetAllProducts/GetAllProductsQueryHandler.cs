using Domain.DTOs.Launch;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Product.Query.GetAllProducts;
public class GetAllProductsQueryHandler(IProductService _service) : IRequestHandler<GetAllProductsQuery, List<ProductDTO>>
{
    public async Task<List<ProductDTO>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken) => await _service.GetAll();
}
