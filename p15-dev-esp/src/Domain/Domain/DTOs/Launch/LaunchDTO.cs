using Domain.Models.Launch.Launch;

namespace Domain.DTOs.Launch;
public class LaunchDTO
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreationDate { get; set; }
    public LaunchTypeEnum LaunchType { get; set; }
    public ConsolidationStatusEnum Status { get; set; }
    public DateTime ModificationDate { get; set; }
    public List<ProductsOrderDTO>? ProductsOrder { get; set; } = new List<ProductsOrderDTO>();
}