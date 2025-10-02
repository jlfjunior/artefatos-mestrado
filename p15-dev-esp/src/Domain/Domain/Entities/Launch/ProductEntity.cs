namespace Domain.Entities.Launch;

public class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<LaunchProductEntity> LaunchProducts { get; set; } = new List<LaunchProductEntity>();
}
