using Domain.Models;
using MediatR;

namespace Application.Launch.Product.Command.DeleteProduct;
public class DeleteProductCommand : IRequest<ApiResponse>
{
    public int Id { get; set; }
}