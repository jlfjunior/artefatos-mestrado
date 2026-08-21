namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Registro de client OAuth2 confidencial.
/// </summary>
/// <param name="Id">Identificador interno do client.</param>
/// <param name="ClientId">Identificador público do client OAuth.</param>
/// <param name="ClientType">Tipo do client (ex.: confidential).</param>
/// <param name="GrantTypes">Grant types permitidos, separados por vírgula.</param>
/// <param name="SecretHash">Hash do secret do client.</param>
public sealed record OAuthClientRecord(
    Guid Id,
    string ClientId,
    string ClientType,
    string GrantTypes,
    string SecretHash);
