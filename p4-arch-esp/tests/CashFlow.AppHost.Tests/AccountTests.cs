using CashFlow.AppHost.EndToEndTests.Fixture;
using CashFlow.Application.Account.Handlers;
using FluentAssertions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CashFlow.AppHost.EndToEndTests;

[Collection("CashFlowApp")]
public class AccountTests
{
    private readonly CashFlowAppsFixture _fixture;


    public AccountTests(CashFlowAppsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAccount_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _fixture.ClientCashFlowApi().GetAsync("/me/accounts");
        // Assert
        response.Should().Be401Unauthorized();
    }

    [Fact]
    public async Task CreateAccount_Authenticated_ReturnsCreated()
    {
        // act
        var tokenForNewUser =
            await UserTests.GetTokenForNewUserAsync(_fixture.ClientIdentityServerApi());
        
        // assert
        tokenForNewUser!.AccessToken.Should().NotBeNullOrEmpty();

        // act
        var accountResponse =
            await CreateAccountAsync(_fixture.ClientCashFlowApi(), tokenForNewUser!.AccessToken, "Conta Teste");
        
        accountResponse.Should().Be201Created();
    }

    public static async Task<HttpResponseMessage?> CreateAccountAsync(HttpClient httpClient, string bearerToken, string name)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);

        var request = new CreateAccountRequest(name);
        return  await httpClient.PostAsJsonAsync("/me/accounts", request);        
    }
}
