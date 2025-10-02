using Domain.DTOs.Launch;

namespace Domain.Models.Consolidation;
public class DailyConsolidationEvent
{
    public List<LaunchDTO> Launches { get; set; } = new List<LaunchDTO>();
}
