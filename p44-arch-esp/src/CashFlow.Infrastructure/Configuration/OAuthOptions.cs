namespace CashFlow.Infrastructure.Configuration;

/// <summary>
/// Configurações do servidor OAuth2 e JWT.
/// </summary>
public sealed class OAuthOptions
{
    /// <summary>Nome da seção de configuração no arquivo de configuração.</summary>
    public const string SectionName = "OAuth";

    /// <summary>URL do emissor do token.</summary>
    public string Issuer { get; set; } = "http://localhost:8081";

    /// <summary>Chave secreta para assinar as chaves RSA.</summary>
    public string SigningKeysSecret { get; set; } = string.Empty;

    /// <summary>Tamanho do pool de chaves RSA.</summary>
    public int SigningKeysPoolSize { get; set; } = 2;

    /// <summary>Tempo de vida do token de acesso.</summary>
    public int AccessTokenTtlSeconds { get; set; } = 28800;
}
