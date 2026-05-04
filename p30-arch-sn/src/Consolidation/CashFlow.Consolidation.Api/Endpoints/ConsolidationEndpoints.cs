using CashFlow.Consolidation.Api.Extensions;
using CashFlow.Consolidation.Api.Requests;
using CashFlow.Consolidation.Application.Queries;
using MediatR;

namespace CashFlow.Consolidation.Api.Endpoints;

public static class ConsolidationEndpoints
{
    public static IEndpointRouteBuilder MapConsolidationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "consolidation-api" }));
        app.MapGet("/api/consolidations/{date}", GetDailyAsync);
        app.MapGet("/api/consolidations", GetPeriodAsync);

        return app;
    }

    private static Task<IResult> GetDailyAsync(
        DateOnly date,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetDailyConsolidationQuery(date);
        return sender.SendQuery(query, cancellationToken);
    }

    private static Task<IResult> GetPeriodAsync(
        [AsParameters] GetConsolidationQueryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetPeriodConsolidationQuery(request.StartDate, request.EndDate);

        return sender.SendQuery(query, cancellationToken);
    }
}
