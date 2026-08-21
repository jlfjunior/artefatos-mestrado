using Shared.Enums;

namespace Consolidation.API.Domain.Models;

/// <summary>
/// Entidade representando a consolidação diária por lojista (saldo do dia de cada loja).
/// </summary>
public class Consolidation
{
    public Guid Id { get; private set; }
    public Lojista Lojista { get; private set; }
    public DateTime ConsolidationDate { get; private set; }
    public decimal DebitTotal { get; private set; }
    public decimal CreditTotal { get; private set; }
    public decimal DailyBalance { get; private set; }
    public int ProcessedCount { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private Consolidation() { }

    public static Consolidation Create(Lojista lojista, DateTime consolidationDate)
    {
        return new Consolidation
        {
            Id = Guid.NewGuid(),
            Lojista = lojista,
            ConsolidationDate = consolidationDate.Date,
            DebitTotal = 0,
            CreditTotal = 0,
            DailyBalance = 0,
            ProcessedCount = 0,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public void AddTransaction(decimal amount, string type)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");

        if (type.Equals("Debit", StringComparison.OrdinalIgnoreCase))
        {
            DebitTotal += amount;
        }
        else if (type.Equals("Credit", StringComparison.OrdinalIgnoreCase))
        {
            CreditTotal += amount;
        }

        DailyBalance = CreditTotal - DebitTotal;
        ProcessedCount++;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFromTransaction(decimal amount, string type)
    {
        AddTransaction(amount, type);
    }
}
