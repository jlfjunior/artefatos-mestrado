using EasyCash.Report.Query.Consolidations;

namespace EasyCash.Report.Query.Store.Consolidations;

internal sealed class ReportConsolidationDto
{
    public DateOnly Date { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal FinalBalance { get; set; }

    public static GetReportByDatesQueryItemResult MapToResult(ReportConsolidationDto dto) =>
        new()
        {
            Date = dto.Date,
            TotalCredit = dto.TotalCredit,
            TotalDebit = dto.TotalDebit,
            FinalBalance = dto.FinalBalance
        };
}