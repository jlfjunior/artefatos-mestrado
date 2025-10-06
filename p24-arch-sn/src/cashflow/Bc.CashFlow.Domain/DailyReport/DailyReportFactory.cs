namespace Bc.CashFlow.Domain.DailyReport;

public class DailyReportFactory
{
	// ReSharper disable once MemberCanBeMadeStatic.Global
	public IDailyReport Create(
		int id,
		int? accountId,
		DateTime date,
		decimal totalDebits,
		decimal totalCredits,
		decimal totalFee,
		decimal balance,
		DateTime createdAt)
	{
		return new DailyReportVo
		{
			Id = id,
			AccountId = accountId,
			Date = date,
			TotalDebits = totalDebits,
			TotalCredits = totalCredits,
			TotalFee = totalFee,
			Balance = balance,
			CreatedAt = createdAt
		};
	}
}

file record DailyReportVo : IDailyReport
{
	public int Id { get; init; }
	public int? AccountId { get; init; }
	public DateTime Date { get; init; }
	public decimal TotalDebits { get; init; }
	public decimal TotalCredits { get; init; }
	public decimal TotalFee { get; init; }
	public decimal Balance { get; init; }
	public DateTime CreatedAt { get; init; }
}