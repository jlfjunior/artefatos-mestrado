using Cashflow.Management.Application.Requests.GetCashStatementReportConsolidated;
using Cashflow.Management.Application.Requests.GetCashStatementSummary;
using Cashflow.Management.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Management.Api.Endpoints
{
    public static class CashStatementEndpoints
    {
        public static void MapCashStatementEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/v1/cash-statement/summary", async (IMediator _mediator) =>
            {
                var request = new GetCashStatementSummaryRequest();

                var response = await _mediator.Send(request);

                if (response == null)
                    return Results.NoContent();

                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("GetCashStatementSummary")
            .WithSummary("Get current balance, inflows and outflows for the last 30 days.")
            .RequireAuthorization("Employee")
            .Produces<GetCashStatementSummaryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("v1/cash-statement/report/consolidated", async (IMediator _mediator, [FromQuery] string interval, [FromQuery] int page, [FromQuery] int pageSize) =>
            {
                var request = new GetCashStatementReportConsolidatedRequest()
                {
                    Interval = interval,
                    Page = page,
                    PageSize = pageSize
                };

                var response = await _mediator.Send(request);

                if (response == null)
                    return Results.NoContent();

                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("GetCashStatementReportConsolidated")
            .WithSummary("Get consolidated cash statement report for a specific interval.")
            .RequireAuthorization("Employee")
            .Produces<PagedResponse<GetCashStatementReportConsolidatedResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
