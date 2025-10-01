using CashFlow.AppHost.EndToEndTests.Fixture;
using CashFlow.Application.Account.Handlers;
using CashFlow.Application.AccountBalance.Handlers;
using CashFlow.Application.Transactions.Handlers;
using CashFlow.Domain.Transactions;
using FluentAssertions;
using System.Net.Http.Json;

namespace CashFlow.AppHost.EndToEndTests.FunctionalTests
{
    [Collection("CashFlowApp")]
    public class CheckAccountBalanceByUserTest
    {
        private readonly CashFlowAppsFixture _fixture;
        public CheckAccountBalanceByUserTest(CashFlowAppsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory()]
        [InlineData(20, 50, -30)]
        [InlineData(100, 50, 50)]
        [InlineData(1000, 900, 100)]
        [InlineData(10, 100, -90)]
        public async Task CheckAccountBalance_ForNewUsers(decimal totalIncoming, decimal totalOutgoing, decimal expectedBalance)
        {
            // user
            var tokenForNewUser =
                await UserTests.GetTokenForNewUserAsync(_fixture.ClientIdentityServerApi());

            var clientWithToken = _fixture.ClientCashFlowApi();

            // account
            var accountResponse =
                await AccountTests.CreateAccountAsync(clientWithToken, tokenForNewUser!.AccessToken, "Conta Teste");
            var createdAccount = await accountResponse.Content.ReadFromJsonAsync<CreatedAccountResponse>();

            // initial balance
            var currentBalance = await GetAccountBalanceAsync(clientWithToken, createdAccount.Id);
            currentBalance!.Should().Be(0);

            // transaction incoming
            var responseIncomming = await clientWithToken.PostAsJsonAsync($"/me/accounts/{createdAccount.Id}/transactions",
                new CreateTransactionRequest(
                    ETransactionType.Incoming,
                    EPaymentMethod.Pix,
                    totalIncoming,
                    "Transação de teste de entrada")
                );
            responseIncomming.Should().Be201Created();

            // transaction outgoing
            var responseOutgoing = await clientWithToken.PostAsJsonAsync($"/me/accounts/{createdAccount.Id}/transactions",
                new CreateTransactionRequest(
                    ETransactionType.Outgoing,
                    EPaymentMethod.Cash,
                    totalOutgoing,
                    "Transação de teste de saida")
                );
            responseOutgoing.Should().Be201Created();
            
            currentBalance = await GetAccountBalanceAsync(clientWithToken, createdAccount.Id);            
            currentBalance!.Should().Be(expectedBalance);
        }

        private async Task<decimal> GetAccountBalanceAsync(HttpClient httpClient, Guid accountId)
        {
            await Task.Delay(TimeSpan.FromSeconds(60));

            var response =
                await httpClient.GetAsync($"/me/accounts/{accountId}/balances");
            response.Should().Be200Ok();            

            var result = await response.Content.ReadFromJsonAsync<GetAccountBalanceResponse>();

            return result!.CurrentTotal;
        }
    }
}
