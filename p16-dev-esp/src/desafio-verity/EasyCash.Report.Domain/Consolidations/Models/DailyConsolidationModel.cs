namespace EasyCash.Report.Domain.Consolidations.Models;

public sealed class DailyConsolidationModel
{
    public DateOnly Date { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }

    public static DailyConsolidationModel Empty(DateOnly date) =>
        new()
        {
            Date = date,
            TotalCredit = 0,
            TotalDebit = 0
        };
}

