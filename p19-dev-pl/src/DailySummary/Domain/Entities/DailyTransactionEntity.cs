using Shared.Enums;

namespace Domain.Entities;

public class DailyTransactionEntity
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }

    protected DailyTransactionEntity() { }

    public static DailyTransactionEntity Create(Guid id, DateTime date, decimal amount, TransactionType type)
    {
        return new DailyTransactionEntity
        {
            Id = id,
            Date = date,
            Amount = amount,
            Type = type
        };
    }

    public void Update(decimal amount, TransactionType type, DateTime date)
    {
        Amount = amount;
        Type = type;
        Date = date;
    }
}