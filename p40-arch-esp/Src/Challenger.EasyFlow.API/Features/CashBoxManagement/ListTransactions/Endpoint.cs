using Challenger.EasyFlow.API.Common;
using Challenger.EasyFlow.Application.Features.CashBoxManagement.ListTransaction;
using Microsoft.AspNetCore.Mvc;

namespace Challenger.EasyFlow.API.Features.CashBoxManagement.ListTransactions;

public sealed class Endpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("{cashBoxId}/transactions", Handle)
            .WithName("ListTransactions")
            .WithSummary("Lists transactions for a specific cash box.")
            .WithDescription("Retrieves a paginated list of transactions associated with the specified cash box ID.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async ValueTask<IResult> Handle(
        [FromRoute] Guid cashBoxId,
        [AsParameters] QueryOptions options,
        [FromServices] ListTransactionQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListTransactionQuery(cashBoxId, options);
        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
