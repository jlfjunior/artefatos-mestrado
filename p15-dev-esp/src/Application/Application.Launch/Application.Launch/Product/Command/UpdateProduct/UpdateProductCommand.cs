using Domain.Models;
using MediatR;

namespace Application.Launch.Product.Command.UpdateProduct;
public class UpdateProductCommand : IRequest<ApiResponse>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}