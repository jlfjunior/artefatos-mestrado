using CashFlow.Application.AccountBalance.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts.Balance
{
    public static class GetBalanceByAccountIdEndpoint
    {
        public static IEndpointRouteBuilder MapGetBalanceByAccountIdEndpoint(this IEndpointRouteBuilder group)
        {
            group.MapGet("/", GetBalanceByAccountIdAsync)
                .WithTags("Balance")
                .WithName("GetBalanceByAccountId")
                .WithSummary("Get a balance by account Id.")
                .WithDescription("Retrieves an account balance by its ID for the authenticated user.")
                .Produces<GetAccountBalanceResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            return group;
        }

        private static async Task<IResult> GetBalanceByAccountIdAsync(
            [FromRoute] Guid accountId,
            ClaimsPrincipal claims,
            [FromServices]  IGetBalanceByAccountIdHandler handler,
            CancellationToken cancellationToken)
        {
            var userId = claims.GetUserIdAsValidatedGuid();

            var response = await handler.HandleAsync(userId: userId, accountId: accountId, cancellationToken);

            if (response.IsError)
                return response.Errors.ToProblem();

            return Results.Ok(response.Value);
        }
    }
}
