using System.Text.Json.Serialization;

namespace CashFlow.Shared.Messaging;

/// <summary>
/// Envelope CloudEvents 1.0 com payload tipado em <c>data</c>.
/// </summary>
/// <typeparam name="TData">Conteúdo de negócio do evento.</typeparam>
public record CloudEvent<TData> : IIntegrationMessage
{
    /// <summary>Versão da especificação CloudEvents.</summary>
    [JsonPropertyName("specversion")]
    public string SpecVersion { get; init; } = CloudEventAttributes.SpecVersion;

    /// <summary>Identificador único do evento.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Origem do evento.</summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>Tipo do evento.</summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>Momento em que o evento ocorreu.</summary>
    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; init; }

    /// <summary>Tipo do conteúdo do payload.</summary>
    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; init; } = CloudEventAttributes.DataContentType;

    /// <summary>Conteúdo de negócio do evento.</summary>
    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}
