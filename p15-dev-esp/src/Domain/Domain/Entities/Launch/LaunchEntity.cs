using Domain.Models.Launch.Launch;

namespace Domain.Entities.Launch;

public class LaunchEntity
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreationDate { get; set; }
    public LaunchTypeEnum LaunchType { get; set; }
    public ConsolidationStatusEnum Status { get; set; }
    public DateTime ModificationDate { get; set; }
    public virtual List<LaunchProductEntity> LaunchProducts { get; set; } = new List<LaunchProductEntity>();
}
