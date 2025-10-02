using Domain.DTOs.Launch;
using MediatR;

namespace Application.Launch.Product.Query.GetAllProducts;

public class GetAllProductsQuery : IRequest<List<ProductDTO>>
{
}