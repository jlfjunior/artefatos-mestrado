using System.ComponentModel;

namespace Domain.Models.Launch.Launch;
public enum ConsolidationStatusEnum
{
    [Description("Launched")]
    Launched,
    [Description("Processing")]
    Processing,
    [Description("Consolidated")]
    Consolidated,
    [Description("Error")]
    Error
}
