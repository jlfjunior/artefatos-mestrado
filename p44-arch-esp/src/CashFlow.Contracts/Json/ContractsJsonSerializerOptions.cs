using System.Text.Json;

namespace CashFlow.Contracts.Json;

/// <summary>
/// Convenção JSON dos DTOs HTTP da API (requests e responses em snake_case).
/// </summary>
public static class ContractsJsonSerializerOptions
{
    /// <summary>
    /// Opções compartilhadas para serialização dos contratos HTTP.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = Create();

    /// <summary>
    /// Cria uma nova instância com a convenção de contratos.
    /// </summary>
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions();
        Apply(options);
        return options;
    }

    /// <summary>
    /// Aplica snake_case e desserialização case-insensitive.
    /// </summary>
    public static void Apply(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.PropertyNameCaseInsensitive = true;
    }
}
