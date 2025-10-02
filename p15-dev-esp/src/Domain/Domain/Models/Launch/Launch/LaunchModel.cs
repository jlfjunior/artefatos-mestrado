namespace Domain.Models.Launch.Launch;

public class LaunchModel
{
    public LaunchTypeEnum LaunchType { get; set; }
    public List<ProductsOrder> ProductsOrder { get; set; } = new List<ProductsOrder>();
}