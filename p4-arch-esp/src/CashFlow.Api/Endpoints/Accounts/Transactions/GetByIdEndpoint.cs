using CashFlow.Application.Transactions.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts.Transactions;

public static class GetByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetTransactionByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", GetByIdAsync)
            .WithTags("Transactions")
            .WithName("GetTransactionById")
            .WithSummary("Get a transaction by ID.")
            .WithDescription("Retrieves a transaction by its ID for the authenticated user.")
            .Produces<GetTransactionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> GetByIdAsync(
        [FromRoute] Guid accountId,
        [FromRoute] Guid id,
        [FromServices] IGetTransactionByIdHandler handler,
        ClaimsPrincipal claims,
        [FromServices] CancellationToken cancellationToken)
    {
        var userId = claims.GetUserIdAsValidatedGuid();

        var response = await handler.HandleAsync(userId, id, cancellationToken);

        if (response.IsError)
            return response.Errors.ToProblem();

        return Results.Ok(response.Value);
    }
}
