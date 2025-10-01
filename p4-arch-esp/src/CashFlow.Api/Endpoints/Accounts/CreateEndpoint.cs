using CashFlow.Application.Account.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts
{
    public static class CreateEndpoint
    {
        public static RouteGroupBuilder MapCreateAccountEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateAsync)
                .WithTags("Accounts")
                .WithName("CreateAccount")
                .WithSummary("Create a new account.")
                .WithDescription("Creates a new account for the authenticated user.")
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status409Conflict);
            
            return group;
        }

        private static async Task<IResult> CreateAsync(
            [FromBody] CreateAccountRequest request,
            ICreateAccountHandler handler,
            ClaimsPrincipal claims,
            CancellationToken cancellationToken)
        {
            var userId = claims.GetUserIdAsValidatedGuid();

            var response = await handler.HandleAsync(userId, request, cancellationToken);
            if (response.IsError)
            {
                return response.Errors.ToProblem();
            }

            return Results.CreatedAtRoute("GetAccountById", new { id = response.Value.Id }, response.Value);
        }
    }
}
