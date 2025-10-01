using CashFlow.Application.AccountDailyBalance.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashFlow.Api.Endpoints.Accounts.DailyBalance
{
    public static class GetDailyBalanceByDateEndpoint
    {
        public static IEndpointRouteBuilder MapGetDailyBalanceByDateEndpoint(this IEndpointRouteBuilder group)
        {
            group.MapGet("/{date}", GetAsync)
                .WithTags("Accounts")
                .WithName("GetDailyBalanceByDate")
                .WithSummary("Get a daily balance by date.")
                .WithDescription("Retrieves a daily balance by data for the authenticated user.")
                .Produces<GetAccountDailyBalanceResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            return group;
        }

        private static async Task<IResult> GetAsync(
            [FromRoute] Guid accountId,
            [FromRoute] DateOnly date,
            [FromServices] IGetDailyBalanceByDateHandler handler,
            [FromServices] GetDailyBalanceByDateValidator validator,
            ClaimsPrincipal claims,
            CancellationToken cancellationToken)
        {
            var userId = claims.GetUserIdAsValidatedGuid();
            var query = new GetAccountDailyBalanceQuery(userId, accountId, date);
            var validationResult = await validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)            
                return Results.ValidationProblem(validationResult.ToDictionary());            

            var response = await handler.HandleAsync(query, cancellationToken);

            if (response.IsError)
                return response.Errors.ToProblem();

            return Results.Ok(response.Value);
        }
    }
}
