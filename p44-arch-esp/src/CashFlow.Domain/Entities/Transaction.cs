using CashFlow.Shared.Enums;

namespace CashFlow.Domain.Entities;

/// <summary>
/// Agregado imutável que representa um lançamento de fluxo de caixa (débito ou crédito).
/// </summary>
public sealed class Transaction
{
    /// <summary>Identificador único do lançamento.</summary>
    public Guid Id { get; private set; }

    /// <summary>Descrição do lançamento.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Valor absoluto do lançamento.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Tipo do lançamento (crédito ou débito).</summary>
    public EntryType EntryType { get; private set; }

    /// <summary>Data contábil do lançamento.</summary>
    public DateOnly TransactionDate { get; private set; }

    /// <summary>Identificador do usuário cliente titular.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Instante UTC de criação do registro.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Identificador do usuário que criou o lançamento.</summary>
    public Guid CreatedBy { get; private set; }

    private Transaction()
    {
    }

    /// <summary>
    /// Cria um novo lançamento validando invariantes de domínio.
    /// </summary>
    /// <param name="description">Descrição do lançamento.</param>
    /// <param name="amount">Valor absoluto do lançamento.</param>
    /// <param name="entryType">Tipo do lançamento.</param>
    /// <param name="transactionDate">Data contábil do lançamento.</param>
    /// <param name="userId">Identificador do usuário cliente titular.</param>
    /// <param name="createdBy">Identificador do usuário que cria o lançamento.</param>
    /// <returns>Instância persistível do agregado.</returns>
    public static Transaction Create(
        string description,
        decimal amount,
        EntryType entryType,
        DateOnly transactionDate,
        Guid userId,
        Guid createdBy)
    {
        ValidateDescription(description);
        ValidateAmount(amount);

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (createdBy == Guid.Empty)
        {
            throw new ArgumentException("Created by is required.", nameof(createdBy));
        }

        return new Transaction
        {
            Id = Guid.NewGuid(),
            Description = description.Trim(),
            Amount = amount,
            EntryType = entryType,
            TransactionDate = transactionDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Reconstrói o agregado a partir da persistência.
    /// </summary>
    /// <param name="id">Identificador único do lançamento.</param>
    /// <param name="description">Descrição do lançamento.</param>
    /// <param name="amount">Valor absoluto do lançamento.</param>
    /// <param name="entryType">Tipo do lançamento.</param>
    /// <param name="transactionDate">Data contábil do lançamento.</param>
    /// <param name="userId">Identificador do usuário cliente titular.</param>
    /// <param name="createdAt">Instante UTC de criação do registro.</param>
    /// <param name="createdBy">Identificador do usuário que criou o lançamento.</param>
    /// <returns>Instância reidratada do agregado.</returns>
    public static Transaction Restore(
        Guid id,
        string description,
        decimal amount,
        EntryType entryType,
        DateOnly transactionDate,
        Guid userId,
        DateTime createdAt,
        Guid createdBy)
    {
        return new Transaction
        {
            Id = id,
            Description = description,
            Amount = amount,
            EntryType = entryType,
            TransactionDate = transactionDate,
            UserId = userId,
            CreatedAt = createdAt,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Retorna o impacto signed do lançamento no saldo (+ crédito, - débito).
    /// </summary>
    /// <returns>Valor com sinal aplicado ao saldo consolidado.</returns>
    public decimal SignedAmount() =>
        EntryType == EntryType.Credit ? Amount : -Amount;

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }
    }
}
