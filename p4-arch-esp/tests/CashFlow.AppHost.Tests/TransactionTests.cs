using CashFlow.AppHost.EndToEndTests.Fixture;
using CashFlow.Application.Account.Handlers;
using CashFlow.Application.Transactions.Handlers;
using CashFlow.Domain.Transactions;
using FluentAssertions;
using System.Net.Http.Json;

namespace CashFlow.AppHost.EndToEndTests;

[Collection("CashFlowApp")]
public class TransactionTests
{
    private readonly CashFlowAppsFixture _fixture;

    public TransactionTests(CashFlowAppsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateTransactions_ForNewAccount_ReturnsCreated()
    {
        // act
        var tokenForNewUser =
            await UserTests.GetTokenForNewUserAsync(_fixture.ClientIdentityServerApi());

        // Act
        var response = await CreateTransactionForNewAccountAsync(_fixture.ClientCashFlowApi(),
            tokenForNewUser!.AccessToken,
            new CreateTransactionRequest(
                ETransactionType.Incoming,
                EPaymentMethod.Pix,
                100,
                "Transação de teste")
            );
        var result = await response.Content.ReadFromJsonAsync<CreateTransactionResponse>();

        // Assert
        result.Id.Should().NotBeEmpty();
    }

    public static async Task<HttpResponseMessage?> CreateTransactionForNewAccountAsync(HttpClient httpClient,
        string bearerToken,
        CreateTransactionRequest createTransaction)
    {
        var accountResult = await AccountTests
            .CreateAccountAsync(httpClient, bearerToken, "Conta para transações");

        var accountResponse =
            await accountResult.Content.ReadFromJsonAsync<CreatedAccountResponse>();

        return await httpClient.PostAsJsonAsync($"/me/accounts/{accountResponse.Id}/transactions", createTransaction);
    }
}
