using CashFlow.Application.Common;
using CashFlow.Application.Transactions.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts.Transactions
{
    public static class GetPagedEndpoint
    {
        public static IEndpointRouteBuilder MapGetPagedTransactionsEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", GetPagedAsync)
                .WithTags("Transactions")
                .WithName("GetPagedTransaction")
                .WithSummary("Get a paged list of transactions.")
                .WithDescription("Retrieves a paged list of transactions for the authenticated user.")
                .Produces<PagedResult<GetTransactionResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            return app;
        }

        private static async Task<IResult> GetPagedAsync(
            [FromRoute] Guid accountId,
            [AsParameters] GetPagedTransactionsQuery query,
            [FromServices] IGetPagedTransactionHandler handler,
            ClaimsPrincipal claims,
            CancellationToken cancellationToken)
        {
            var userId = claims.GetUserIdAsValidatedGuid();

            var response = await handler.HandleAsync(userId: userId, accountId: accountId, query: query, cancellationToken);

            if (response.IsError)
                return response.Errors.ToProblem();

            return Results.Ok(response.Value);
        }
    }
}
