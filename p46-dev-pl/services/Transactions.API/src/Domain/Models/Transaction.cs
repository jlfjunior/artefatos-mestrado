using Shared.Enums;

namespace Transactions.API.Domain.Models;

/// <summary>
/// Entidade de domínio representando uma transação (débito ou crédito).
/// </summary>
public class Transaction
{
    public Guid Id { get; private set; }
    public Lojista Lojista { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsProcessed { get; private set; }

    private Transaction() { }

    public static Transaction Create(Lojista lojista, decimal amount, TransactionType type, string? description, DateTime transactionDate)
    {
        ValidateLojista(lojista);
        ValidateAmount(amount);
        ValidateDate(transactionDate);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            Lojista = lojista,
            Amount = amount,
            Type = type,
            Description = description,
            TransactionDate = transactionDate,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };
    }

    private static void ValidateLojista(Lojista lojista)
    {
        if (!Enum.IsDefined(typeof(Lojista), lojista))
            throw new ArgumentException("Lojista inválido. Use um valor entre 0001 e 0004.", nameof(lojista));
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));
    }

    private static void ValidateDate(DateTime date)
    {
        if (date > DateTime.UtcNow.AddDays(1))
            throw new ArgumentException("Data não pode ser no futuro", nameof(date));
    }
}

public enum TransactionType
{
    Debit = 0,
    Credit = 1
}
