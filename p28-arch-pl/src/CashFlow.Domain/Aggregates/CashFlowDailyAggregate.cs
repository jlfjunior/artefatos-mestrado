using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Aggregates;

public class CashFlowDailyAggregate
{
    public CashFlowDailyAggregate(Guid id, Guid accountId, DateOnly date)
    {
        Id = id;
        AccountId = accountId;
        Date = date;
        Transactions = new List<Transaction?>();
    }

    public CashFlowDailyAggregate(Guid id, Guid accountId, DateOnly date, decimal initialBalance)
    {
        Id = id;
        AccountId = accountId;
        Date = date;
        InitialBalance = initialBalance;
        Transactions = new List<Transaction?>();
    }

    protected CashFlowDailyAggregate()
    {
    }

    public Guid Id { get; protected set; }

    public List<Transaction?> Transactions { get; protected set; }


    public decimal InitialBalance { get; protected set; } // D -1
    public decimal CurrentBalance => InitialBalance + GetBalance();
    public DateOnly Date { get; set; }
    public Guid AccountId { get; set; }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
    }

    public decimal GetBalance()
    {
        decimal balance = 0;
        foreach (var transaction in Transactions)
            if (transaction!.Type == TransactionType.Credit)
                balance += transaction.AmountVO.Amount;
            else
                balance -= transaction.AmountVO.Amount;
        return balance;
    }

    public IReadOnlyList<Transaction?> GetTransactionsInRange(DateTime startDate, DateTime endDate)
    {
        var filteredTransactions = Transactions
            .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
            .ToList();
        return filteredTransactions.AsReadOnly();
    }

    public IReadOnlyList<Transaction?> GetTransactionsByType(TransactionType transactionType)
    {
        var filteredTransactions = Transactions
            .Where(t => t.Type == transactionType)
            .ToList();
        return filteredTransactions.AsReadOnly();
    }

    public decimal GetTotalDebits()
    {
        var totalDebits = Transactions
            .Where(t => t.Type == TransactionType.Debit)
            .Sum(t => t.AmountVO.Amount);
        return totalDebits;
    }

    public decimal GetTotalCredits()
    {
        var totalCredits = Transactions
            .Where(t => t.Type == TransactionType.Credit)
            .Sum(t => t.AmountVO.Amount);
        return totalCredits;
    }

    public Transaction? ReverseTransaction(Guid transactionId)
    {
        var transactionOriginal = Transactions.FirstOrDefault(t => t.Id == transactionId);
        if (transactionOriginal != null)
        {
            var transactionReverse = transactionOriginal.Reverse();
            Transactions.Add(transactionReverse);
            return transactionReverse;
        }

        return null;
    }

    public bool HasNegativeBalance()
    {
        var balance = GetBalance();
        return balance < 0;
    }
}