using EasyCash.Command.Users.Login;
using EasyCash.Integration.Tests.Users;
using RestSharp;
using System.Net.Http.Json;

namespace EasyCash.Integration.Tests.Infrastructure;

public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;
    public RestClient DefaultRestClient { get; private set; }

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        DefaultRestClient = new RestClient(factory.CreateDefaultClient(), false);
    }

    protected async Task<string> GetAccessToken()
    {
        HttpResponseMessage loginResponse = await HttpClient.PostAsJsonAsync(
            "api/v1/users/login",
            new LogInUserCommand(
                UserData.RegisterTestUserRequest.Email,
                UserData.RegisterTestUserRequest.Password));

        AccessTokenCommandResult? accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenCommandResult>();

        return accessTokenResponse!.AccessToken;
    }
}