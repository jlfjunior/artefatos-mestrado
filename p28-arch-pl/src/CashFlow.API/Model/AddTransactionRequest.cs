using CashFlow.Domain.Entities;

namespace CashFlow.API.Model;

public class AddTransactionRequest
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
}