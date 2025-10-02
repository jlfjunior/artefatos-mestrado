using Dapper;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Abstractions.Messaging.Queries.PagedQueries;
using EasyCash.Report.Query.Consolidations;
using EasyCash.Report.Query.Consolidations.Interfaces;
using System.Data;

namespace EasyCash.Report.Query.Store.Consolidations;

internal sealed class ReportConsolidationQueryStore : IReportConsolidationQueryStore
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ReportConsolidationQueryStore(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<GetReportByDatesQueryResult> GetReportByDatesAsync(GetReportByDatesQuery query, CancellationToken cancellationToken)
    {
        var param = new DynamicParameters();
        param.Add("@startDate", query.StartDate, direction: ParameterDirection.Input);
        param.Add("@endDate", query.EndDate, direction: ParameterDirection.Input);
        param.Add("@Skip", query.GetPageOffset(), direction: ParameterDirection.Input);
        param.Add("@Take", query.PageSize, direction: ParameterDirection.Input);

        var count = await _sqlConnectionFactory.CreateConnection()
            .QueryFirstOrDefaultAsync<int>(
                new CommandDefinition(ReportConsolidationQueryStoreConsts.SQL_COUNT_BY_DATES, param, cancellationToken: cancellationToken));

        if (count == 0)
            return GetReportByDatesQueryResult.Empty;

        var result = await _sqlConnectionFactory.CreateConnection()
            .QueryAsync<ReportConsolidationDto>(
                new CommandDefinition(ReportConsolidationQueryStoreConsts.SQL_GET_BY_DATES, param, cancellationToken: cancellationToken));

        var reportConsolidationDtos = result as ReportConsolidationDto[] ?? result.ToArray();

        return GetReportByDatesQueryResult.Ok(reportConsolidationDtos.Select(ReportConsolidationDto.MapToResult).ToArray(), query, count);

    }
}

internal static class ReportConsolidationQueryStoreConsts
{
    public const string SQL_COUNT_BY_DATES = @"
        SELECT COUNT(id)
        FROM daily_consolidations
        WHERE date::DATE BETWEEN @startDate AND @endDate";

    public const string SQL_GET_BY_DATES =
        $@"
        SELECT 
            date {nameof(ReportConsolidationDto.Date)},
            total_credit {nameof(ReportConsolidationDto.TotalCredit)}, 
            total_debit {nameof(ReportConsolidationDto.TotalDebit)}, 
            final_balance {nameof(ReportConsolidationDto.FinalBalance)}
        FROM daily_consolidations
        WHERE date::DATE BETWEEN @startDate AND @endDate
        ORDER BY Date ASC
        limit @Take offset @Skip";
}