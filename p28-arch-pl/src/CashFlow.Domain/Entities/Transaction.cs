using System.Globalization;
using CashFlow.Domain.ValueObjects;

namespace CashFlow.Domain.Entities;

public enum TransactionType
{
    Debit,
    Credit
}

public class Transaction
{
    public Transaction(Guid cashFlowId, decimal amount, TransactionType type)
    {
        CashFlowId = cashFlowId;
        Id = Guid.NewGuid();
        AmountVO = new Money(amount);
        Timestamp = DateTime.UtcNow;
        Type = type;
    }

    protected Transaction()
    {
    }

    public Guid Id { get; protected set; }
    public Guid CashFlowId { get; protected set; }
    public Money AmountVO { get; protected set; }
    public DateTime Timestamp { get; protected set; }
    public TransactionType Type { get; protected set; }

    public Transaction WithAmount(decimal amount)
    {
        return new Transaction(CashFlowId, amount, Type);
    }

    public Transaction WithTimestamp(DateTime timestamp)
    {
        return new Transaction(CashFlowId, AmountVO.Amount, Type)
        {
            Timestamp = timestamp
        };
    }

    public Transaction Reverse()
    {
        var reversedAmount = AmountVO.Multiply(-1);
        return new Transaction(CashFlowId, reversedAmount.Amount, Type);
    }


    public string GetFormattedAmount()
    {
        return AmountVO.Amount.ToString(CultureInfo.InvariantCulture);
    }

    public bool IsPositiveAmount()
    {
        return AmountVO.Amount > 0;
    }

    public bool IsNegativeAmount()
    {
        return AmountVO.Amount < 0;
    }

    public bool IsSameTransaction(Transaction other)
    {
        return Id == other.Id;
    }

    public bool IsSameAmount(Transaction other)
    {
        return AmountVO == other.AmountVO;
    }

    public bool IsInFuture()
    {
        return Timestamp > DateTime.UtcNow;
    }

    public Transaction? Merge(Transaction other)
    {
        var mergedAmount = AmountVO.Amount + other.AmountVO.Amount;
        return new Transaction(CashFlowId, mergedAmount, Type);
    }

    public bool HasSameTimestamp(Transaction other)
    {
        return Timestamp == other.Timestamp;
    }

    public Transaction ApplyPercentage(decimal percentage)
    {
        var newAmount = AmountVO.Amount * (percentage / 100);
        return new Transaction(CashFlowId, newAmount, Type);
    }
}