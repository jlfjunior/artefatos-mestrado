using System.Text.Json.Serialization;

namespace CashFlow.Contracts;

/// <summary>
/// Resposta de erro padronizada da API.
/// </summary>
/// <param name="Error">Código estável do erro.</param>
/// <param name="Description">Descrição legível do erro.</param>
public sealed record ErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string Description);
