using CashFlow.Domain.Entities;

namespace CashFlow.Application.Queries;

public class GetDailyBalanceQueryResponse
{
    public decimal CurrentBalance { get; set; }
    public List<Transaction> Transactions { get; set; }

    public DateOnly Date { get; set; }
    public Guid AccountId { get; set; }
}