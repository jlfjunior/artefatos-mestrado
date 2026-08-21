namespace CashFlow.Shared.Messaging;

/// <summary>
/// Contrato base de mensagens de integração no formato CloudEvents.
/// </summary>
public interface IIntegrationMessage
{
    /// <summary>Identificador único do evento (<c>id</c>).</summary>
    string Id { get; }

    /// <summary>Momento em que o evento ocorreu (<c>time</c>).</summary>
    DateTimeOffset? Time { get; }

    /// <summary>Tipo do evento (<c>type</c>), alinhado à exchange.</summary>
    string Type { get; }

    /// <summary>Origem do evento (<c>source</c>).</summary>
    string Source { get; }
}
