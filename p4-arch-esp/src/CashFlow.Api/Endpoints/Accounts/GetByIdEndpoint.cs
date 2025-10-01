using CashFlow.Application.Account.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts
{
    public static class GetByIdEndpoint
    {
        public static RouteGroupBuilder MapGetAccountByIdEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("/{id:guid}", GetByIdAsync)
                .WithTags("Accounts")
                .WithName("GetAccountById")
                .WithSummary("Get an store by ID.")
                .WithDescription("Retrieves an store by its ID for the authenticated user.")
                .Produces<GetAccountResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            return group;
        }
        private static async Task<IResult> GetByIdAsync(
            [FromRoute] Guid id,
            ClaimsPrincipal claims,
            IGetAccountByIdHandler handler,
            CancellationToken cancellationToken)
        {
            var userId = claims.GetUserIdAsValidatedGuid();

            var response = await handler.HandleAsync(userId: userId, id: id, cancellationToken);
            
            if (response.IsError)
                return response.Errors.ToProblem();

            return Results.Ok(response.Value);
        }
    }
}
