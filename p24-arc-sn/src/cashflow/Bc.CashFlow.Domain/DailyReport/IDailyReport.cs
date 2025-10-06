namespace Bc.CashFlow.Domain.DailyReport;

public interface IDailyReport
{
	int Id { get; }
	int? AccountId { get; }
	DateTime Date { get; }
	decimal TotalDebits { get; }
	decimal TotalCredits { get; }
	decimal TotalFee { get; }
	decimal Balance { get; }
	DateTime CreatedAt { get; }
}