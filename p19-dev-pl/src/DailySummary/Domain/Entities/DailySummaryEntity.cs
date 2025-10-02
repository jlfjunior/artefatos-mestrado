namespace Domain.Entities;

public class DailySummaryEntity
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal Balance => TotalCredits - TotalDebits;

    private DailySummaryEntity() { }

    public static DailySummaryEntity Create(DateTime date, decimal totalCredits, decimal totalDebits)
    {
        return new DailySummaryEntity
        {
            Id = Guid.NewGuid(),
            Date = date,
            TotalCredits = totalCredits,
            TotalDebits = totalDebits
        };
    }

    public void Update(decimal totalCredits, decimal totalDebits)
    {
        TotalCredits = Math.Max(0, totalCredits);
        TotalDebits = Math.Max(0, totalDebits);
    }
}