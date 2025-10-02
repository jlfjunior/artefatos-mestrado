namespace Domain.Entities.Launch;

public class LaunchProductEntity
{
    public int LaunchId { get; set; }
    public virtual LaunchEntity? Launch { get; set; }

    public int ProductId { get; set; }
    public virtual ProductEntity? Product { get; set; }

    public int ProductQuantity { get; set; }
    public decimal ProductPrice { get; set; }
}