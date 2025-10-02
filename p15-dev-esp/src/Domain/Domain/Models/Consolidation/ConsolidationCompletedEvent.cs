using Domain.DTOs.Launch;

namespace Domain.Models.Consolidation;
public class ConsolidationCompletedEvent
{
    public List<LaunchDTO> Launches { get; set; } = new List<LaunchDTO>();
}
