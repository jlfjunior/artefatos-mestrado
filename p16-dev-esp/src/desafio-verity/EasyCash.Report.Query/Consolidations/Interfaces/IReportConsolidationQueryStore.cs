namespace EasyCash.Report.Query.Consolidations.Interfaces;

public interface IReportConsolidationQueryStore
{
    Task<GetReportByDatesQueryResult> GetReportByDatesAsync(GetReportByDatesQuery query, CancellationToken cancellationToken);
}