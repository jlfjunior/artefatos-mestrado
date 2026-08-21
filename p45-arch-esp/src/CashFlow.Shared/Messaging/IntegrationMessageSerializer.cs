using System.Text.Json;
using System.Text.Json.Serialization;

namespace CashFlow.Shared.Messaging;

/// <summary>
/// Serializa/deserializa mensagens CloudEvents (tipo definido pela exchange).
/// </summary>
public static class IntegrationMessageSerializer
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializa o CloudEvent para JSON.
    /// </summary>
    /// <typeparam name="TMessage">Tipo da mensagem a ser serializada.</typeparam>
    /// <param name="message">Mensagem a ser serializada.</param>
    /// <returns>JSON serializado.</returns>
    public static byte[] Serialize<TMessage>(TMessage message)
        where TMessage : IIntegrationMessage =>
        JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), JsonOptions);

    /// <summary>
    /// Desserializa JSON para o CloudEvent do consumer.
    /// </summary>
    /// <typeparam name="TMessage">Tipo da mensagem a ser deserializada.</typeparam>
    /// <param name="json">JSON a ser deserializado.</param>
    /// <returns>Mensagem deserializada.</returns>
    public static TMessage Deserialize<TMessage>(ReadOnlySpan<byte> json)
        where TMessage : IIntegrationMessage =>
        JsonSerializer.Deserialize<TMessage>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Invalid {typeof(TMessage).Name} payload.");

    /// <summary>
    /// Resolve a exchange a partir do atributo <c>type</c> do CloudEvent.
    /// </summary>
    /// <param name="message">Mensagem a partir da qual a exchange será resolvida.</param>
    /// <returns>Exchange resolvida.</returns>
    public static string ResolveExchange(IIntegrationMessage message) => message.Type;
}
