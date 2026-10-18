namespace CashFlow.Web.Configuration;

/// <summary>
/// URL base da API HTTP consumida pelo frontend Blazor.
/// </summary>
public sealed class ApiSettings
{
    /// <summary>Nome da seção em appsettings.</summary>
    public const string SectionName = "ApiSettings";

    /// <summary>URL base da CashFlow API.</summary>
    public string BaseUrl { get; set; } = "http://localhost:8081";
}
