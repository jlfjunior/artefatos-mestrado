using Domain.DTOs.Launch;
using MediatR;

namespace Application.Launch.Product.Query.GetProductById;
public class GetProductsByIdQuery : IRequest<ProductDTO>
{
    public int Id { get; set; }
}