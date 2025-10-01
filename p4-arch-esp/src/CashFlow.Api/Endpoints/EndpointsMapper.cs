using CashFlow.Api.Endpoints.Accounts;
using CashFlow.Api.Endpoints.Accounts.Balance;
using CashFlow.Api.Endpoints.Accounts.DailyBalance;
using CashFlow.Api.Endpoints.Accounts.Transactions;

namespace CashFlow.Api.Endpoints;
public static class EndpointGroupMapper
{
    public static void MapAllEndpoints(this WebApplication app)
    {
        var meRoutes =
            app.MapGroup("/me")
                .RequireAuthorization()
                .AddEndpointFilter<RequireUserIdFilter>()
                .ProducesValidationProblem();

        var accounts =
            meRoutes.MapGroup("/accounts")
                .MapCreateAccountEndpoint()
                .MapGetAccountByIdEndpoint()
                .MapGetPagedAccountsEndpoint();

            accounts.MapGroup("/{accountId:guid}/balances")
                .MapGetBalanceByAccountIdEndpoint();
        
            accounts.MapGroup("/{accountId:guid}/transactions")
                .MapCreateTransactionEndpoint()
                .MapGetTransactionByIdEndpoint()
                .MapGetPagedTransactionsEndpoint();

            accounts.MapGroup("/{accountId:guid}/daily-balances/")
                .MapGetDailyBalanceByDateEndpoint()
            ;
    }
}
