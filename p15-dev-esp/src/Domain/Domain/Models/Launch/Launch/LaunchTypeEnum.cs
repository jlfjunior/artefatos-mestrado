using System.ComponentModel;

namespace Domain.Models.Launch.Launch;
public enum LaunchTypeEnum
{
    [Description("Debit")]
    Debit,
    [Description("Credit")]
    Credit
}