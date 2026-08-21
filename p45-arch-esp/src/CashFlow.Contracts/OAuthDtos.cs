using System.Text.Json.Serialization;

namespace CashFlow.Contracts;

/// <summary>
/// Resposta de sucesso do endpoint de token OAuth2.
/// </summary>
public sealed record OAuthTokenSuccessResponse
{
    /// <summary>Token de acesso.</summary>
    public required string AccessToken { get; init; }

    /// <summary>Tipo de token.</summary>
    public required string TokenType { get; init; }

    /// <summary>Tempo de expiração em segundos.</summary>
    public required int ExpiresIn { get; init; }

    /// <summary>Escopos do token.</summary>
    public required string Scope { get; init; }
}

/// <summary>
/// Corpo JSON alternativo para <c>POST /oauth/token</c> (ex.: Scalar UI).
/// O formato canônico OAuth2 continua sendo <c>application/x-www-form-urlencoded</c>.
/// </summary>
public sealed record OAuthTokenRequest
{
    /// <summary>Grant type OAuth2.</summary> 
    public string GrantType { get; init; } = string.Empty;

    /// <summary>Client id público.</summary>
    public string? ClientId { get; init; }

    /// <summary>Secret do client.</summary>
    public string? ClientSecret { get; init; }

    /// <summary>E-mail do usuário.</summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>Senha do usuário.</summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Resultado da emissão de token OAuth2 (RFC 6749).
/// </summary>
/// <param name="AccessToken">Token de acesso JWT.</param>
/// <param name="ExpiresIn">Tempo de expiração em segundos.</param>
/// <param name="Scope">Escopos concedidos.</param> 
public sealed record OAuthTokenResponse(string AccessToken, int ExpiresIn, string Scope);

/// <summary>
/// Documento de descoberta OpenID Connect (<c>/.well-known/openid-configuration</c>).
/// </summary>
public sealed record OpenIdConfigurationResponse
{
    /// <summary>Emissor dos tokens JWT.</summary>
    public required string Issuer { get; init; }

    /// <summary>URL do endpoint de emissão de tokens.</summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>URL do conjunto de chaves públicas (JWKS).</summary>
    public required string JwksUri { get; init; }

    /// <summary>Grant types suportados pelo authorization server.</summary>
    public required IReadOnlyList<string> GrantTypesSupported { get; init; }

    /// <summary>Algoritmos de assinatura suportados para ID/access tokens.</summary>
    public required IReadOnlyList<string> IdTokenSigningAlgValuesSupported { get; init; }

    /// <summary>Escopos OAuth2 disponíveis na API.</summary>
    public required IReadOnlyList<string> ScopesSupported { get; init; }

    /// <summary>Claims presentes nos tokens emitidos.</summary>
    public required IReadOnlyList<string> ClaimsSupported { get; init; }
}

/// <summary>Conjunto de chaves públicas JWT (JWKS).</summary>
public sealed record JsonWebKeySetResponse
{
    /// <summary>Chaves públicas RSA disponíveis para validação de tokens.</summary>
    public required IReadOnlyList<JsonWebKeyResponse> Keys { get; init; }
}

/// <summary>
/// Representação de uma chave pública RSA no formato JWK.
/// </summary>
public sealed record JsonWebKeyResponse
{
    /// <summary>Tipo criptográfico da chave (ex.: <c>RSA</c>).</summary>
    [JsonPropertyName("kty")]
    public required string KeyType { get; init; }

    /// <summary>Identificador da chave (<c>kid</c>).</summary>
    [JsonPropertyName("kid")]
    public required string KeyId { get; init; }

    /// <summary>Uso previsto da chave (ex.: <c>sig</c>).</summary>
    [JsonPropertyName("use")]
    public required string Use { get; init; }

    /// <summary>Algoritmo de assinatura (ex.: <c>RS256</c>).</summary>
    [JsonPropertyName("alg")]
    public required string Algorithm { get; init; }

    /// <summary>Módulo RSA codificado em Base64URL.</summary>
    [JsonPropertyName("n")]
    public required string Modulus { get; init; }

    /// <summary>Expoente RSA codificado em Base64URL.</summary>
    [JsonPropertyName("e")]
    public required string Exponent { get; init; }
}
