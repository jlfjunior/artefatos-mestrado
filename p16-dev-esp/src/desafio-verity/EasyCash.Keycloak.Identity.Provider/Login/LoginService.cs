using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Exceptions;
using EasyCash.Keycloak.Identity.Provider.Authentication;
using EasyCash.Keycloak.Identity.Provider.Authentication.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace EasyCash.Keycloak.Identity.Provider.Login;

internal sealed class LoginService : ILoginService
{
    private static readonly Error AuthenticationFailed = new(
        "Keycloak.AuthenticationFailed",
        "Failed to acquire access token do to authentication failure");

    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _keycloakOptions;

    public LoginService(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
    {
        _httpClient = httpClient;
        _keycloakOptions = keycloakOptions.Value;
    }

    public async Task<string> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("scope", "openid email"),
                new("grant_type", "password"),
                new("username", email),
                new("password", password)
            };

            using var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

            HttpResponseMessage response = await _httpClient.PostAsync(
                "",
                authorizationRequestContent,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            AuthorizationToken? authorizationToken = await response
                .Content
                .ReadFromJsonAsync<AuthorizationToken>(cancellationToken);

            if (authorizationToken is null)
            {
                throw new BusinessException(AuthenticationFailed);
            }

            return authorizationToken.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            throw new BusinessException(AuthenticationFailed);
        }
    }
}
