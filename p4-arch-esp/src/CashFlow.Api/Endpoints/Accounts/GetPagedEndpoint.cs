using CashFlow.Application.Account.Handlers;
using CashFlow.Application.Common;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts
{
    public static class GetPagedEndpoint
    {
        public static IEndpointRouteBuilder MapGetPagedAccountsEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", GetPagedAsync)
                .WithTags("Accounts")
                .WithName("GetPagedAccount")
                .WithSummary("Get a paged list of accounts.")
                .WithDescription("Retrieves a paged list of accounts for the authenticated user.")
                .Produces<PagedResult<GetAccountResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            return app;
        }

        private static async Task<IResult> GetPagedAsync(
            [AsParameters] GetPagedAccountsQuery query,
            ClaimsPrincipal claims,
            IGetPagedAccountsHandler handler,
            CancellationToken cancellationToken)
        {
            var userId = claims.GetUserIdAsValidatedGuid();

            var response = await handler.HandleAsync(userId: userId, query: query, cancellationToken);

            if (response.IsError)
                return response.Errors.ToProblem();

            return Results.Ok(response.Value);
        }
    }
}
