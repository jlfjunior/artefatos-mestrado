using CashFlow.Shared.Enums;

namespace CashFlow.Shared.Messaging;

/// <summary>
/// Payload de negócio do evento de lançamento criado.
/// </summary>
/// <param name="TransactionId">Identificador único do lançamento.</param>
/// <param name="UserId">Identificador único do usuário cliente.</param>
/// <param name="CreatedBy">Identificador único do usuário que criou o lançamento.</param>
/// <param name="Description">Descrição do lançamento.</param>
/// <param name="Amount">Valor do lançamento.</param>
/// <param name="EntryType">Tipo do lançamento.</param>
/// <param name="TransactionDate">Data do lançamento.</param>
public sealed record TransactionCreatedData(
    Guid TransactionId,
    Guid UserId,
    Guid CreatedBy,
    string Description,
    decimal Amount,
    EntryType EntryType,
    DateOnly TransactionDate);

/// <summary>
/// CloudEvent publicado após criação de lançamento de caixa.
/// </summary>
public sealed record TransactionCreatedMessage : CloudEvent<TransactionCreatedData>
{
    /// <summary>
    /// Cria o evento a partir do agregado persistido.
    /// </summary>
    /// <param name="transactionId">Identificador único do lançamento.</param>
    /// <param name="userId">Identificador único do usuário cliente.</param>
    /// <param name="createdBy">Identificador único do usuário que criou o lançamento.</param>
    /// <param name="description">Descrição do lançamento.</param>
    /// <param name="amount">Valor do lançamento.</param>
    /// <param name="entryType">Tipo do lançamento.</param>
    /// <param name="transactionDate">Data do lançamento.</param>
    /// <returns>Evento de lançamento criado.</returns>
    public static TransactionCreatedMessage Create(
        Guid transactionId,
        Guid userId,
        Guid createdBy,
        string description,
        decimal amount,
        EntryType entryType,
        DateOnly transactionDate) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Source = CloudEventSources.TransactionsApi,
            Type = EventExchanges.TransactionCreated,
            Time = DateTimeOffset.UtcNow,
            Data = new TransactionCreatedData(
                transactionId,
                userId,
                createdBy,
                description,
                amount,
                entryType,
                transactionDate)
        };
}
