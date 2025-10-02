using Domain.Models.Launch.Launch;

namespace Domain.DTOs.Launch;
public class ProductsOrderDTO : ProductsOrder
{
    public decimal Price { get; set; }
}
