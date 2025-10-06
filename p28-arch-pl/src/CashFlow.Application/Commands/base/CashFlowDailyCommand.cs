namespace CashFlow.Application.Commands;

public abstract class CashFlowDailyCommand
{
    public Guid AccountId { get; set; }
}