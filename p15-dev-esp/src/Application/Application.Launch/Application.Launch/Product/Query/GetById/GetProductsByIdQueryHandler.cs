using Domain.DTOs.Launch;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Product.Query.GetProductById;
public class GetProductByIdQueryHandler (IProductService _service): IRequestHandler<GetProductsByIdQuery, ProductDTO>
{

    public async Task<ProductDTO> Handle(GetProductsByIdQuery request, CancellationToken cancellationToken) => await _service.GetById(request.Id);
}
