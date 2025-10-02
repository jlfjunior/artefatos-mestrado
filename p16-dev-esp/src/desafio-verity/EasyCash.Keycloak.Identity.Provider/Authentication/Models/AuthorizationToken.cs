using System.Text.Json.Serialization;

namespace EasyCash.Keycloak.Identity.Provider.Authentication.Models;

internal sealed class AuthorizationToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;
}
