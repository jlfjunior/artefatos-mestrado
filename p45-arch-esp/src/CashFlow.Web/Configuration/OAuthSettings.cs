namespace CashFlow.Web.Configuration;

/// <summary>
/// Credenciais OAuth2 para autenticação da interface web.
/// </summary>
public sealed class OAuthSettings
{
    /// <summary>Nome da seção em appsettings.</summary>
    public const string SectionName = "OAuth";

    /// <summary>URL do endpoint de emissão de token.</summary>
    public string TokenUrl { get; set; } = "http://localhost:8081/oauth/token";

    /// <summary>Client id público do OAuth.</summary>
    public string ClientId { get; set; } = "cashflow.web";

    /// <summary>Secret do client OAuth.</summary>
    public string ClientSecret { get; set; } = "cashflow-web-secret";
}
