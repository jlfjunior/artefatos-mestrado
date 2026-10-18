using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using CashFlow.Contracts.Json;
using CashFlow.Web.Configuration;
using Microsoft.Extensions.Options;

namespace CashFlow.Web.Services;

/// <summary>
/// Cliente para obtenção de tokens OAuth2.
/// </summary>
/// <param name="httpClient">Cliente HTTP.</param>
/// <param name="options">Configurações OAuth2 da interface web.</param>
public sealed class OAuthTokenClient(HttpClient httpClient, IOptions<OAuthSettings> options)
{
    private static readonly JsonSerializerOptions JsonOptions = ContractsJsonSerializerOptions.Default;

    /// <summary>
    /// Realiza o login e obtém o token de acesso.
    /// </summary>
    /// <param name="username">Nome de usuário.</param>
    /// <param name="password">Senha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Token de acesso, papel e ID do usuário.</returns>
    public async Task<(string AccessToken, string Role, Guid UserId)> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, settings.TokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{settings.ClientId}:{settings.ClientSecret}")));

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username,
            ["password"] = password
        });

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Falha ao obter token OAuth ({(int)response.StatusCode}): {body}",
                null,
                response.StatusCode);
        }

        var payload = await response.Content.ReadFromJsonAsync<TokenPayload>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Resposta de token vazia.");

        if (string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            throw new InvalidOperationException("Resposta de token sem access_token.");
        }

        var accessToken = payload.AccessToken;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        var role = jwt.Claims.First(c => c.Type == "role").Value;
        var userId = Guid.Parse(jwt.Claims.First(c => c.Type == "sub").Value);

        return (accessToken, role, userId);
    }

    private sealed record TokenPayload(string? AccessToken);
}
